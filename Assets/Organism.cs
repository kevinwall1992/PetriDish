using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Compound
{
    Molecule molecule;
    float quantity;

    public Molecule Molecule
    {
        get
        {
            return molecule;
        }
    }

    public float Quantity
    {
        get { return quantity; }
        
        set { quantity = value; }
    }

    public Compound(Molecule molecule_, float quantity_)
    {
        molecule = molecule_;
        quantity = quantity_;
    }

    public Compound Split(float quantity)
    {
        quantity = Mathf.Min(quantity, Quantity);

        Quantity -= quantity;

        return new Compound(Molecule, quantity);
    }
}

public class Cell
{
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
            if (compound != null || compound_.Quantity== 0)
                return;

            compound = compound_;

            if(CatalystCompound == null && Compound.Molecule is Catalyst)
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


    Organism organism;

    List<Slot> slots= new List<Slot>();

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

        return slots[index% 6];
    }

    public void Rotate(int count)
    {
        if (count < 0)
            count = 6 -(count % 6);
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
}


public class Organism
{
    List<List<Cell>> cells= new List<List<Cell>>();
    Solution cytozol= new Solution(1000000000.0f);

    public Solution Cytozol
    {
        get { return cytozol; }
    }

    public Organism()
    {
        cells.Add(new List<Cell>());
        cells[0].Add(new Cell(this));
    }

    public Vector2Int GetCellPosition(Cell cell)
    {
        foreach (List<Cell> column in cells)
            foreach (Cell cell_ in column)
                if (cell == cell_)
                    return new Vector2Int(cells.IndexOf(column), column.IndexOf(cell));

        return new Vector2Int(-1, -1);
    }

    public Cell GetCell(Vector2Int position)
    {
        return cells[position.x][position.y];
    }


    public enum HexagonalDirection { Up, UpRight, DownRight, Down, DownLeft, UpLeft };

    Vector2Int GetDisplacement(bool even_column, HexagonalDirection direction)
    {
        switch (direction)
        {
            case HexagonalDirection.Up: return new Vector2Int(0, 1);
            case HexagonalDirection.UpRight: return new Vector2Int(1, even_column ? 0 : 1);
            case HexagonalDirection.DownRight: return new Vector2Int(1, even_column ? -1 : 0);
            case HexagonalDirection.Down: return new Vector2Int(0, -1);
            case HexagonalDirection.DownLeft: return new Vector2Int(-1, even_column ? -1 : 0);
            case HexagonalDirection.UpLeft: return new Vector2Int(-1, even_column ? 0 : 1);
        }

        return Vector2Int.zero;
    }

    Vector2Int GetNeighborPosition(Cell cell, HexagonalDirection direction)
    {
        Vector2Int cell_position = GetCellPosition(cell);

        return cell_position + GetDisplacement(cell_position.x % 2 == 0, direction);
    }

    bool IsPositionWithinBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < cells.Count &&
               position.y >= 0 && position.y < cells[0].Count;
    }

    //Expands _once_ in each direction towards position, if necessary
    void ExpandTowardsPosition(Vector2Int position)
    {
        if (cells.Count == position.x)
            cells.Add(Utility.CreateNullList<Cell>(cells[0].Count));
        else if (position.x == -1)
            cells.Insert(0, Utility.CreateNullList<Cell>(cells[0].Count));

        if (cells[0].Count == position.y)
            foreach (List<Cell> column in cells)
                column.Add(null);
        else if (position.y == -1)
            foreach (List<Cell> column in cells)
                column.Insert(0, null);
    }

    public Cell GetNeighbor(Cell cell, HexagonalDirection direction)
    {
        Vector2Int position = GetNeighborPosition(cell, direction);

        if (IsPositionWithinBounds(position))
            return cells[position.x][position.y];

        return null;
    }

    public Cell AddCell(Cell host_cell, HexagonalDirection direction)
    {
        ExpandTowardsPosition(GetNeighborPosition(host_cell, direction));

        Vector2Int position = GetNeighborPosition(host_cell, direction);

        if (IsPositionWithinBounds(position))
            cells[position.x][position.y] = new Cell(this);

        return GetCell(position);
    }

    public void RemoveCell(Cell cell)
    {
        Vector2Int position = GetCellPosition(cell);

        cells[position.x][position.y] = null;

        if (GetCellCount() == 0)
            cells[0][0] = new Cell(this);
    }

    public List<Cell> GetCells()
    {
        List<Cell> cell_list= new List<Cell>();

        foreach (List<Cell> column in cells)
            foreach (Cell cell in column)
                if (cell != null)
                    cell_list.Add(cell);

        return cell_list;
    }

    public int GetCellCount()
    {
        return GetCells().Count;
    }
}
