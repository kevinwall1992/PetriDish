

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

        return new PoweredAction(slot, atp_slot, new RotateAction(slot));
    }
}


public class Constructase : Ribozyme
{
    public class ConstructCell : PoweredAction
    {
        public ConstructCell(Cell.Slot slot)
            : base(slot, slot,
                   new ReactionAction(slot,
                                      null, null,
                                      Utility.CreateList<Compound>(new Compound(Glucose, 7), new Compound(Phosphate, 1)), null))
        {

        }

        public override void Beginning()
        {
            base.Beginning();
        }

        public override void End()
        {
            base.End();

            Organism.AddCell(Cell, Slot.Direction);
        }
    }


    public Constructase() : base("Constructase", 6)
    {

    }

    public override Action Catalyze(Cell.Slot slot)
    {
        if (slot.Cell.Organism.GetNeighbor(slot.Cell, slot.Direction) != null)
            return null;

        return new ConstructCell(slot);
    }
}
