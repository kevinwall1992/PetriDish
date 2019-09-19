using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;

public interface Catalyst : Copiable<Catalyst>, Stackable, Encodable
{
    //Same across all instances
    string Name { get; }
    string Description { get; }
    int Price { get; }

    Example Example { get; }

    int Power { get; }

    Dictionary<Cell.Slot.Relation, Attachment> Attachments { get; }

    //State
    Cell.Slot.Relation Orientation { get; set; }
    IEnumerable<Compound> Cofactors { get; }


    T GetFacet<T>() where T : class, Catalyst;

    Cell.Slot.Relation GetAttachmentDirection(Attachment attachment);

    void RotateLeft();
    void RotateRight();

    bool CanAddCofactor(Compound cofactor);
    void AddCofactor(Compound cofactor);

    void Step(Cell.Slot slot);
    void Communicate(Cell.Slot slot, Action.Stage stage);
    Action Catalyze(Cell.Slot slot, Action.Stage stage);

    Catalyst Mutate();

    //Essentially, .Equals() without state
    //(We ignore orientation and cofactors)
    bool IsSame(Catalyst other);
}

public abstract class Attachment
{
    public Cell.Slot GetSlotPointedAt(Cell.Slot catalyst_slot)
    {
        if (catalyst_slot.Compound == null)
            return null;

        Catalyst catalyst = catalyst_slot.Compound.Molecule as Catalyst;
        if (catalyst == null)
            return null;

        return catalyst_slot.GetAdjacentSlot(catalyst.GetAttachmentDirection(this));
    }
}

public class InputAttachment : Attachment
{
    public Molecule Molecule { get; private set; }

    public InputAttachment(Molecule molecule = null)
    {
        Molecule = molecule;
    }

    public Compound Take(Cell.Slot catalyst_slot, float quantity)
    {
        Cell.Slot slot = GetSlotPointedAt(catalyst_slot);
        if (slot == null)
            return null;

        return slot.Compound.Split(quantity);
    }
}

public class OutputAttachment : Attachment
{
    public Molecule Molecule { get; private set; }

    public OutputAttachment(Molecule molecule = null)
    {
        Molecule = molecule;
    }

    public void Put(Cell.Slot catalyst_slot, Compound compound)
    {
        Cell.Slot slot = GetSlotPointedAt(catalyst_slot);
        if (slot == null || (slot.Compound != null && !slot.Compound.Molecule.IsStackable(compound.Molecule)))
            return;

        slot.AddCompound(compound);
    }
}

//This Catalyst executes the action in its entirety 
//all at once, and therefore may have to wait several
//turns building up to it. 
public abstract class ProgressiveCatalyst : Catalyst
{
    List<Compound> cofactors = new List<Compound>();
    Dictionary<string, object> aspects = new Dictionary<string, object>();

    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Price { get; private set; }
    bool is_initialized = false;

    public virtual Example Example { get { return null; } }

    public abstract int Power { get; }

    public Dictionary<Cell.Slot.Relation, Attachment> Attachments { get; private set; }

    //Orientation describes the direction the 
    //"front" of the Catalyst is pointing
    public Cell.Slot.Relation Orientation { get; set; }
    public IEnumerable<Compound> Cofactors { get { return cofactors; } }

    public float Progress { get; set; }

    public ProgressiveCatalyst(string name, int price, string description = "")
    {
        Initialize(name, price, description);

        Attachments = new Dictionary<Cell.Slot.Relation, Attachment>();

        Orientation = Cell.Slot.Relation.Across;
    }

    protected ProgressiveCatalyst()
    {
        Name = "Uninitialized";
        Description = "";
        Price = 0;

        Attachments = new Dictionary<Cell.Slot.Relation, Attachment>();

        Orientation = Cell.Slot.Relation.Across;
    }

    protected void Initialize(string name, int price, string description)
    {
        if(is_initialized)
        {
            Debug.Assert(false);
            return;
        }

        Name = name;
        Description = description;
        Price = price;

        is_initialized = true;
    }

    protected abstract Action GetAction(Cell.Slot slot);

    public virtual void Step(Cell.Slot slot)
    {
        Progress += slot.Compound.Quantity;
    }

    public virtual void Communicate(Cell.Slot slot, Action.Stage stage)
    {
        Action action = GetAction(slot);
        if (action == null || !stage.Includes(action) || !action.IsLegal)
            return;

        Dictionary<object, List<Compound>> demands = action.GetResourceDemands();
        foreach (object source in demands.Keys)
            foreach (Compound compound in demands[source])
                MakeClaim(source, compound);
    }

    public virtual Action Catalyze(Cell.Slot slot, Action.Stage stage)
    {
        Action action = GetAction(slot);
        if (action == null)
            return null;

        if (!stage.Includes(action))
            return null;

        if (GetNormalizedClaimYield() < 1)
            return null;

        if (!action.IsLegal)
        {
            Progress = 0;
            return null;
        }

        if(Progress>= action.Cost)
        {
            Progress = 0;
            return action;
        }

        return null;
    }

    protected static T GetMoleculeInSlotAs<T>(Cell.Slot slot) where T : Molecule
    {
        if (slot.Compound == null)
            return null;

        return slot.Compound.Molecule as T;
    }

    public virtual Catalyst Mutate()
    {
        if (MathUtility.Roll(0.9f))
            return this;
        else
            return MathUtility.RandomElement(Utility.CreateList<Catalyst>(
                new Interpretase(),
                new Constructase(),
                new Transcriptase()));
    }

    public T GetFacet<T>() where T : class, Catalyst
    {
        return this as T;
    }

    public Cell.Slot.Relation GetAttachmentDirection(Attachment attachment)
    {
        foreach (Cell.Slot.Relation direction in Enum.GetValues(typeof(Cell.Slot.Relation)))
            if (Attachments.ContainsKey(direction) && Attachments[direction] == attachment)
                return ApplyOrientation(direction);

        return Cell.Slot.Relation.None;
    }

    public void RotateLeft()
    {
        Orientation = Cell.Slot.RotateRelation(Orientation, false);
    }

    public void RotateRight()
    {
        Orientation = Cell.Slot.RotateRelation(Orientation, true);
    }

    public Cell.Slot.Relation ApplyOrientation(Cell.Slot.Relation direction)
    {
        return (Cell.Slot.Relation)MathUtility.Mod((int)direction + (int)Orientation, 3);
    }

    public virtual bool CanAddCofactor(Compound cofactor)
    {
        return false;
    }

    public void AddCofactor(Compound cofactor)
    {
        if (!CanAddCofactor(cofactor))
            return;

        foreach (Compound compound in cofactors)
            if (compound.Molecule.Equals(cofactor.Molecule))
            {
                compound.Quantity += cofactor.Quantity;
                return;
            }

        cofactors.Add(cofactor);
    }


    public virtual bool IsSame(Catalyst other)
    {
        if (this == other)
            return true;

        return GetType() == other.GetType();
    }

    public virtual bool IsStackable(object obj)
    {
        if (!(obj is Catalyst))
            return false;

        Catalyst other = obj as Catalyst;

        if (!IsSame(other))
            return false;

        return Utility.SetEquality(Cofactors, other.Cofactors);
    }

    public override bool Equals(object obj)
    {
        if (!IsStackable(obj))
            return false;

        Catalyst other = obj as Catalyst;

        if (Orientation != other.Orientation)
            return false;

        if (Progress != (other as ProgressiveCatalyst).Progress)
            return false;

        return true;
    }

    public abstract Catalyst Copy();

    protected virtual ProgressiveCatalyst CopyStateFrom(ProgressiveCatalyst other)
    {
        Orientation = other.Orientation;

        foreach (Compound cofactor in other.cofactors)
            cofactors.Add(cofactor.Copy());

        Progress = other.Progress;

        return this;
    }


    public virtual JObject EncodeJson()
    {
        JArray json_cofactor_array = new JArray();

        foreach (Compound cofactor in Cofactors)
            json_cofactor_array.Add(cofactor.EncodeJson());

        return JObject.FromObject(Utility.CreateDictionary<string, object>(
            "Type", GetType().Name, 
            "Orientation", System.Enum.GetName(typeof(Cell.Slot.Relation), Orientation),
            "Cofactors", json_cofactor_array));
    }

    public virtual void DecodeJson(JObject json_object)
    {
        switch(Utility.JTokenToString(json_object["Orientation"]))
        {
            case "Right": Orientation = Cell.Slot.Relation.Right; break;
            case "Left": Orientation = Cell.Slot.Relation.Left; break;
            case "Across": Orientation = Cell.Slot.Relation.Across; break;
        }

        foreach(JToken json_cofactor_token in json_object["Cofactors"])
            AddCofactor(Compound.DecodeCompound(json_cofactor_token as JObject));
    }

    public static Catalyst DecodeCatalyst(JObject json_object)
    {
        Catalyst catalyst;

        switch (Utility.JTokenToString(json_object["Type"]))
        {
            case "Constructase": catalyst = new Constructase(); break;
            case "Separatase": catalyst = new Separatase(); break;
            case "Pumpase": catalyst = new Pumpase(); break;
            case "Porin": catalyst = new Porin(null); break;
            case "Transcriptase": catalyst = new Transcriptase(); break;
            case "Interpretase": catalyst = new Interpretase(); break;

            case "ReactionCatalyst": catalyst = Reaction.CreateBlankCatalyst(); break;

            default: throw new System.NotImplementedException();
        }

        catalyst.DecodeJson(json_object);

        return catalyst;
    }


    protected struct Claim
    {
        Catalyst claimant;

        object source;
        Compound compound;

        public Catalyst Claimant { get { return claimant; } }

        public object Source { get { return source; } }
        public Compound Resource { get { return compound; } }

        public Claim(Catalyst claimant_, object source_, Compound compound_)
        {
            claimant = claimant_;

            source = source_;
            compound = compound_;
        }
    }

    static Dictionary<object, List<Claim>> sources = new Dictionary<object, List<Claim>>();
    static Dictionary<Catalyst, List<Claim>> claimants = new Dictionary<Catalyst, List<Claim>>();

    static Dictionary<Catalyst, float> normalized_claim_yields = null;

    protected void MakeClaim(object source, Compound compound)
    {
        if (normalized_claim_yields != null)
        {
            sources.Clear();
            claimants.Clear();
            normalized_claim_yields = null;
        }

        if (!claimants.ContainsKey(this))
            claimants[this] = new List<Claim>();
        
        if (!sources.ContainsKey(source))
            sources[source] = new List<Claim>();

        Claim claim = new Claim(this, source, compound);
        sources[source].Add(claim);
        claimants[this].Add(claim);
    }

    class Availability
    {
        public float available_quantity, unclaimed_quantity, demanded_quantity;

        public float UnclaimedToDemandedRatio
        {
            get { return Mathf.Min(1, unclaimed_quantity / demanded_quantity); }
        }

        public float UnclaimedToAvailableRatio
        {
            get { return Mathf.Min(1, unclaimed_quantity / available_quantity); }
        }

        public Availability(float available_quantity_)
        {
            available_quantity = unclaimed_quantity = available_quantity_;
            demanded_quantity = 0;
        }
    }

    protected float GetNormalizedClaimYield()
    {
        if (normalized_claim_yields == null)
        {
            normalized_claim_yields = new Dictionary<Catalyst, float>();

            Dictionary<object, Dictionary<Molecule, Availability>> availablities =
                new Dictionary<object, Dictionary<Molecule, Availability>>();

            List<Catalyst> processed_claimants = new List<Catalyst>();

            //In in each iteration, sum the demands of remaining claimants, 
            //scale demands down to fit within resource availability,
            //Subtract this final demanded quantity from availability,
            //And remove claimants that demanded any now depleted resource.
            while (processed_claimants.Count < claimants.Count)
            {
                //Clear demands
                foreach (object source in availablities.Keys)
                    foreach (Molecule molecule in availablities[source].Keys)
                        availablities[source][molecule].demanded_quantity = 0;

                //Set up initial availabilities, set up this iteration's demands
                foreach (Catalyst claimant in claimants.Keys)
                {
                    if (processed_claimants.Contains(claimant))
                        continue;

                    foreach (Claim claim in claimants[claimant])
                    {
                        Molecule molecule = claim.Resource.Molecule;

                        if (!availablities.ContainsKey(claim.Source))
                            availablities[claim.Source] = new Dictionary<Molecule, Availability>();

                        if (!availablities[claim.Source].ContainsKey(molecule))
                        {
                            float available_quantity = 0;

                            if (claim.Source is Cell.Slot)
                            {
                                Cell.Slot slot = claim.Source as Cell.Slot;

                                if (slot.Compound != null)
                                    available_quantity = slot.Compound.Quantity;
                            }
                            else if (claim.Source is Cytosol)
                                available_quantity = (claim.Source as Cytosol).GetQuantity(molecule);
                            else if (claim.Source is WaterLocale)
                                available_quantity = (claim.Source as WaterLocale).Solution.GetQuantity(molecule);

                            availablities[claim.Source][molecule] = new Availability(available_quantity);
                        }

                        availablities[claim.Source][molecule].demanded_quantity += claim.Resource.Quantity;
                    }
                }

                //Remove claimants that must have all their resources at once,
                //but sharing is required. (their claim yield is therefore 0)
                //Remove their demands as well. 
                foreach (Catalyst claimant in claimants.Keys)
                {
                    if (claimant is InstantCatalyst)
                        continue;

                    bool cannot_execute_action = false;

                    foreach (Claim claim in claimants[claimant])
                        if (availablities[claim.Source][claim.Resource.Molecule].UnclaimedToDemandedRatio < 1)
                        {
                            cannot_execute_action = true;
                            break;
                        }

                    if (cannot_execute_action)
                    {
                        foreach (Claim claim in claimants[claimant])
                            availablities[claim.Source][claim.Resource.Molecule].demanded_quantity -= claim.Resource.Quantity;

                        normalized_claim_yields[claimant] = 0;
                        processed_claimants.Add(claimant);
                    }
                }

                //Compute actual demands based on availability
                //f.e. 1 unit available, 1 unit demanded from 3 claimants:
                //1/3 unit given to each claimant. If a claimant demands more than
                //the other claimants, it gets proportionally more of supply. 
                Dictionary<Claim, float> quantity_claimed = new Dictionary<Claim, float>();
                foreach (Catalyst claimant in claimants.Keys)
                {
                    if (processed_claimants.Contains(claimant))
                        continue;

                    float smallest_ratio = 1;

                    foreach (Claim claim in claimants[claimant])
                    {
                        float ratio = availablities[claim.Source][claim.Resource.Molecule].UnclaimedToDemandedRatio;

                        smallest_ratio = Mathf.Min(smallest_ratio, ratio);
                    }

                    if (smallest_ratio == 1)
                        processed_claimants.Add(claimant);

                    normalized_claim_yields[claimant] = smallest_ratio;

                    foreach (Claim claim in claimants[claimant])
                    {
                        if (smallest_ratio == 0)
                            quantity_claimed[claim] = 0;
                        else
                            quantity_claimed[claim] =
                                claim.Resource.Quantity *
                                smallest_ratio *
                                availablities[claim.Source][claim.Resource.Molecule].UnclaimedToAvailableRatio;
                    }
                }

                //Remove claimants who got everything they wanted
                foreach (Catalyst claimant in claimants.Keys)
                {
                    if (processed_claimants.Contains(claimant))
                        continue;

                    foreach (Claim claim in claimants[claimant])
                        availablities[claim.Source][claim.Resource.Molecule].unclaimed_quantity -= quantity_claimed[claim];

                    if (normalized_claim_yields[claimant] == 1)
                        processed_claimants.Add(claimant);
                }

                //Remove claimants who demanded a now depleted resource.
                foreach (Catalyst claimant in claimants.Keys)
                {
                    if (processed_claimants.Contains(claimant))
                        continue;

                    foreach (Claim claim in claimants[claimant])
                    {
                        Molecule molecule = claim.Resource.Molecule;

                        if (availablities[claim.Source][molecule].unclaimed_quantity < 0.0000001f)
                        {
                            processed_claimants.Add(claimant);
                            continue;
                        }
                    }
                }
            }
        }

        if (!normalized_claim_yields.ContainsKey(this))
            return 1;

        return normalized_claim_yields[this];
    }
}

//This Catalyst always returns an action (if able), 
//scaling the effect of the action to fit the 
//productivity of the Catalyst. 
public abstract class InstantCatalyst : ProgressiveCatalyst
{
    public InstantCatalyst(string name, int price, string description = "") : base(name, price, description)
    {
        
    }

    protected InstantCatalyst()
    {

    }

    public override void Communicate(Cell.Slot slot, Action.Stage stage)
    {
        Action action = GetAction(slot);
        if (action == null || !stage.Includes(action) || !action.IsLegal)
            return;

        action.Cost = slot.Compound.Quantity;

        Dictionary<object, List<Compound>> demands = action.GetResourceDemands();
        foreach (object source in demands.Keys)
            foreach (Compound compound in demands[source])
                MakeClaim(source, compound);
    }

    //Enforce productivity from above?
    public override Action Catalyze(Cell.Slot slot, Action.Stage stage)
    {
        Action action = GetAction(slot);

        if (action == null)
            return null;

        if (!stage.Includes(action))
            return null;

        if (!action.IsLegal)
            return null;

        action.Cost = slot.Compound.Quantity;

        float claim_yield = GetNormalizedClaimYield();
        if (claim_yield == 0)
            return null;
        action.Scale *= claim_yield;

        return action;
    }
}


public class Constructase : ProgressiveCatalyst
{
    public override int Power { get { return 7; } }

    public InputAttachment Feed { get; private set; }
    public Extruder Extruder { get; private set; }
    public float RequiredQuantity { get { return 4; } }

    public Constructase() : base("Constructase", 1, "Makes new cells")
    {
        Attachments[Cell.Slot.Relation.Left] = Feed = new InputAttachment(Molecule.GetMolecule("Structate"));
        Attachments[Cell.Slot.Relation.Across] = Extruder = new Extruder();
    }

    protected override Action GetAction(Cell.Slot slot)
    {
        return new ConstructCell(slot);
    }

    public override Catalyst Copy()
    {
        return new Constructase().CopyStateFrom(this);
    }


    public class ConstructCell : EnergeticAction
    {
        public override bool IsLegal
        {
            get
            {
                if (Constructase.Extruder.GetSlotPointedAt(CatalystSlot) != null)
                    return false;

                Cell.Slot feed_slot = Constructase.Feed.GetSlotPointedAt(CatalystSlot);
                if (feed_slot == null ||
                    feed_slot.Compound == null ||
                    !feed_slot.Compound.Molecule.IsStackable(Constructase.Feed.Molecule))
                    return false;

                return base.IsLegal;
            }
        }

        public Compound Feedstock { get; private set; }

        public Constructase Constructase { get { return Catalyst.GetFacet<Constructase>(); } }

        public ConstructCell(Cell.Slot catalyst_slot)
            : base(catalyst_slot, 2, -2.0f)
        {
            
        }

        public override Dictionary<object, List<Compound>> GetResourceDemands()
        {
            Dictionary<object, List<Compound>> demands = base.GetResourceDemands();

            Cell.Slot slot = Constructase.Feed.GetSlotPointedAt(CatalystSlot);
            demands[slot] = Utility.CreateList(new Compound(Constructase.Feed.Molecule, Constructase.RequiredQuantity));

            return demands;
        }

        public override void Begin()
        {
            base.Begin();

            Feedstock = Constructase.Feed.Take(CatalystSlot, Constructase.RequiredQuantity);
        }

        public override void End()
        {
            if (Cell.GetAdjacentCell(CatalystSlot.Direction) == null)
                Organism.AddCell(Cell, CatalystSlot.Direction);

            //A cell is unexpectedly in the way
            else;

            base.End();
        }
    }
}

public class Extruder : Attachment { }


public class Separatase : ProgressiveCatalyst
{
    public Separator Separator { get; private set; }

    public override int Power { get { return 10; } }

    public Separatase() : base("Separatase", 1, "Separates cells from one another")
    {
        Attachments[Cell.Slot.Relation.Across] = Separator = new Separator();
    }

    protected override Action GetAction(Cell.Slot slot)
    {
        return new SeparateCell(slot);
    }

    public override Catalyst Copy()
    {
        return new Separatase().CopyStateFrom(this);
    }


    public class SeparateCell : EnergeticAction
    {
        public Compound SeedCompound { get; private set; }

        public override bool IsLegal
        {
            get
            {
                if (CatalystSlot.AdjacentCell == null)
                    return false;

                if (Cytosol.GetQuantity(ChargeableMolecule.NRG) < (EnergyBalance + 10))
                    return false;

                return base.IsLegal;
            }
        }

        public SeparateCell(Cell.Slot catalyst_slot)
            : base(catalyst_slot, 4, -4)
        {

        }

        public override Dictionary<object, List<Compound>> GetResourceDemands()
        {
            Dictionary<object, List<Compound>> demands = base.GetResourceDemands();

            demands[Cytosol].Add(new Compound(Molecule.NRG, 10));

            return demands;
        }

        public override void Begin()
        {
            if (!IsLegal)
                return;

            base.Begin();

            SeedCompound = Cytosol.RemoveCompound(Molecule.NRG, 10);
        }

        public override void End()
        {
            base.End();

            Organism.Separate(Cell, CatalystSlot.AdjacentCell);

            Organism.Cytosol.AddCompound(SeedCompound);
        }
    }
}

public class Separator : Attachment { }


public class Pumpase : InstantCatalyst
{
    bool is_isomer;

    public Molecule Molecule { get; private set; }

    public InputAttachment InPump { get; private set; }
    public OutputAttachment OutPump { get; private set; }

    public override int Power { get { return 6; } }


    Pumpase(Molecule molecule, bool is_isomer_)
        : base()
    {
        is_isomer = is_isomer_;
        Molecule = molecule;

        

        DefaultInitialization();
    }

    public Pumpase()
    {
        
    }

    void InitializeAttachments()
    {
        InPump = new InputAttachment(Molecule);
        OutPump = new OutputAttachment(Molecule);

        if (!is_isomer)
        {
            Attachments[Cell.Slot.Relation.Across] = InPump;
            Attachments[Cell.Slot.Relation.Right] = OutPump;
        }
        else
        {
            Attachments[Cell.Slot.Relation.Across] = OutPump;
            Attachments[Cell.Slot.Relation.Right] = InPump;
        }
    }

    void DefaultInitialization()
    {
        base.Initialize("Pumpase", 1, "Exchanges compounds with environment");
    }

    protected override Action GetAction(Cell.Slot slot)
    {
        if (Molecule == null && slot.Compound == null)
            return null;

        Cell.Slot input_slot = InPump.GetSlotPointedAt(slot);
        Cell.Slot output_slot = OutPump.GetSlotPointedAt(slot);

        object source = input_slot != null ? (object)input_slot : slot.Cell.Organism.Locale;
        object destination = output_slot != null ? (object)output_slot : slot.Cell.Organism.Locale;


        return new PumpAction(slot, Molecule, source, destination, 1);
    }

    public override Catalyst Mutate()
    {
        if(MathUtility.Roll(0.1f))
            return base.Mutate();
        else
        {
            if (MathUtility.Roll(0.9f))
                return new Pumpase(Molecule, !is_isomer);
            else
                return new Pumpase(GetRandomMolecule(), is_isomer);
        }
    }

    public override bool IsSame(Catalyst other)
    {
        if (!base.IsSame(other))
            return false;

        Pumpase other_pumpase = other as Pumpase;

        return other_pumpase.is_isomer == is_isomer &&
               other_pumpase.Molecule.Equals(Molecule);
    }

    public override Catalyst Copy()
    {
        return new Pumpase(Molecule, is_isomer).CopyStateFrom(this);
    }

    public override JObject EncodeJson()
    {
        JObject json_catalyst_object = base.EncodeJson();

        json_catalyst_object["Is Isomer"] = is_isomer;
        json_catalyst_object["Molecule"] = Molecule.Name;

        return json_catalyst_object;
    }

    public override void DecodeJson(JObject json_object)
    {
        base.DecodeJson(json_object);

        is_isomer = Utility.JTokenToBool(json_object["Is Isomer"]);
        Molecule = Molecule.GetMolecule(Utility.JTokenToString(json_object["Molecule"]));

        InitializeAttachments();
        DefaultInitialization();
    }


    static Molecule GetRandomMolecule()
    {
        Dictionary<Molecule, float> weighted_molecules =
            Utility.CreateDictionary<Molecule, float>(Molecule.GetMolecule("Hindenburgium Gas"), 10.0f,
                                                      Molecule.GetMolecule("Umamium Gas"), 10.0f,
                                                      Molecule.GetMolecule("Karbon Diaeride"), 10.0f,
                                                      Molecule.GetMolecule("Hindenburgium Stankide"), 10.0f);

        foreach (Molecule molecule in Molecule.Molecules)
            if (!weighted_molecules.ContainsKey(molecule))
                weighted_molecules[molecule] = 1;

        return MathUtility.RandomElement(weighted_molecules);
    }
}

public class PumpAction : EnergeticAction
{
    Molecule molecule;
    float base_quantity;

    Pumpase Pumpase { get { return Catalyst.GetFacet<Pumpase>(); } }

    public object Source { get; private set; }
    public object Destination { get; private set; }

    public override bool IsLegal
    {
        get
        {
            if (!CatalystSlot.IsExposed)
                return false;

            return base.IsLegal;
        }
    }

    public Compound PumpedCompound { get; private set; }

    public PumpAction(Cell.Slot catalyst_slot, 
                      Molecule molecule_, object source, object destination, float rate) 
        : base(catalyst_slot, 1, 0.1f)
    {
        molecule = molecule_;

        Source = source;
        Destination = destination;

        base_quantity = Organism.Membrane.GetTransportRate(molecule, Destination is Locale) * 
                        CatalystSlot.Compound.Quantity * rate;
    }

    public override Dictionary<object, List<Compound>> GetResourceDemands()
    {
        Dictionary<object, List<Compound>> demands = base.GetResourceDemands();

        if (!demands.ContainsKey(Source))
            demands[Source] = new List<Compound>();
        demands[Source].Add(new Compound(molecule, base_quantity * Scale));

        return demands;
    }

    public override void Begin()
    {
        base.Begin();

        float final_quantity = base_quantity * Scale;

        if (Source is Cell.Slot)
            PumpedCompound = (Source as Cell.Slot).Compound.Split(final_quantity);
        else
        {
            Solution source_solution = Source is Cytosol ? Source as Cytosol : (Source as WaterLocale).Solution;
            PumpedCompound = source_solution.RemoveCompound(new Compound(molecule, final_quantity));
        }
    }

    public override void End()
    {
        if (Destination is Cell.Slot)
        {
            Cell.Slot destination_slot = Destination as Cell.Slot;

            if (destination_slot.Compound != null && !destination_slot.Compound.Molecule.IsStackable(molecule))
                destination_slot.AddCompound(new Mess(destination_slot.RemoveCompound(), PumpedCompound), 1);
            else
                destination_slot.AddCompound(PumpedCompound);
        }
        else
        {
            Solution destination_solution = Destination is Cytosol ? Destination as Cytosol : (Destination as WaterLocale).Solution;
            destination_solution.AddCompound(PumpedCompound);
        }

        base.End();
    }
}


public class Porin : InstantCatalyst
{
    float size;

    public Molecule Molecule { get; private set; }

    public override int Power { get { return 6; } }


    public Porin(Molecule molecule, float size_ = 1)
        : base("Porin", 1, "Equalizes the concentration of a substance with the environment.")
    {
        Molecule = molecule;
        size = size_;
    }

    protected override Action GetAction(Cell.Slot slot)
    {
        if (Molecule == null && slot.Compound == null)
            return null;

        float cytosol_concentration = slot.Cell.Organism.Cytosol.GetConcentration(Molecule);
        float locale_concentration = (slot.Cell.Organism.Locale as WaterLocale).Solution.GetConcentration(Molecule);

        object source, destination;
        if(cytosol_concentration > locale_concentration)
        {
            source = slot.Cell.Organism.Cytosol;
            destination = slot.Cell.Organism.Locale;
        }
        else
        {
            source = slot.Cell.Organism.Locale;
            destination = slot.Cell.Organism.Cytosol;
        }

        return new PumpAction(slot, Molecule, source, destination, size);
    }

    public override Catalyst Copy()
    {
        return new Porin(Molecule, size).CopyStateFrom(this);
    }

    public override JObject EncodeJson()
    {
        JObject json_catalyst_object = base.EncodeJson();

        json_catalyst_object["Size"] = size;
        json_catalyst_object["Molecule"] = Molecule.Name;

        return json_catalyst_object;
    }

    public override void DecodeJson(JObject json_object)
    {
        base.DecodeJson(json_object);

        Molecule = Molecule.GetMolecule(Utility.JTokenToString(json_object["Molecule"]));
        size = Utility.JTokenToFloat(json_object["Size"]);
    }
}


public class Transcriptase : InstantCatalyst
{
    public override int Power { get { return 9; } }

    public InputAttachment Feed { get; private set; }
    public Transcriptor Transcriptor { get; private set; }

    public Transcriptase() : base("Transcriptase", 3, "Copies DNA")
    {
        Attachments[Cell.Slot.Relation.Left] = Feed = new InputAttachment(Molecule.GetMolecule("Genes"));
        Attachments[Cell.Slot.Relation.Right] = Transcriptor = new Transcriptor();
    }

    protected override Action GetAction(Cell.Slot slot)
    {
        DNA dna = Transcriptor.GetDNA(slot);
        float cost = dna.Monomers.Count / (100.0f);

        return new ReactionAction(
                slot,
                Utility.CreateDictionary<Cell.Slot, Compound>(Feed.GetSlotPointedAt(slot), new Compound(Feed.Molecule, dna.Monomers.Count / 100.0f)),
                Utility.CreateDictionary<Cell.Slot, Compound>(Transcriptor.GetSlotPointedAt(slot), new Compound(dna, 1)),
                null,
                Utility.CreateList(new Compound(Molecule.Water, (dna.Monomers.Count - 1) / 100.0f)),
                null, null,
                cost * 3,
                cost);
    }

    public override Catalyst Copy()
    {
        return new Transcriptase().CopyStateFrom(this);
    }


    
}

public class Transcriptor : Attachment
{
    public Transcriptor()
    {

    }

    public DNA GetDNA(Cell.Slot catalyst_slot)
    {
        Compound compound = GetSlotPointedAt(catalyst_slot).Compound;
        if (compound == null)
            return null;

        return compound.Molecule as DNA;
    }
}
