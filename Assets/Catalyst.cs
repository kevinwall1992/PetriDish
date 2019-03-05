using System.Collections.Generic;
using UnityEngine;

public interface Catalyst
{
    string Name { get; }
    string Description { get; }
    int Price { get; }

    Example Example { get; }

    Action Catalyze(Cell.Slot slot);

    Catalyst Mutate();
}

//This Catalyst executes the action in its entirety 
//all at once, and therefore may have to wait several
//turns building up to it. 
public abstract class ProgressiveCatalyst : Catalyst
{
    static Dictionary<Cell.Slot, Action> actions_in_progress = new Dictionary<Cell.Slot, Action>();

    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Price { get; private set; }

    public virtual Example Example { get { return null; } }

    public ProgressiveCatalyst(string name, int price, string description = "")
    {
        Name = name;
        Description = description;
        Price = price;
    }

    protected abstract Action GetAction(Cell.Slot slot);

    protected Action GetActionInProgress(Cell.Slot slot)
    {
        if (!actions_in_progress.ContainsKey(slot))
            actions_in_progress[slot] = null;

        Action action = actions_in_progress[slot];

        if (action == null || action.HasFailed || action.IsPaidFor)
            actions_in_progress[slot] = GetAction(slot);

        action = actions_in_progress[slot];
        if (action != null && action.Prepare())
            return action;
        else
            return null;
    }

    public virtual Action Catalyze(Cell.Slot slot)
    {
        Action action = GetActionInProgress(slot);
        if (action == null)
            return null;

        action.Pay(slot.Compound.Quantity);

        if (action.IsPaidFor)
            return action;
        else
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
                new Rotase(),
                new Constructase(),
                new Pipase(Pipase.Location.Five, Pipase.Location.Across),
                new Transcriptase(),
                new Actuase(),
                new Sporulase()));
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

    //Enforce productivity from above?
    public override Action Catalyze(Cell.Slot slot)
    { 
        Action action = GetActionInProgress(slot);
        if (action == null)
            return null;

        action.Pay(slot.Compound.Quantity);
        action.Cost = action.AmountPaid;

        return action;
    }
}




public class Rotase : ProgressiveCatalyst
{
    public override Example Example
    {
        get
        {
            Organism organism = new Organism();
            Cell cell = organism.GetCell(new Vector2Int(0, 0));

            cell.Slots[0].AddCompound(new Compound(Ribozyme.GetFamily(Name)[0], 1));
            cell.Slots[5].AddCompound(new Compound(Molecule.ATP, 1));

            return new Example(organism, 1);
        }
    }

    public Rotase() : base("Rotase", 1, "Rotates cells")
    {

    }

    protected override Action GetAction(Cell.Slot slot)
    {
        return new PoweredAction(slot, slot.PreviousSlot, 1, new RotateAction(slot));
    }


    //Powered action?
    public class RotateAction : Action
    {
        public RotateAction(Cell.Slot slot) : base(slot, 1)
        {

        }

        public override bool Prepare() { return true; }

        public override void Begin() { }

        public override void End()
        {
            Cell.Rotate(1);
        }
    }
}


public class Constructase : ProgressiveCatalyst
{
    public Constructase() : base("Constructase", 1, "Makes new cells")
    {

    }

    protected override Action GetAction(Cell.Slot slot)
    {
        if (slot.AdjacentCell != null)
            return null;

        return new ConstructCell(slot);
    }


    public class ConstructCell : PoweredAction
    {
        public ConstructCell(Cell.Slot slot)
            : base(slot, slot.PreviousSlot, 5,
                   new ReactionAction(slot,
                                      null, null,
                                      Utility.CreateList<Compound>(new Compound(Molecule.Glucose, 7), 
                                                                   new Compound(Molecule.Phosphate, 1)), null))
        {

        }

        public override void Begin()
        {
            base.Begin();
        }

        public override void End()
        {
            base.End();

            Organism.AddCell(Cell, Slot.Direction);
        }
    }
}

public class Pipase : InstantCatalyst
{
    public enum Location { One, Two, Three, Four, Five, Across }

    Location source, destination;

    public Pipase(Location source_, Location destination_) : base("Pipase", 1, "Moves compounds from a specific slot to another")
    {
        source = source_;
        destination = destination_;
        
        if(destination == source)
        {
            if (source != Location.Across)
                destination = Location.Across;
            else
                destination = Location.One;
        }
    }

    Cell.Slot LocationToSlot(Cell.Slot slot, Location location)
    {
        if (location == Location.Across)
            return slot.AcrossSlot;
        else
            return slot.Cell.Slots[slot.Index + (int)location];
    }

    protected override Action GetAction(Cell.Slot slot)
    {
        return new MoveToSlotAction(slot, 
                                    LocationToSlot(slot, source), 
                                    LocationToSlot(slot, destination), 
                                    1);
    }

    public override Catalyst Mutate()
    {
        if (MathUtility.Roll(0.1f))
            return base.Mutate();
        else
            return new Pipase((Location)MathUtility.RandomIndex(6), (Location)MathUtility.RandomIndex(6));
    }

    public override bool Equals(object obj)
    {
        if (obj == this)
            return true;

        Pipase other = obj as Pipase;
        if (other == null)
            return false;

        return other.source == source && other.destination == destination;
    }
}

public class Pumpase : InstantCatalyst
{
    bool pump_out;
    Molecule molecule;

    public Pumpase(bool pump_out_, Molecule molecule_) 
        : base(pump_out_ ? "Exopumpase" : "Endopumpase", 
               1, 
               pump_out_ ? "Removes compounds from cell" : "Draws in compounds from outside")
    {
        pump_out = pump_out_;
        molecule = molecule_;
    }

    Molecule GetMolecule(Cell.Slot slot)
    {
        if (molecule != null)
            return molecule;

        if (slot.Compound == null)
            return null;

        return slot.PreviousSlot.Compound.Molecule;
    }

    protected override Action GetAction(Cell.Slot slot)
    {
        if (molecule == null && slot.Compound == null)
            return null;

        return new PoweredAction(slot, 
                                 slot.PreviousSlot, 
                                 0.1f, 
                                 new PumpAction(slot, pump_out, GetMolecule(slot), 1));
    }

    public static Pumpase Endo(Molecule molecule)
    {
        return new Pumpase(false, molecule);
    }

    public static Pumpase Exo(Molecule molecule)
    {
        return new Pumpase(true, molecule);
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
                return new Pumpase(!pump_out, molecule);
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == this)
            return true;

        Pumpase other = obj as Pumpase;
        if (other == null)
            return false;

        return other.pump_out == pump_out && 
               other.molecule.Equals(molecule);
    }


    static Molecule GetRandomMolecule()
    {
        Dictionary<Molecule, float> weighted_molecules =
            Utility.CreateDictionary<Molecule, float>(Molecule.GetMolecule("Hydrogen"), 10.0f,
                                                      Molecule.GetMolecule("Methane"), 10.0f,
                                                      Molecule.GetMolecule("Carbon Dioxide"), 10.0f,
                                                      Molecule.GetMolecule("Hydrogen Sulfide"), 10.0f,
                                                      Molecule.GetMolecule("Oxygen"), 5.0f,
                                                      Molecule.GetMolecule("Water"), 10.0f,
                                                      Molecule.GetMolecule("Salt"), 10.0f,
                                                      Molecule.GetMolecule("Nitrogen"), 5.0f,
                                                      Molecule.GetMolecule("Glucose"), 10.0f);

        foreach (Molecule molecule in Molecule.Molecules)
            if (!weighted_molecules.ContainsKey(molecule))
                weighted_molecules[molecule] = 1;

        return MathUtility.RandomElement(weighted_molecules);
    }
}

public class PumpAction : Action
{
    bool pump_out;
    Molecule molecule;
    float rate;

    Solution Source
    {
        get
        {
            if (!(Organism.Locale is WaterLocale))
                throw new System.NotImplementedException();

            return pump_out ? Organism.Cytozol : (Organism.Locale as WaterLocale).Solution;
        }
    }

    Solution Destination
    {
        get
        {
            if (!(Organism.Locale is WaterLocale))
                throw new System.NotImplementedException();

            return pump_out ? (Organism.Locale as WaterLocale).Solution : Organism.Cytozol;
        }
    }

    float EffectiveRate
    {
        get
        {
            float source_concentration = Source.GetConcentration(molecule);
            float destination_concentration = Destination.GetConcentration(molecule);

            return rate * 
                source_concentration * 10000000 *
                Mathf.Min(source_concentration / destination_concentration, 10) *
                Scale;
        }
    }

    public Compound PumpedCompound { get; private set; }

    public PumpAction(Cell.Slot slot, bool pump_out_, Molecule molecule_, float rate_) : base(slot, 1)
    {
        pump_out = pump_out_;
        molecule = molecule_;
        rate = rate_;
    }

    public override bool Prepare()
    {
        if (!Slot.IsExposed)
            Fail();

        return !HasFailed;
    }

    public override void Begin()
    {
        PumpedCompound = Source.RemoveCompound(new Compound(molecule, EffectiveRate));
    }

    public override void End()
    {
        Destination.AddCompound(PumpedCompound);
    }
}


public class Transcriptase : InstantCatalyst
{
    public Transcriptase() : base("Transcriptase", 3, "Copies DNA")
    {

    }

    protected override Action GetAction(Cell.Slot slot)
    {
        Cell.Slot dna_slot = slot.PreviousSlot;
        DNA dna = GetMoleculeInSlotAs<DNA>(dna_slot);
        if (dna == null)
            return null;

        int amp_count = 0,
            cmp_count = 0,
            gmp_count = 0,
            tmp_count = 0;
        foreach (Nucleotide nucleotide in dna.Monomers)
        {
            if (nucleotide == Nucleotide.AMP)
                amp_count++;
            else if (nucleotide == Nucleotide.CMP)
                cmp_count++;
            else if (nucleotide == Nucleotide.GMP)
                gmp_count++;
            else if (nucleotide == Nucleotide.TMP)
                tmp_count++;
        }

        Cell.Slot amp_slot = null,
                  cmp_slot = null,
                  gmp_slot = null,
                  tmp_slot = null;

        foreach (Cell.Slot other_slot in slot.Cell.Slots)
            if (other_slot.Compound.Molecule == Nucleotide.AMP)
                amp_slot = other_slot;
            else if (other_slot.Compound.Molecule == Nucleotide.CMP)
                cmp_slot = other_slot;
            else if (other_slot.Compound.Molecule == Nucleotide.GMP)
                gmp_slot = other_slot;
            else if (other_slot.Compound.Molecule == Nucleotide.TMP)
                tmp_slot = other_slot;

        foreach (Cell.Slot other_slot in Utility.CreateList(amp_slot, cmp_slot, gmp_slot, tmp_slot))
            if (other_slot == null)
                return null;

        return new CompositeAction(
            slot,
            new ReactionAction(
                slot,
                Utility.CreateDictionary<Cell.Slot, Compound>(
                    amp_slot, new Compound(Nucleotide.AMP, amp_count),
                    cmp_slot, new Compound(Nucleotide.CMP, cmp_count),
                    gmp_slot, new Compound(Nucleotide.GMP, gmp_count),
                    tmp_slot, new Compound(Nucleotide.TMP, tmp_count)),
                Utility.CreateDictionary<Cell.Slot, Compound>(
                    slot.AcrossSlot, new Compound(dna_slot.Compound.Molecule, 1)),
                null,
                Utility.CreateList(new Compound(Molecule.Water, dna.Monomers.Count - 1)),
                (amp_count + cmp_count + gmp_count + tmp_count) / (6.0f / 4.0f)),
            new ATPConsumptionAction(slot, 1));
    }
}

public class Actuase : InstantCatalyst
{
    public Actuase() : base("Actuase", 2, "Moves compounds using a DNA program.")
    {

    }

    protected override Action GetAction(Cell.Slot slot)
    {
        Cell.Slot dna_slot = slot.PreviousSlot;
        DNA dna = GetMoleculeInSlotAs<DNA>(dna_slot);
        if (dna == null)
            return null;

        if (dna.ActiveCodonIndex >= dna.CodonCount)
            dna.ActiveCodonIndex = 0;

        int codon_index = dna.ActiveCodonIndex;

        object location0 = Interpretase.CodonToLocation(dna_slot, codon_index, out codon_index);
        object location1 = Interpretase.CodonToLocation(dna_slot, codon_index, out codon_index);

        if (!(location0 is Cell.Slot) || !(location1 is Cell.Slot))
            return null;

        return new Interpretase.MoveCommand(slot,
                                            dna_slot,
                                            dna.ActiveCodonIndex + 2, 
                                            location1 as Cell.Slot, 
                                            location0 as Cell.Slot, 
                                            1);
    }
}

public class Sporulase : ProgressiveCatalyst
{
    public Sporulase() : base("Sporulase", 2, "Detatches a cell, creating a new organism")
    {

    }

    protected override Action GetAction(Cell.Slot slot)
    {
        if (slot.Cell.Organism.GetCells().Count == 1)
            return null;

        return new PoweredAction(slot, slot.PreviousSlot, 4, new SporulateAction(slot));
    }

    public class SporulateAction : Action
    {
        public SporulateAction(Cell.Slot slot) : base(slot, 2)
        {

        }

        public override bool Prepare() { return true; }

        public override void Begin() { }

        public override void End()
        {
            Cell.Detatch();
        }
    }
}
