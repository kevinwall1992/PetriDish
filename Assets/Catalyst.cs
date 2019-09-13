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
        return (Cell.Slot.Relation)(((int)direction + (int)Orientation) % 3);
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


    public virtual bool IsStackable(object obj)
    {
        if (this == obj)
            return true;

        if (GetType() != obj.GetType())
            return false;

        Catalyst other = obj as Catalyst;

        foreach (Compound compound in cofactors)
            if (!other.Cofactors.Contains(compound))
                return false;

        foreach (Compound compound in other.Cofactors)
            if (!Cofactors.Contains(compound))
                return false;

        return true;
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


    public virtual string EncodeString() { return EncodeJson().ToString(); }
    public virtual void DecodeString(string string_encoding) { DecodeJson(JObject.Parse(string_encoding)); }

    public virtual JObject EncodeJson()
    {
        JArray json_cofactor_array = new JArray();

        foreach (Compound cofactor in Cofactors)
            json_cofactor_array.Add(JObject.FromObject(Utility.CreateDictionary<string, object>("Molecule", cofactor.Molecule.EncodeJson(), 
                                                                                                "Quantity", cofactor.Quantity)));

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

        foreach(JToken json_token in json_object["Cofactors"])
        {
            JObject json_cofactor_object = json_token as JObject;

            AddCofactor(new Compound(Molecule.DecodeMolecule(json_cofactor_object["Molecule"] as JObject), 
                                     Utility.JTokenToFloat(json_cofactor_object["Quantity"])));
        }
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

            List<Catalyst> satisfied_claimants = new List<Catalyst>();

            int foo = 0;
            while (satisfied_claimants.Count < claimants.Count && foo++ < 100)
            {
                foreach (object source in availablities.Keys)
                    foreach (Molecule molecule in availablities[source].Keys)
                        availablities[source][molecule].demanded_quantity = 0;

                foreach (Catalyst claimant in claimants.Keys)
                {
                    if (satisfied_claimants.Contains(claimant))
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

                List<Catalyst> pissed_off_claimants = new List<Catalyst>();
                foreach (Catalyst claimant in claimants.Keys)
                {
                    if (claimant is InstantCatalyst)
                        continue;

                    foreach (Claim claim in claimants[claimant])
                        if (availablities[claim.Source][claim.Resource.Molecule].UnclaimedToDemandedRatio < 1)
                        {
                            pissed_off_claimants.Add(claimant);
                            break;
                        }
                }
                foreach (Catalyst claimant in pissed_off_claimants)
                {
                    foreach (Claim claim in claimants[claimant])
                        availablities[claim.Source][claim.Resource.Molecule].demanded_quantity -= claim.Resource.Quantity;

                    claimants.Remove(claimant);
                }

                Dictionary<Claim, float> quantity_claimed = new Dictionary<Claim, float>();
                foreach (Catalyst claimant in claimants.Keys)
                {
                    if (satisfied_claimants.Contains(claimant))
                        continue;

                    float smallest_ratio = 1;

                    foreach (Claim claim in claimants[claimant])
                    {
                        float ratio = availablities[claim.Source][claim.Resource.Molecule].UnclaimedToDemandedRatio;

                        smallest_ratio = Mathf.Min(smallest_ratio, ratio);
                    }

                    if (smallest_ratio == 1)
                        satisfied_claimants.Add(claimant);

                    normalized_claim_yields[claimant] = smallest_ratio;

                    foreach (Claim claim in claimants[claimant])
                        quantity_claimed[claim] =
                            claim.Resource.Quantity *
                            smallest_ratio *
                            availablities[claim.Source][claim.Resource.Molecule].UnclaimedToAvailableRatio;
                }

                foreach (Catalyst claimant in claimants.Keys)
                {
                    if (satisfied_claimants.Contains(claimant))
                        continue;

                    foreach (Claim claim in claimants[claimant])
                        availablities[claim.Source][claim.Resource.Molecule].unclaimed_quantity -= quantity_claimed[claim];

                    if (normalized_claim_yields[claimant] == 1)
                        satisfied_claimants.Add(claimant);
                }

                foreach (Catalyst claimant in claimants.Keys)
                {
                    if (satisfied_claimants.Contains(claimant))
                        continue;

                    foreach (Claim claim in claimants[claimant])
                    {
                        Molecule molecule = claim.Resource.Molecule;

                        if (availablities[claim.Source][molecule].unclaimed_quantity < 0.0000001f)
                        {
                            satisfied_claimants.Add(claimant);
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
        if (!stage.Includes(action))
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

        action.Cost = slot.Compound.Quantity;
        action.Scale *= GetNormalizedClaimYield();

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
        Attachments[Cell.Slot.Relation.Across] = Feed = new InputAttachment(Molecule.GetMolecule("Structate"));
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
                if ((Catalyst as Constructase).Extruder.GetSlotPointedAt(CatalystSlot) != null)
                    return false;

                Cell.Slot feed_slot = (Catalyst as Constructase).Feed.GetSlotPointedAt(CatalystSlot);
                if (feed_slot == null ||
                    feed_slot.Compound == null ||
                    !feed_slot.Compound.Molecule.IsStackable((Catalyst as Constructase).Feed.Molecule))
                    return false;

                return base.IsLegal;
            }
        }

        public Compound Feedstock { get; private set; }

        public ConstructCell(Cell.Slot catalyst_slot)
            : base(catalyst_slot, 2, -2.0f)
        {
            
        }

        public override Dictionary<object, List<Compound>> GetResourceDemands()
        {
            Dictionary<object, List<Compound>> demands = base.GetResourceDemands();

            Constructase constructase = Catalyst as Constructase;
            demands[constructase.Feed.GetSlotPointedAt(CatalystSlot)]
                .Add(new Compound(constructase.Feed.Molecule, constructase.RequiredQuantity));

            return demands;
        }

        public override void Begin()
        {
            Constructase constructase = Catalyst as Constructase;

            Feedstock = constructase.Feed.Take(CatalystSlot, constructase.RequiredQuantity);
        }

        public override void End()
        {
            base.Begin();

            if (Cell.GetAdjacentCell(CatalystSlot.Direction) == null)
                Organism.AddCell(Cell, CatalystSlot.Direction);

            //A cell is unexpectedly in the way
            else;
        }
    }
}

public class Extruder : Attachment { }


public class Separatase : ProgressiveCatalyst
{
    public override int Power { get { return 10; } }

    public Separatase() : base("Separatase", 1, "Separates cells from one another")
    {

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

public class Pumpase : InstantCatalyst
{
    bool pump_out;

    public Molecule Molecule { get; private set; }

    public override int Power { get { return 6; } }


    Pumpase(bool pump_out_, Molecule molecule) 
        : base()
    {
        pump_out = pump_out_;
        Molecule = molecule;

        DefaultInitialization();
    }

    public Pumpase()
    {

    }

    void DefaultInitialization()
    {
        base.Initialize(pump_out ? "Exopumpase" : "Endopumpase", 1, "Draws in compounds from outside");
    }

    protected override Action GetAction(Cell.Slot slot)
    {
        if (Molecule == null && slot.Compound == null)
            return null;

        return new PumpAction(slot, pump_out, Molecule, 1);
    }

    public override Catalyst Mutate()
    {
        if(MathUtility.Roll(0.1f))
            return base.Mutate();
        else
        {
            if (MathUtility.Roll(0.9f))
                return new Pumpase(pump_out, GetRandomMolecule());
            else
                return new Pumpase(!pump_out, Molecule);
        }
    }

    public override bool IsStackable(object obj)
    {
        if (!base.IsStackable(obj))
            return false;

        Pumpase other = obj as Pumpase;

        return other.pump_out == pump_out &&
               other.Molecule.Equals(Molecule);
    }

    public override Catalyst Copy()
    {
        return new Pumpase(pump_out, Molecule).CopyStateFrom(this);
    }

    public override JObject EncodeJson()
    {
        JObject json_catalyst_object = base.EncodeJson();

        json_catalyst_object["Direction"] = pump_out ? "Out" : "In";
        json_catalyst_object["Molecule"] = Molecule.Name;

        return json_catalyst_object;
    }

    public override void DecodeJson(JObject json_object)
    {
        base.DecodeJson(json_object);

        pump_out = Utility.JTokenToString(json_object["Direction"]) == "Out";
        Molecule = Molecule.GetMolecule(Utility.JTokenToString(json_object["Molecule"]));

        DefaultInitialization();
    }


    public static Pumpase Endo(Molecule molecule)
    {
        return new Pumpase(false, molecule);
    }

    public static Pumpase Exo(Molecule molecule)
    {
        return new Pumpase(true, molecule);
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
    bool pump_out;
    Molecule molecule;
    float base_quantity;

    Solution Source
    {
        get
        {
            if (!(Organism.Locale is WaterLocale))
                throw new System.NotImplementedException();

            return pump_out ? Organism.Cytosol : (Organism.Locale as WaterLocale).Solution;
        }
    }

    Solution Destination
    {
        get
        {
            if (!(Organism.Locale is WaterLocale))
                throw new System.NotImplementedException();

            return pump_out ? (Organism.Locale as WaterLocale).Solution : Organism.Cytosol;
        }
    }

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
                      bool pump_out_, Molecule molecule_, float rate_) 
        : base(catalyst_slot, 1, 0.1f)
    {
        pump_out = pump_out_;
        molecule = molecule_;
        base_quantity = Organism.Membrane.GetTransportRate(molecule, pump_out) * 
                        CatalystSlot.Compound.Quantity;
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

        PumpedCompound = Source.RemoveCompound(new Compound(molecule, base_quantity * Scale));
    }

    public override void End()
    {
        Destination.AddCompound(PumpedCompound);
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

        return new PumpAction(slot, cytosol_concentration > locale_concentration, Molecule, size);
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
