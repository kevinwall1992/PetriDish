using System.Collections.Generic;
using UnityEngine;

public interface Catalyst
{
    string Name { get; }

    Action Catalyze(Cell.Slot slot);
}

//This Catalyst executes the action in its entirety 
//all at once, and therefore may have to wait several
//turns building up to it. 
public abstract class ProgressiveCatalyst : Catalyst
{
    static Dictionary<Cell.Slot, Action> actions_in_progress = new Dictionary<Cell.Slot, Action>();

    public string Name { get; private set; }

    public ProgressiveCatalyst(string name)
    {
        Name = name;
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

        action.Pay(slot.CatalystCompound.Quantity);

        if (action.IsPaidFor)
            return action;
        else
            return null;
    }
}

//This Catalyst always returns an action (if able), 
//scaling the effect of the action to fit the 
//productivity of the Catalyst. 
public abstract class InstantCatalyst : ProgressiveCatalyst
{
    public InstantCatalyst(string name) : base(name)
    {
        
    }

    //Enforce productivity from above?
    public override Action Catalyze(Cell.Slot slot)
    { 
        Action action = GetActionInProgress(slot);
        if (action == null)
            return null;

        action.Pay(slot.CatalystCompound.Quantity);
        action.Cost = action.AmountPaid;

        return action;
    }
}




public class Rotase : ProgressiveCatalyst
{
    public Rotase() : base("Rotase")
    {

    }

    protected override Action GetAction(Cell.Slot slot)
    {
        return new PoweredAction(slot, slot, 1, new RotateAction(slot));
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
    public Constructase() : base("Constructase")
    {

    }

    protected override Action GetAction(Cell.Slot slot)
    {
        return new ConstructCell(slot);
    }


    public class ConstructCell : PoweredAction
    {
        public ConstructCell(Cell.Slot slot)
            : base(slot, slot, 5,
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
    public Pipase() : base("Pipase")
    {

    }

    protected override Action GetAction(Cell.Slot slot)
    {
        return new PipeAction(slot, 1);
    }


    public class PipeAction : Action
    {
        float rate;

        public Compound PipedCompound { get; private set; }

        //Change rate to quantity
        public PipeAction(Cell.Slot slot, float rate_) : base(slot, 1)
        {
            rate = rate_;
        }

        public override bool Prepare()
        {
            if (Slot.Compound == null)
                Fail();

            if (Slot.AcrossSlot != null)
                if (Slot.AcrossSlot.Compound != null &&
                    Slot.AcrossSlot.Compound.Molecule != Slot.Compound.Molecule)
                    Fail();

            return !HasFailed;
        }


        public override void Begin()
        {
            PipedCompound = Slot.Compound.Split(rate * Scale);
        }

        public override void End()
        {
            if (Slot.AcrossSlot != null)
                Slot.AcrossSlot.AddCompound(PipedCompound);
            else
            {
                if (Organism.Locale is WaterLocale)
                    (Organism.Locale as WaterLocale).Solution.AddCompound(PipedCompound);
                else
                    throw new System.NotImplementedException();
            }
        }
    }
}

public class Pumpase : InstantCatalyst
{
    bool pump_out;
    Molecule molecule;

    protected Pumpase(bool pump_out_, Molecule molecule_, string name) 
        : base(name)
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

        return slot.Compound.Molecule;
    }

    protected override Action GetAction(Cell.Slot slot)
    {
        if (molecule == null && slot.Compound == null)
            return null;

        return new PoweredAction(slot, 
                                 slot.NextSlot, 
                                 0.1f, 
                                 new PumpAction(slot, pump_out, GetMolecule(slot), 1));
    }
}

public class Endopumpase : Pumpase
{
    public Endopumpase(Molecule molecule = null) : base(false, molecule, "Endopumpase")
    {

    }
}

public class Exopumpase : Pumpase
{
    public Exopumpase(Molecule molecule = null) : base(true, molecule, "Exopumpase")
    {

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
    public Transcriptase() : base("Transcriptase")
    {

    }

    protected override Action GetAction(Cell.Slot slot)
    {
        if (slot.Compound == null || !(slot.Compound.Molecule is DNA))
            return null;

        int amp_count = 0,
            cmp_count = 0,
            gmp_count = 0,
            tmp_count = 0;
        foreach (Nucleotide nucleotide in (slot.Compound.Molecule as DNA).Monomers)
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
                  tmp_slot = null,
                  empty_slot = null;

        foreach (Cell.Slot other_slot in slot.Cell.Slots)
            if (other_slot.Compound == null)
                empty_slot = other_slot;
            else if (other_slot.Compound.Molecule == Nucleotide.AMP)
                amp_slot = other_slot;
            else if (other_slot.Compound.Molecule == Nucleotide.CMP)
                cmp_slot = other_slot;
            else if (other_slot.Compound.Molecule == Nucleotide.GMP)
                gmp_slot = other_slot;
            else if (other_slot.Compound.Molecule == Nucleotide.TMP)
                tmp_slot = other_slot;

        foreach (Cell.Slot other_slot in Utility.CreateList(amp_slot, cmp_slot, gmp_slot, tmp_slot, empty_slot))
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
                    empty_slot, new Compound(slot.Compound.Molecule, 1)),
                null,
                Utility.CreateList(new Compound(Molecule.Water, (slot.Compound.Molecule as DNA).Monomers.Count - 1)),
                (amp_count + cmp_count + gmp_count + tmp_count) / (6.0f / 4.0f)),
            new ATPConsumptionAction(slot, 1));
    }
}

public class Actuase : InstantCatalyst
{
    public Actuase() : base("Actuase")
    {

    }

    protected override Action GetAction(Cell.Slot slot)
    {
        if (slot.Compound == null || !(slot.Compound.Molecule is DNA))
            return null;

        DNA dna = slot.Compound.Molecule as DNA;

        if (dna.ActiveCodonIndex >= dna.CodonCount)
            dna.ActiveCodonIndex = 0;

        int codon_index = dna.ActiveCodonIndex;

        object location0 = Interpretase.CodonToLocation(slot, codon_index, out codon_index);
        object location1 = Interpretase.CodonToLocation(slot, codon_index, out codon_index);

        if (!(location0 is Cell.Slot) || !(location1 is Cell.Slot))
            return null;

        return new Interpretase.MoveCommand(slot, 
                                            dna.ActiveCodonIndex + 2, 
                                            location1 as Cell.Slot, 
                                            location0 as Cell.Slot, 
                                            slot.CatalystCompound.Quantity);
    }
}

public class Sporulase : ProgressiveCatalyst
{
    public Sporulase() : base("Sporulase")
    {

    }

    protected override Action GetAction(Cell.Slot slot)
    {
        return new PoweredAction(slot, slot, 4, new SporulateAction(slot));
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