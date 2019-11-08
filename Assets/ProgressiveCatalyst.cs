using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;


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
        if (is_initialized)
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
        if (action == null || !stage.Includes(action) || !action.IsLegal || action.Cost > Progress)
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

        if (Progress >= action.Cost)
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
        foreach (Cell.Slot.Relation direction in System.Enum.GetValues(typeof(Cell.Slot.Relation)))
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

    public Compound RemoveCofactor(Compound cofactor)
    {
        cofactors.Remove(cofactor);
        return cofactor;
    }


    public void ClearState()
    {
        cofactors.Clear();
        Orientation = DefaultOrientation;
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
        switch (Utility.JTokenToString(json_object["Orientation"]))
        {
            case "Right": Orientation = Cell.Slot.Relation.Right; break;
            case "Left": Orientation = Cell.Slot.Relation.Left; break;
            case "Across": Orientation = Cell.Slot.Relation.Across; break;
        }

        foreach (JToken json_cofactor_token in json_object["Cofactors"])
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
                                smallest_ratio;
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