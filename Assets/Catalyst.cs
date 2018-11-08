

using UnityEngine;

public interface Catalyst
{
    Action Catalyze(Cell.Slot slot);
}


public class Rotase : Ribozyme
{
    public Rotase() : base("Rotase", 6)
    {

    }

    //Should Catalysts check if action is possible? Or should Actions? Both?
    public override Action Catalyze(Cell.Slot slot)
    {
        Cell.Slot atp_slot = slot;

        if (atp_slot.Compound == null || atp_slot.Compound.Molecule != Molecule.ATP)
            return null;

        return new PoweredAction(slot, atp_slot, 1, new RotateAction(slot));
    }


    //Powered action?
    public class RotateAction : Action
    {
        public RotateAction(Cell.Slot slot) : base(slot)
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


public class Constructase : Ribozyme
{
    public Constructase() : base("Constructase", 6)
    {

    }

    public override Action Catalyze(Cell.Slot slot)
    {
        if (slot.Cell.Organism.GetNeighbor(slot.Cell, slot.Direction) != null)
            return null;

        return new ConstructCell(slot);
    }


    public class ConstructCell : PoweredAction
    {
        public ConstructCell(Cell.Slot slot)
            : base(slot, slot, 5,
                   new ReactionAction(slot,
                                      null, null,
                                      Utility.CreateList<Compound>(new Compound(Glucose, 7), 
                                                                   new Compound(Phosphate, 1)), null))
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

public class Pipase : Ribozyme
{
    public Pipase() : base("Pipase", 4)
    {

    }

    public override Action Catalyze(Cell.Slot slot)
    {
        return new PipeAction(slot, 1);
    }


    public class PipeAction : Action
    {
        float rate;

        public Compound PipedCompound { get; private set; }

        public PipeAction(Cell.Slot slot, float rate_) : base(slot)
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
            PipedCompound = Slot.Compound.Split(rate);
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

public class Exopumpase : Ribozyme
{
    public Exopumpase() : base("Exopumpase", 6)
    {

    }

    public override Action Catalyze(Cell.Slot slot)
    {
        return new PoweredAction(slot, slot.NextSlot, 0.1f, new PumpAction(slot, true, 1));
    }
}

public class Endopumpase : Ribozyme
{
    public Endopumpase() : base("Endopumpase", 6)
    {

    }

    public override Action Catalyze(Cell.Slot slot)
    {
        return new PoweredAction(slot, slot.NextSlot, 0.1f, new PumpAction(slot, true, 1));
    }
}

public class PumpAction : Action
{
    bool cytozol_to_locale;
    float rate;

    Molecule PumpedMolecule
    {
        get
        {
            if (Slot.Compound == null)
                return null;

            return Slot.Compound.Molecule;
        }
    }

    Solution Source
    {
        get
        {
            if (!(Organism.Locale is WaterLocale))
                throw new System.NotImplementedException();

            return cytozol_to_locale ? Organism.Cytozol : (Organism.Locale as WaterLocale).Solution;
        }
    }

    Solution Destination
    {
        get
        {
            if (!(Organism.Locale is WaterLocale))
                throw new System.NotImplementedException();

            return cytozol_to_locale ? (Organism.Locale as WaterLocale).Solution : Organism.Cytozol;
        }
    }

    float EffectiveRate
    {
        get
        {
            return rate * Source.GetConcentration(PumpedMolecule) * 10000000;
        }
    }

    public Compound PumpedCompound { get; private set; }

    public PumpAction(Cell.Slot slot, bool cytozol_to_locale_, float rate_) : base(slot)
    {
        cytozol_to_locale = cytozol_to_locale_;
        rate = rate_;
    }

    public override bool Prepare()
    {
        if (PumpedMolecule == null)
            Fail();
        else if (Source.GetQuantity(PumpedMolecule) == 0)
            Fail();

        return !HasFailed;
    }

    public override void Begin()
    {
        PumpedCompound = Source.RemoveCompound(new Compound(PumpedMolecule, EffectiveRate));
    }

    public override void End()
    {
        Destination.AddCompound(PumpedCompound);
    }
}


public class Transcriptase : Ribozyme
{
    public Transcriptase() : base("Transcriptase", 6)
    {

    }

    public override Action Catalyze(Cell.Slot slot)
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
                Utility.CreateList(new Compound(Molecule.Water, (slot.Compound.Molecule as DNA).Monomers.Count - 1))), 
            new ATPConsumptionAction(slot, 2));
    }
}
