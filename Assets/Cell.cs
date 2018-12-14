using System.Collections.Generic;


public class Cell
{
    Organism organism;

    SlotList slots = new SlotList();

    public SlotList Slots
    {
        get { return slots; }
    }

    public Organism Organism
    {
        get { return organism; }
        set { organism = value; }
    }

    public Cell(Organism organism_)
    {
        organism = organism_;

        for (int i = 0; i < 6; i++)
            slots.Add(new Slot(this));
    }

    public int GetSlotIndex(Slot slot)
    {
        return slots.IndexOf(slot);
    }

    public void Rotate(int count)
    {
        if (count < 0)
            count = 6 - (count % 6);
        else
            count = count % 6;

        while (count-- > 0)
        {
            Slot slot = slots[slots.Count - 1];

            slots.Remove(slot);
            slots.Insert(0, slot);
        }
    }

    public void Detatch()
    {
        Organism.Locale.AddOrganism(new Organism(Organism.RemoveCell(this)));
    }

    public List<Action> GetActions()
    {
        List<Action> actions = new List<Action>();

        foreach (Slot slot in slots)
            if (slot.CatalystCompound != null)
                actions.Add((slot.CatalystCompound.Molecule as Catalyst).Catalyze(slot));

        return actions;
    }


    public class Slot
    {
        Cell cell;

        Compound compound;
        Compound catalyst_compound;

        public Cell Cell
        {
            get { return cell; }
        }

        public Cell AdjacentCell
        {
            get { return Cell.Organism.GetNeighbor(Cell, Direction); }
        }

        public Compound Compound
        {
            get
            {
                if (compound != null && compound.Quantity == 0)
                    compound = null;

                return compound;
            }

            private set { compound = value; }
        }

        public Compound CatalystCompound
        {
            get
            {
                if (catalyst_compound != null && catalyst_compound.Quantity == 0)
                    catalyst_compound = null;

                return catalyst_compound;
            }

            private set { catalyst_compound = value; }
        }

        public Compound ExposedCompound
        {
            get
            {
                if (Compound != null)
                    return Compound;

                if (CatalystCompound != null)
                    return CatalystCompound;

                return null;
            }
        }

        public int Index
        {
            get { return Cell.GetSlotIndex(this); }
        }

        public Slot NextSlot
        {
            get { return Cell.Slots[Index + 1]; }
        }

        public Slot PreviousSlot
        {
            get { return Cell.Slots[Index - 1]; }
        }

        public Slot AcrossSlot
        {
            get { return IsExposed ? null : AdjacentCell.Slots[Index + 3]; }
        }

        public bool IsExposed
        {
            get { return AdjacentCell == null; }
        }

        public Organism.HexagonalDirection Direction
        {
            get
            {
                switch (Index)
                {
                    case 0: return Organism.HexagonalDirection.Up;
                    case 1: return Organism.HexagonalDirection.UpRight;
                    case 2: return Organism.HexagonalDirection.DownRight;
                    case 3: return Organism.HexagonalDirection.Down;
                    case 4: return Organism.HexagonalDirection.DownLeft;
                    case 5: return Organism.HexagonalDirection.UpLeft;
                }

                return Organism.HexagonalDirection.Up;
            }
        }

        public Slot(Cell cell_)
        {
            cell = cell_;
        }

        public void AddCompound(Compound compound)
        {
            if (compound.Quantity <= 0)
                return;

            if (Compound == null)
                Compound = compound;
            else if (Compound.Molecule == compound.Molecule)
                Compound.Quantity += compound.Quantity;

            if (CatalystCompound == null)
            {
                if (Compound.Molecule is Catalyst)
                {
                    catalyst_compound = Compound;
                    Compound = null;
                }
            }
            else if (CatalystCompound.Molecule == Compound.Molecule)
            {
                CatalystCompound.Quantity += Compound.Quantity;
                Compound = null;
            }
        }

        public Compound RemoveExposedCompound()
        {
            Compound removed_compound;

            if (Compound != null)
            {
                removed_compound = Compound;
                Compound = null;
            }
            else
            {
                removed_compound = CatalystCompound;
                CatalystCompound = null;
            }

            return removed_compound;
        }
    }

    public class SlotList : List<Slot>
    {
        new public Slot this[int index]
        {
            get
            {
                if (index < 0)
                    index = 5 - (-index - 1) % 6;
                else
                    index = index % 6;

                return base[index];
            }
        }
    }
}