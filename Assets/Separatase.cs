using System.Collections.Generic;

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

                if (Cytosol.GetQuantity(ChargeableMolecule.ChargedNRG) < (EnergyBalance + Balance.Actions.CellSeparation.Endowment))
                    return false;

                return base.IsLegal;
            }
        }

        public SeparateCell(Cell.Slot catalyst_slot)
            : base(catalyst_slot, Balance.Actions.CellSeparation.Cost, Balance.Actions.CellSeparation.EnergyChange)
        {

        }

        public override Dictionary<object, List<Compound>> GetResourceDemands()
        {
            Dictionary<object, List<Compound>> demands = base.GetResourceDemands();

            demands[Cytosol].Add(new Compound(Molecule.ChargedNRG, Balance.Actions.CellSeparation.Endowment));

            return demands;
        }

        public override void Begin()
        {
            if (!IsLegal)
                return;

            base.Begin();

            SeedCompound = Cytosol.RemoveCompound(Molecule.ChargedNRG, Balance.Actions.CellSeparation.Endowment);
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