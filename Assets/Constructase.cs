using System.Collections.Generic;

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
            : base(catalyst_slot, Balance.Actions.CellConstruction.Cost, Balance.Actions.CellConstruction.EnergyChange)
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