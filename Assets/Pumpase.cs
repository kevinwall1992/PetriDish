using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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
            Attachments[Cell.Slot.Relation.Across] = InPump;
            Attachments[Cell.Slot.Relation.Left] = OutPump;

        }
    }

    void DefaultInitialization()
    {
        InitializeAttachments();
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
        if (MathUtility.Roll(0.1f))
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
                      Molecule molecule_, object source, object destination, float rate, bool is_passive = false)
        : base(catalyst_slot, 
               Balance.Actions.MembraneTransport.Cost, 
               is_passive ? 0 : Balance.Actions.MembraneTransport.EnergyChange)
    {
        molecule = molecule_;

        Source = source;
        Destination = destination;

        base_quantity = Organism.Membrane.GetTransportRate(molecule, Destination is Locale) *
                        Balance.Actions.MembraneTransport.RateMultipliers[molecule];

        ScaleByFactor(rate);
    }

    public override Dictionary<object, List<Compound>> GetResourceDemands()
    {
        Dictionary<object, List<Compound>> demands = base.GetResourceDemands();

        if (!demands.ContainsKey(Source))
            demands[Source] = new List<Compound>();
        demands[Source].Add(new Compound(molecule, base_quantity * Scale));

        return demands;
    }

    protected override Dictionary<Cell.Slot, float> GetStackIncreases()
    {
        Dictionary<Cell.Slot, float> stack_increases = new Dictionary<Cell.Slot, float>();
        if (Destination is Cell.Slot)
            stack_increases[Destination as Cell.Slot] = base_quantity;

        return stack_increases;
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