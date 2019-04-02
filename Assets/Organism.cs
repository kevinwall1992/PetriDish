using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;


public class Organism : Chronal, Versionable<Organism>
{
    List<List<Cell>> cells= new List<List<Cell>>();

    //3.3e-11 moles is based on model cell with volume of 0.6 cubic micrometers
    Cytozol cytosol= new Cytozol(Measures.MolesToSmoles(3.3e-14f));

    Membrane membrane;

    public Cytozol Cytozol{ get { return cytosol; } }
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

    //Would like to make this private, 
    //and instead add a method tailored for use by visual layer
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
        if (!IsPositionWithinBounds(position))
            return null;

        return cells[position.x][position.y];
    }

    Vector2Int GetNeighborPosition(Cell cell, Cell.Relation direction)
    {
        Vector2Int position = GetCellPosition(cell);
        bool even_column = position.x % 2 == 0;

        switch (direction)
        {
            case Cell.Relation.Up: return position + new Vector2Int(0, 1); break;
            case Cell.Relation.UpRight: return position + new Vector2Int(1, even_column ? 0 : 1); break;
            case Cell.Relation.DownRight: return position + new Vector2Int(1, even_column ? -1 : 0); break;
            case Cell.Relation.Down: return position + new Vector2Int(0, -1); break;
            case Cell.Relation.DownLeft: return position + new Vector2Int(-1, even_column ? -1 : 0); break;
            case Cell.Relation.UpLeft: return position + new Vector2Int(-1, even_column ? 0 : 1); break;
        }

        return new Vector2Int();
    }

    public Cell GetNeighbor(Cell cell, Cell.Relation direction)
    {
        return GetCell(GetNeighborPosition(cell, direction));
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

                foreach (Cell.Relation direction in Enum.GetValues(typeof(Cell.Relation)))
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
                    RemoveCell(cell);
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
                            string dna_sequence = Interpretase.GetBlockSequence(dna, marker, codon_index);

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

    public Cell AddCell(Cell host_cell, Cell.Relation direction, Cell new_cell = null)
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

    Cell RemoveCell(Cell cell)
    {
        Vector2Int position = GetCellPosition(cell);

        cells[position.x][position.y] = null;

        return cell;
    }

    public Cell SeparateCell(Cell cell)
    {
        RemoveCell(cell);

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

    public List<Action> GetActions(Action.Stage stage)
    {
        List<Action> actions = new List<Action>();

        foreach (Cell cell in GetCells())
            actions.AddRange(cell.GetActions(stage));

        return actions;
    }

    public void Step()
    {
        Membrane.Step();

        foreach (Cell cell in GetCells())
            cell.Step();

        Queue<Action.Stage> stage_queue = new Queue<Action.Stage>(Action.Stages);

        while(stage_queue.Count > 0)
        {
            List<Action> actions = GetActions(stage_queue.Dequeue());

            //Resolve inter-stage conflicts by not executing, otherwise begin
            //(Pre-stage conflicts are resolved in Catalyst.Catalyze())
            foreach (Action action in actions) action.Begin();

            //End(). Any sequence conflicts at this point must be 
            //resolved through gameplay mechanics
            foreach (Action action in actions) if(action.HasBegun) action.End();
        }
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
                        cell_copy.Slots[slot_index].AddCompound(slot.Compound.Copy());
                }

                organism.cells[row].Add(cell_copy);
            }
        }

        organism.cytosol = new Cytozol(0);
        foreach (Molecule molecule in cytosol.Molecules)
            organism.cytosol.AddCompound(new Compound(molecule.Copy(), cytosol.GetQuantity(molecule)));

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
                        cell_copy.Slots[slot_index].AddCompound(slot.Compound.Copy());
                }

                cells[row].Add(cell_copy);
            }
        }


        foreach (Molecule molecule in cytosol.Molecules)
            cytosol.RemoveCompound(molecule, cytosol.GetQuantity(molecule));

        foreach (Molecule molecule in other.cytosol.Molecules)
            cytosol.AddCompound(new Compound(molecule.Copy(), other.cytosol.GetQuantity(molecule)));
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

        foreach (Molecule molecule in cytosol.Molecules)
            if (cytosol.GetQuantity(molecule) != other.cytosol.GetQuantity(molecule))
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