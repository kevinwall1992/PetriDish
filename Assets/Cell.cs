using System.Collections.Generic;


public class Cell
{
    Organism organism;

    List<Slot> slots = new List<Slot>();

    public Organism Organism
    {
        get { return organism; }
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

    public Slot GetSlot(int index)
    {
        if (index < 0)
            index = -index;

        return slots[index % 6];
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

        public Compound Compound
        {
            get
            {
                if (compound != null && compound.Quantity == 0)
                    compound = null;

                return compound;
            }
        }

        public Compound CatalystCompound
        {
            get
            {
                if (catalyst_compound != null && catalyst_compound.Quantity == 0)
                    catalyst_compound = null;

                return catalyst_compound;
            }
        }

        public int Index
        {
            get { return Cell.GetSlotIndex(this); }
        }

        public Slot NextSlot
        {
            get { return Cell.GetSlot(Index + 1); }
        }

        public Slot PreviousSlot
        {
            get { return Cell.GetSlot(Index - 1); }
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

        public void AddCompound(Compound compound_)
        {
            if (compound != null || compound_.Quantity == 0)
                return;

            compound = compound_;

            if (CatalystCompound == null && Compound.Molecule is Catalyst)
            {
                catalyst_compound = compound;
                compound = null;
            }
        }

        public Compound RemoveCompound()
        {
            Compound removed_compound = compound;
            compound = null;

            return removed_compound;
        }
    }
}