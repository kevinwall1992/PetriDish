﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Compound
{
    Molecule molecule;
    int quantity;

    public Molecule Molecule
    {
        get
        {
            return molecule;
        }
    }

    public int Quantity
    {
        get { return quantity; }
        
        set { quantity = value; }
    }

    public Compound(Molecule molecule_, int quantity_)
    {
        molecule = molecule_;
        quantity = quantity_;
    }

    public Compound Split(int quantity)
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

        public Slot(Cell cell_)
        {
            cell = cell_;
        }

        public void AddCompound(Compound compound_)
        {
            if (Compound != null || compound_.Quantity== 0)
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
        return slots[index];
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

public class Cytozol
{
    Dictionary<Molecule, Compound> compounds= new Dictionary<Molecule, Compound>();

    public void AddCompound(Compound compound)
    {
        if (compounds.ContainsKey(compound.Molecule))
            compounds[compound.Molecule].Quantity += compound.Quantity;
        else
            compounds[compound.Molecule] = new Compound(compound.Molecule, compound.Quantity);
    }

    public Compound RemoveCompound(Molecule molecule, int quantity)
    {
        if (!(compounds.ContainsKey(molecule) && compounds[molecule].Quantity >= quantity))
            return new Compound(molecule, 0);

        Compound compound = compounds[molecule].Split(quantity);

        if (compounds[molecule].Quantity <= 0)
            compounds.Remove(molecule);

        return compound;
            
    }

    public Compound GetCompound(Molecule molecule)
    {
        if (compounds.ContainsKey(molecule))
            return compounds[molecule];

        return new Compound(molecule, 0);
    }
}

public class Organism
{
    List<List<Cell>> cells= new List<List<Cell>>();
    Cytozol cytozol= new Cytozol();

    public Cytozol Cytozol
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

    Vector2Int GetDisplacement(HexagonalDirection direction)
    {
        switch (direction)
        {
            case HexagonalDirection.Up: return new Vector2Int(0, 1);
            case HexagonalDirection.UpRight: return new Vector2Int(1, 1);
            case HexagonalDirection.DownRight: return new Vector2Int(1, -1);
            case HexagonalDirection.Down: return new Vector2Int(0, -1);
            case HexagonalDirection.DownLeft: return new Vector2Int(-1, -1);
            case HexagonalDirection.UpLeft: return new Vector2Int(-1, 1);
        }

        return Vector2Int.zero;
    }

    Vector2Int GetNeighborPosition(Cell cell, HexagonalDirection direction)
    {
        return GetCellPosition(cell) + GetDisplacement(direction);
    }

    bool IsPositionWithinBounds(Vector2Int position)
    {
        return position.x < 0 || position.x >= cells.Count ||
               position.y < 0 || position.y >= cells[0].Count;
    }

    //Expands _once_ in each direction towards position, if necessary
    void ExpandTowardsPosition(Vector2Int position)
    {
        if (cells.Count == position.x)
            cells.Add(new List<Cell>());
        else if (position.x == -1)
            cells.Insert(0, new List<Cell>());

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

        if (!IsPositionWithinBounds(position))
            return cells[position.x][position.y];

        return null;
    }

    public void AddCell(Cell host_cell, HexagonalDirection direction)
    {
        ExpandTowardsPosition(GetNeighborPosition(host_cell, direction));

        Vector2Int position = GetNeighborPosition(host_cell, direction);

        if (IsPositionWithinBounds(position))
            cells[position.x][position.y] = new Cell(this);
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
