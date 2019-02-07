using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;


public class Organism : Chronal, Versionable<Organism>
{
    List<List<Cell>> cells= new List<List<Cell>>();

    //3.3e-11 moles is based on model cell with volume of 0.6 cubic micrometers
    Cytozol cytozol= new Cytozol(Measures.MolesToSmoles(3.3e-14f));

    Membrane membrane;

    public Cytozol Cytozol{ get { return cytozol; } }
    public Membrane Membrane { get { return membrane; } }
    public Locale Locale { get; set; }

    public Deck Deck { get { return GetDeck(); } }

    public float SurfaceArea { get { return GetSurfaceArea(); } }


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

    public Vector2Int GetNeighborPosition(Cell cell, HexagonalDirection direction)
    {
        Vector2Int cell_position = GetCellPosition(cell);

        return cell_position + GetDisplacement(cell_position.x % 2 == 0, direction);
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

    //Position is not guaranteed to be within bounds after
    //this call, it only expands towards is once.
    //(except in the case of -x positions, which 
    //require expanding an even number of times)
    void ExpandTowardsPosition(Vector2Int position)
    {
        if (cells.Count == position.x)
            cells.Add(Utility.CreateNullList<Cell>(cells[0].Count));
        else if (position.x == -1)
        {
            cells.Insert(0, Utility.CreateNullList<Cell>(cells[0].Count));
            cells.Insert(0, Utility.CreateNullList<Cell>(cells[0].Count));
        }

        if (cells[0].Count == position.y)
            foreach (List<Cell> column in cells)
                column.Add(null);
        else if (position.y == -1)
            foreach (List<Cell> column in cells)
                column.Insert(0, null);
    }

    float GetSurfaceArea()
    {
        int total_exposed_edges = 0;

        foreach (List<Cell> column in cells)
            foreach (Cell cell in column)
                if (cell != null)
                    foreach (Cell.Slot slot in cell.Slots)
                        if (slot.IsExposed)
                            total_exposed_edges++;

        return total_exposed_edges / 6.0f;
    }

    Deck GetDeck()
    {
        Deck deck = new Deck();

        foreach (Cell cell in GetCells())
        {
            foreach (Cell.Slot slot in cell.Slots)
            {
                if (slot.Compound == null)
                    continue;

                if (slot.Compound.Molecule is Catalyst && slot.Compound.Molecule is Ribozyme)
                    deck.Add(slot.Compound.Molecule as Catalyst);
                else if (slot.Compound.Molecule is DNA)
                {
                    DNA dna = slot.Compound.Molecule as DNA;
                    
                    for (int marker_value = 48; marker_value < 63; marker_value++)
                    {
                        string marker = Interpretase.ValueToCodon(marker_value);

                        int codon_index = 0;

                        while (codon_index < dna.CodonCount && 
                               (codon_index = Interpretase.FindMarkerCodon(dna, marker, codon_index, false, false)) >= 0)
                        {
                            string dna_sequence = dna.GetSubsequence(codon_index + 1, Interpretase.GetSegmentLength(dna, marker, codon_index));

                            Ribozyme ribozyme = Ribozyme.GetRibozyme(dna_sequence);
                            if (ribozyme != null)
                                deck.Add(ribozyme);

                            Enzyme enzyme = Enzyme.GetEnzyme(Enzyme.DNASequenceToAminoAcidSequence(dna_sequence));
                            if (enzyme != null)
                                deck.Add(enzyme);

                            codon_index++;
                        }
                    }
                }
            }
        }

        return deck;
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

        List<Action> commands = new List<Action>(),
                     reactions = new List<Action>(),
                     move_actions = new List<Action>(),
                     powered_actions = new List<Action>();

        foreach (List<Cell> column in cells)
            foreach (Cell cell in column)
                if (cell != null)
                    foreach (Action action in cell.GetActions())
                    {
                        if (action is Interpretase.Command)
                            commands.Add(action);
                        else if (action is ReactionAction)
                            reactions.Add(action);
                        else if (action is MoveToSlotAction)
                            move_actions.Add(action);
                        else if (action is PoweredAction)
                            powered_actions.Add(action);
                    }

        ExecuteActions(commands);
        ExecuteActions(reactions);
        ExecuteActions(move_actions);
        ExecuteActions(powered_actions);
    }

    public Organism Copy()
    {
        Organism organism = new Organism();

        organism.cells.Clear();
        for (int row = 0; row < cells.Count; row++)
        {
            organism.cells.Add(new List<Cell>());

            for (int column = 0; column < cells[row].Count; column++)
            {
                Cell cell = cells[row][column];
                Cell cell_copy = new Cell(organism);

                for (int slot_index = 0; slot_index < 6; slot_index++)
                {
                    Cell.Slot slot = cell.Slots[slot_index];

                    if (slot.Compound != null)
                        cell_copy.Slots[slot_index].AddCompound(new Compound(slot.Compound.Molecule.Copy(), slot.Compound.Quantity));
                }

                organism.cells[row].Add(cell_copy);
            }
        }

        organism.cytozol = new Cytozol(0);
        foreach (Molecule molecule in cytozol.Molecules)
            organism.cytozol.AddCompound(new Compound(molecule.Copy(), cytozol.GetQuantity(molecule)));

        return organism;
    }

    public void Checkout(Organism other)
    {
        cells.Clear();
        for (int row = 0; row < other.cells.Count; row++)
        {
            cells.Add(new List<Cell>());

            for (int column = 0; column < other.cells[row].Count; column++)
            {
                Cell other_cell = other.cells[row][column];
                Cell cell_copy = new Cell(this);

                for (int slot_index = 0; slot_index < 6; slot_index++)
                {
                    Cell.Slot slot = other_cell.Slots[slot_index];

                    if (slot.Compound != null)
                        cell_copy.Slots[slot_index].AddCompound(new Compound(slot.Compound.Molecule.Copy(), slot.Compound.Quantity));
                }

                cells[row].Add(cell_copy);
            }
        }


        foreach (Molecule molecule in cytozol.Molecules)
            cytozol.RemoveCompound(molecule, cytozol.GetQuantity(molecule));

        foreach (Molecule molecule in other.cytozol.Molecules)
            cytozol.AddCompound(new Compound(molecule.Copy(), other.cytozol.GetQuantity(molecule)));
    }

    public bool IsSameVersion(Organism other)
    {
        if (cells.Count != other.cells.Count ||
           (cells.Count > 0 && cells[0].Count != other.cells[0].Count))
            return false;

        for (int row = 0; row < cells.Count; row++)
            for (int column = 0; column < cells[row].Count; column++)
                for (int slot_index = 0; slot_index < 6; slot_index++)
                {
                    Compound this_compound = cells[row][column].Slots[slot_index].Compound;
                    Compound other_compound = other.cells[row][column].Slots[slot_index].Compound;

                    if (this_compound == null && other_compound == null)
                        continue;

                    if (this_compound == null || other_compound == null)
                        return false;

                    if (!this_compound.Molecule.Equals(other_compound.Molecule))
                        return false;

                    if (this_compound.Quantity != other_compound.Quantity)
                        return false;
                }

        foreach (Molecule molecule in cytozol.Molecules)
            if (cytozol.GetQuantity(molecule) != other.cytozol.GetQuantity(molecule))
                return false;

        return true;
    }
}


public class Cytozol : Solution
{
    public Cytozol(float water_quantity) : base(water_quantity)
    {

    }
}