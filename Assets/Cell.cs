using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Cell
{
    //The relationship between adjacent Cells
    //Can also be thought of as a direction
    public enum Relation { None = -1, Up, UpRight, DownRight, Down, DownLeft, UpLeft }

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

    public Cell GetAdjacentCell(Relation relation)
    {
        return Organism.GetNeighbor(this, relation);
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

    public void Step()
    {
        foreach (Slot slot in slots)
            slot.Step();
    }

    public class Slot
    {
        Cell cell;

        Compound compound;

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

        public Cell.Relation Direction
        {
            get
            {
                switch (Index)
                {
                    case 0: return Cell.Relation.Up;
                    case 1: return Cell.Relation.UpRight;
                    case 2: return Cell.Relation.DownRight;
                    case 3: return Cell.Relation.Down;
                    case 4: return Cell.Relation.DownLeft;
                    case 5: return Cell.Relation.UpLeft;
                }

                return Cell.Relation.Up;
            }
        }

        public Slot(Cell cell_)
        {
            cell = cell_;
        }

        public void AddCompound(Compound compound)
        {
            if (Compound == null)
                Compound = compound;
            else if (Compound.Molecule == compound.Molecule)
                Compound.Quantity += compound.Quantity;
        }

        public void AddCompound(Molecule molecule, float quantity)
        {
            AddCompound(new Compound(molecule, quantity));
        }

        public Compound RemoveCompound()
        {
            return Compound.Split(Compound.Quantity);
        }

        public void Step()
        {
            if (Compound != null && Compound.Molecule is Catalyst)
                (Compound.Molecule as Catalyst).Step(this);
        }


        //The relationship between adjacent Slots.
        //Right means clockwise, Left counter clockwise
        //(Righty tighty lefty loosey)
        public enum Relation { None = -1, Across, Left, Right }

        public Slot GetAdjacentSlot(Relation relation)
        {
            switch(relation)
            {
                case Relation.Across: return AcrossSlot;
                case Relation.Left: return PreviousSlot;
                case Relation.Right: return NextSlot;

                default: return null;
            }
        }

        public Relation GetRelation(Slot other)
        {
            if (other == AcrossSlot)
                return Relation.Across;
            else if (other == NextSlot)
                return Relation.Right;
            else if (other == PreviousSlot)
                return Relation.Left;

            return Relation.None;
        }

        public static Relation RotateRelation(Relation relation, bool rotate_right)
        {
            return (Relation)((int)(relation + (rotate_right ? 1 : 2)) % 3);
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