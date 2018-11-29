using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;


public class Organism : Chronal
{
    List<List<Cell>> cells= new List<List<Cell>>();
    Cytozol cytozol= new Cytozol(20000000000.0f);
    Membrane membrane;

    public Cytozol Cytozol{ get { return cytozol; } }

    public Membrane Membrane { get { return membrane; } }

    public float SurfaceArea
    {
        get
        {
            int total_exposed_edges= 0;

            foreach (List<Cell> column in cells)
                foreach (Cell cell in column)
                    if (cell != null)
                        foreach (Cell.Slot slot in cell.Slots)
                            if (slot.IsExposed)
                                total_exposed_edges++;
                        
            return total_exposed_edges/ 6.0f;
        }
    }

    public Locale Locale { get; set; }

    public Organism(Cell cell = null)
    {
        membrane = new Membrane(this, new Dictionary<Molecule, float>());

        cells.Add(new List<Cell>());
        if (cell != null)
        {
            cells[0].Add(cell);
            cell.Organism = this;
        }
        else
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

    void CheckForBreaks()
    {
        HashSet<Cell> keep_set = null;
        while (true)
        {
            HashSet<Cell> cell_set = new HashSet<Cell>();

            Stack<Cell> cell_stack = new Stack<Cell>();
            foreach (Cell cell in GetCells())
                if (keep_set== null || !keep_set.Contains(cell))
                {
                    cell_stack.Push(cell);
                    break;
                }
            if (cell_stack.Count == 0)
                break;

            Organism organism = null;
            if (keep_set != null)
                Locale.AddOrganism(organism = new Organism(cell_stack.First()));

            while (cell_stack.Count > 0)
            {
                Cell cell = cell_stack.Pop();

                if (cell_set.Contains(cell))
                    continue;
                cell_set.Add(cell);

                foreach (HexagonalDirection direction in Enum.GetValues(typeof(HexagonalDirection)))
                {
                    Cell neighbor = GetNeighbor(cell, direction);
                    if (neighbor != null)
                    {
                        cell_stack.Push(neighbor);

                        if (keep_set != null)
                            organism.AddCell(cell, direction, neighbor);
                    }
                }
            }

            if (keep_set == null)
                keep_set = cell_set;
            else
                foreach (Cell cell in cell_set)
                    RemoveCell_NoSideEffects(cell);
        }
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

    public Cell AddCell(Cell host_cell, HexagonalDirection direction, Cell new_cell = null)
    {
        if (new_cell == null)
            new_cell = new Cell(this);
        else
            new_cell.Organism = this;

        ExpandTowardsPosition(GetNeighborPosition(host_cell, direction));

        Vector2Int position = GetNeighborPosition(host_cell, direction);

        if (IsPositionWithinBounds(position))
            cells[position.x][position.y] = new_cell;

        return GetCell(position);
    }

    Cell RemoveCell_NoSideEffects(Cell cell)
    {
        Vector2Int position = GetCellPosition(cell);

        cells[position.x][position.y] = null;

        return cell;
    }

    public Cell RemoveCell(Cell cell)
    {
        RemoveCell_NoSideEffects(cell);

        if (GetCellCount() == 0)
            cells[0][0] = new Cell(this);

        CheckForBreaks();

        return cell;
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

    static void ExecuteActions<T>(List<T> actions) where T : Action
    {
        foreach (Action action in actions) action.Prepare();
        foreach (Action action in actions) if (!action.HasFailed) action.Begin();
        foreach (Action action in actions) if (!action.HasFailed) action.End();
    }

    public void Step()
    {
        Membrane.Step();

        List<Action> actions= new List<Action>();

        foreach (List<Cell> column in cells)
            foreach (Cell cell in column)
                if (cell != null)
                    actions.AddRange(cell.GetActions());

        ExecuteActions(actions.OfType<Interpretase.Command>().ToList());
        ExecuteActions(actions.OfType<ReactionAction>().ToList());
        ExecuteActions(actions.OfType<Pipase.PipeAction>().ToList());
        ExecuteActions(actions.OfType<PoweredAction>().ToList());
    }
}


public class Cytozol : Solution
{
    public Cytozol(float water_quantity) : base(water_quantity)
    {

    }
}