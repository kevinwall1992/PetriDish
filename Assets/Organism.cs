using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;


public class Organism : Chronal, Versionable<Organism>, Encodable
{
    List<List<Cell>> cells= new List<List<Cell>>();

    //3.3e-11 moles is based on model cell with volume of 0.6 cubic micrometers
    Cytosol cytosol= new Cytosol(Measures.MolesToSmoles(3.3e-14f));

    Membrane membrane;

    public Cytosol Cytosol{ get { return cytosol; } }
    public Membrane Membrane { get { return membrane; } }
    public Locale Locale { get; set; }

    public float SurfaceArea { get { return GetSurfaceArea(); } }


    public Deck Deck { get { return GetDeck(); } }

    List<Program> Programs { get; set; }


    public Organism(Cell cell)
    {
        Programs = new List<Program>(cell.Organism.Programs);

        membrane = new Membrane(this);

        cells.Add(new List<Cell>());
        cells[0].Add(cell);
        cell.Organism = this;
    }

    public Organism()
    {
        membrane = new Membrane(this);

        cells.Add(new List<Cell>());
        cells[0].Add(new Cell(this));

        Programs = new List<Program>();
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
        if (direction == Cell.Relation.None)
            return null;

        return GetCell(GetNeighborPosition(cell, direction));
    }

    IEnumerable<IEnumerable<Cell>> GetCellSets()
    {
        List<IEnumerable<Cell>> cell_sets = new List<IEnumerable<Cell>>();

        List<Cell> available_cells = new List<Cell>();
        foreach (Cell cell in GetCells())
            available_cells.Add(cell);

        while (available_cells.Count > 0)
        {
            HashSet<Cell> cell_set = new HashSet<Cell>();

            Queue <Cell> cell_queue = new Queue<Cell>();
            cell_queue.Enqueue(available_cells[0]);

            while (cell_queue.Count > 0)
            {
                Cell cell = cell_queue.Dequeue();
                if (available_cells.Contains(cell))
                    available_cells.Remove(cell);
                else
                    continue;

                foreach (Cell.Relation direction in System.Enum.GetValues(typeof(Cell.Relation)))
                {
                    Cell neighbor = cell.GetAdjacentCell(direction);

                    if(neighbor != null)
                        cell_queue.Enqueue(neighbor);
                }

                cell_set.Add(cell);
            }

            cell_sets.Add(cell_set);
        }

        return cell_sets;
    }

    bool IsPositionWithinBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < cells.Count &&
               position.y >= 0 && position.y < cells[0].Count;
    }

    //Position is not guaranteed to be within bounds after
    //this call, it only expands towards it once.
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

        System.Func<Catalyst, bool> IsAlreadyInDeck =
            delegate (Catalyst catalyst)
            {
                if (catalyst is Ribozyme)
                {
                    Ribozyme ribozyme = catalyst as Ribozyme;
                    foreach (Ribozyme other_ribozyme in deck.Where((other_catalyst) => (other_catalyst is Ribozyme)))
                        if (other_ribozyme.Sequence == ribozyme.Sequence)
                            return true;
                }
                else
                {
                    Protein protein = catalyst as Protein;
                    foreach (Protein other_protein in deck.Where((other_catalyst) => (other_catalyst is Protein)))
                        if (other_protein.DNASequence == protein.DNASequence)
                            return true;
                }

                return false;
            };

        foreach (Cell cell in GetCells())
        {
            foreach (Cell.Slot slot in cell.Slots)
            {
                if (slot.Compound == null)
                    continue;

                DNA dna = null;
                if (slot.Compound.Molecule is Catalyst && slot.Compound.Molecule is Ribozyme)
                {
                    Catalyst catalyst = slot.Compound.Molecule as Catalyst;
                    if(!IsAlreadyInDeck(catalyst))
                        deck.Add(catalyst);

                    dna = Interpretase.GetGeneticCofactor(catalyst);
                }
                else if (slot.Compound.Molecule is DNA)
                    dna = slot.Compound.Molecule as DNA;

                if(dna != null)
                {
                    for (int codon_index = 0; codon_index < dna.CodonCount; codon_index++)
                    {
                        string codon = dna.GetCodon(codon_index);

                        switch (codon[0])
                        {
                            case 'V':
                            case 'F':
                                int length;
                                Catalyst catalyst = Interpretase.GetCatalyst(dna, codon_index, out length);
                                if (catalyst != null)
                                {
                                    if (!IsAlreadyInDeck(catalyst))
                                        deck.Add(catalyst);

                                    codon_index += length - 1;
                                }

                                break;

                            case 'C':
                                codon_index += Interpretase.GetOperandCount(dna, codon_index);
                                break;

                            case 'L':
                                break;
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

    public Organism Separate(Cell loyal_cell, Cell rebel_cell)
    {
        //Collect neighbors before removing rebel cell
        Dictionary<Cell, Cell> host_cells = new Dictionary<Cell, Cell>();
        Dictionary<Cell, Cell.Relation> directions = new Dictionary<Cell, Cell.Relation>();

        Queue<Cell> cell_queue = new Queue<Cell>();
        foreach (Cell.Relation direction in System.Enum.GetValues(typeof(Cell.Relation)))
        {
            Cell neighbor = rebel_cell.GetAdjacentCell(direction);
            if (neighbor == null)
                continue;

            cell_queue.Enqueue(neighbor);
            host_cells[neighbor] = rebel_cell;
            directions[neighbor] = direction;
        }


        //Remove rebel cell, 
        //form sets from remaining cells,
        //remove the loyal set, 
        //and use remaining sets to form rebel cell list
        RemoveCell(rebel_cell);

        IEnumerable<IEnumerable<Cell>> factions = GetCellSets();

        List<Cell> rebel_cells = new List<Cell>();
        rebel_cells.Add(rebel_cell);

        foreach(IEnumerable<Cell> faction in factions)
        {
            bool is_rebel_faction = true;

            foreach (Cell cell in faction)
                if (cell == loyal_cell)
                    is_rebel_faction = false;

            if (is_rebel_faction)
                rebel_cells.AddRange(faction);
        }


        //Create rebel organism,
        //Add rebel cells one by one via their relationship
        //to existing rebel cells.
        Organism rebel_organism = new Organism(rebel_cell);

        while(cell_queue.Count > 0)
        {
            Cell cell = cell_queue.Dequeue();
            if (!rebel_cells.Contains(cell) || rebel_organism.GetCells().Contains(cell))
                continue;

            foreach (Cell.Relation direction in System.Enum.GetValues(typeof(Cell.Relation)))
            {
                Cell neighbor = cell.GetAdjacentCell(direction);
                if (neighbor == null)
                    continue;

                cell_queue.Enqueue(neighbor);
                host_cells[neighbor] = cell;
                directions[neighbor] = direction;
            }

            RemoveCell(cell);
            rebel_organism.AddCell(host_cells[cell], directions[cell], cell);
        }

        Locale.AddOrganism(rebel_organism);
        return rebel_organism;
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

    public Dictionary<Catalyst, Cell.Slot> GetCatalysts()
    {
        Dictionary<Catalyst, Cell.Slot> catalysts = new Dictionary<Catalyst, Cell.Slot>();

        foreach (Cell cell in GetCells())
            foreach (Cell.Slot slot in cell.Slots)
                if (slot.Compound != null && slot.Compound.Molecule is Catalyst)
                    catalysts[slot.Compound.Molecule as Catalyst] = slot;

        return catalysts;
    }

    public List<Action> GetActions(Action.Stage stage)
    {
        Dictionary<Catalyst, Cell.Slot> catalysts = GetCatalysts();

        List<Action> actions = new List<Action>();
        foreach (Catalyst catalyst in GetCatalysts().Keys)
        {
            Action action = catalyst.Catalyze(catalysts[catalyst], stage);

            if(action != null)
                actions.Add(action);
        }

        return actions;
    }

    public void Step()
    {
        Membrane.Step();

        foreach (Cell cell in GetCells())
            cell.Step();

        Dictionary<Catalyst, Cell.Slot> catalysts = GetCatalysts();

        Queue<Action.Stage> stage_queue = new Queue<Action.Stage>(Action.Stages);

        while(stage_queue.Count > 0)
        {
            Action.Stage stage = stage_queue.Dequeue();

            foreach (Catalyst catalyst in catalysts.Keys)
                catalyst.Communicate(catalysts[catalyst], stage);

            //Conflicts caused by the sequence of steps and stages, 
            //(f.e. cell construction not having enough material)
            //are resolved by Catalyst.Catalyze() simply not returning an action.
            //In addition conflicts such as two actions not being able to execute in a 
            //order agnostic way (like one outputting what the other needs), are also
            //detected in that step and thus never appear in the action list.
            //So, at this point, all actions should be able to Begin() without conflict. 
            List<Action> actions = GetActions(stage);
            foreach (Action action in actions) action.Begin();

            //Not all conflicts within a stage can be detected beforehand. In that scenario,
            //conflicts must rise to the level of (order agnostic) gameplay mechanics.
            //(f.e. if two compounds get pushed onto the same slot, a "Mess" is formed) 
            foreach (Action action in actions) if(action.HasBegun) action.End();
        }
    }

    public Program GetProgram(string dna_sequence)
    {
        foreach (Program program in Programs)
            if (program.GenerateDNASequence() == dna_sequence)
                return program;

        Program new_program = new Program(dna_sequence);
        Programs.Add(new_program);

        return new_program;
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
                Cell cell_copy = null;

                if (cell != null)
                {
                    cell_copy = new Cell(organism);

                    for (int slot_index = 0; slot_index < 6; slot_index++)
                    {
                        Cell.Slot slot = cell.Slots[slot_index];

                        if (slot.Compound != null)
                            cell_copy.Slots[slot_index].AddCompound(slot.Compound.Copy());
                    }
                }

                organism.cells[row].Add(cell_copy);
            }
        }

        organism.cytosol = new Cytosol(0);
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
                Cell cell_copy = null;

                if (other_cell != null)
                {
                    cell_copy = new Cell(this);

                    for (int slot_index = 0; slot_index < 6; slot_index++)
                    {
                        Cell.Slot slot = other_cell.Slots[slot_index];

                        if (slot.Compound != null)
                            cell_copy.Slots[slot_index].AddCompound(slot.Compound.Copy());
                    }
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
                    Cell this_cell = cells[row][column];
                    Cell other_cell = other.cells[row][column];

                    if ((this_cell == null) != (other_cell == null))
                        return false;

                    if (this_cell == null)
                        continue;

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

    public JObject EncodeJson()
    {
        JArray json_cell_array = new JArray();

        foreach (Cell cell in GetCells())
        {
            JObject json_cell_object = new JObject();

            Vector2Int position = GetCellPosition(cell);
            json_cell_object["Column"] = position.x;
            json_cell_object["Row"] = position.y;

            JArray json_slot_array = new JArray();
            foreach (Cell.Slot slot in cell.Slots)
            {
                JObject json_slot_object = new JObject();

                if (slot.Compound != null)
                    json_slot_object["Compound"] = slot.Compound.EncodeJson();

                json_slot_array.Add(json_slot_object);
            }
            json_cell_object["Slots"] = json_slot_array;

            json_cell_array.Add(json_cell_object);
        }


        JArray json_deck_array = new JArray();

        Deck deck = GetDeck();
        foreach (Catalyst catalyst in deck)
        {
            Catalyst blank_copy = catalyst.Copy();
            blank_copy.ClearState();

            json_deck_array.Add(blank_copy.EncodeJson());
        }


        JArray json_program_array = new JArray();
        foreach (Program program in Programs)
            json_program_array.Add(program.EncodeJson());


        return JObject.FromObject(Utility.CreateDictionary<string, object>("Cells", json_cell_array, 
                                                                           "Cytosol", cytosol.EncodeJson(), 
                                                                           "Deck", json_deck_array,
                                                                           "Programs", json_program_array));
    }

    public void DecodeJson(JObject json_object)
    {
        Dictionary<Vector2Int, Cell> decoded_cells = new Dictionary<Vector2Int, Cell>();

        Vector2Int min = Vector2Int.zero,
                   max = Vector2Int.zero;
        bool is_min_max_initialized = false;

        foreach (var json_cell_token in json_object["Cells"] as JArray)
        {
            JObject json_cell_object = json_cell_token as JObject;

            Vector2Int position = new Vector2Int(Utility.JTokenToInt(json_cell_object["Column"]),
                                                 Utility.JTokenToInt(json_cell_object["Row"]));

            if (!is_min_max_initialized)
            {
                min = max = position;
                is_min_max_initialized = true;
            }

            min = new Vector2Int(Mathf.Min(min.x, position.x), Mathf.Min(min.y, position.y));
            max = new Vector2Int(Mathf.Max(max.x, position.x), Mathf.Max(max.y, position.y));

            Cell cell = decoded_cells[position] = new Cell(this);

            int slot_index = 0;
            foreach (var json_slot_token in json_cell_object["Slots"] as JArray)
            {
                JObject json_slot_object = json_slot_token as JObject;

                if (json_slot_object["Compound"] != null)
                    cell.Slots[slot_index].AddCompound(Compound.DecodeCompound(json_slot_object["Compound"] as JObject));

                slot_index++;
            }
        }


        cells.Clear();

        for(int x = min.x; x<= max.x; x++)
        {
            List<Cell> column = new List<Cell>();

            for (int y = min.y; y <= max.y; y++)
                column.Add(null);

            cells.Add(column);
        }

        foreach(Vector2Int position in decoded_cells.Keys)
        {
            Vector2Int relative_position = position - min;

            cells[relative_position.x][relative_position.y] = decoded_cells[position];
        }


        cytosol.DecodeJson(json_object["Cytosol"] as JObject);


        foreach (var json_catalyst_token in json_object["Deck"] as JArray)
            Molecule.DecodeMolecule(json_catalyst_token as JObject);

        foreach(var json_program_token in json_object["Programs"] as JArray)
        {
            Program program = new Program();
            program.DecodeJson(json_program_token as JObject);

            Programs.Add(program);
        }
    }
}


public class Cytosol : Solution
{
    public Cytosol(float water_quantity) : base(water_quantity)
    {

    }
}