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
                        foreach (HexagonalDirection direction in Enum.GetValues(typeof(HexagonalDirection)))
                            if (GetNeighbor(cell, direction) == null)
                                total_exposed_edges++;
                        
            return total_exposed_edges/ 6.0f;
        }
    }

    public Locale Locale { get; set; }

    public Organism()
    {
        membrane = new Membrane(this, new Dictionary<Molecule, float>());

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

    static void ExecuteActions<T>(List<T> actions) where T : Action
    {
        foreach (Action action in actions) action.Beginning();
        foreach (Action action in actions) action.End();
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
        ExecuteActions(actions.OfType<PipeAction>().ToList());
        ExecuteActions(actions.OfType<PoweredAction>().ToList());
    }
}


public class Cytozol : Solution
{
    public Cytozol(float water_quantity) : base(water_quantity)
    {

    }
}