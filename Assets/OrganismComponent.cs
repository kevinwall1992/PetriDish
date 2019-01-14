using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

public class OrganismComponent : MonoBehaviour
{
    List<CellComponent> cell_components= new List<CellComponent>();

    Queue<List<Action>> actions= new Queue<List<Action>>();

    public Organism Organism { get; private set; }

    public bool IsVisualizingStep
    {
        get
        {
            return actions.Count > 0 || 
                   GetComponents<ActionComponent>().Length > 0;
        }
    }

    DetailPanel detail_panel;
    public DetailPanel DetailPanel
    {
        get
        {
            if (detail_panel == null)
                detail_panel = CompoundGridPanel.Create(Organism.Cytozol);

            return detail_panel;
        }
    }

    void Awake()
    {
        Organism = new Organism();
    }

    void Start()
    {
        
    }

    void Update()
    {
        ValidateCells();

        if (GetComponents<ActionComponent>().Length > 0)
            return;

        if(actions.Count > 0)
            foreach (Action action in actions.Dequeue())
                gameObject.AddComponent<ActionComponent>().SetAction(action, action is PoweredAction ? 3 : 1.5f);
    }

    void SetCellTransformations()
    {
        foreach (CellComponent cell_component in cell_components)
        {
            Vector2Int position = Organism.GetCellPosition(cell_component.Cell);

            cell_component.transform.parent = transform;
            cell_component.transform.localPosition= new Vector3(position.x* 4.2f* 0.87f, (position.y+ (position.x % 2 == 1 ? 0.5f : 0)) * 4.2f);
        }
    }

    void ValidateCells()
    {
        List<Cell> cells = Organism.GetCells();

        List<CellComponent> cell_components_copy = new List<CellComponent>(cell_components);
        foreach (CellComponent cell_component in cell_components_copy)
            if (!cells.Contains(cell_component.Cell))
            {
                cell_components.Remove(cell_component);
                GameObject.Destroy(cell_component.gameObject);
            }

        if (cell_components.Count != Organism.GetCellCount())
            foreach (Cell cell in cells)
            {
                bool found_cell = false;

                foreach (CellComponent cell_component in cell_components)
                    if (cell_component.Cell == cell)
                        found_cell = true;

                if (!found_cell)
                    cell_components.Add(new GameObject("cell").AddComponent<CellComponent>().SetCell(cell));
            }

        SetCellTransformations();
    }

    public void SetOrganism(Organism organism)
    {
        Organism = organism;

        ValidateCells();
    }

    public CellComponent GetCellComponent(Cell cell)
    {
        if (cell.Organism != Organism)
            return null;

        foreach (CellComponent cell_component in cell_components)
            if (cell_component.Cell == cell)
                return cell_component;

        ValidateCells();
        return GetCellComponent(cell);
    }

    public SlotComponent GetSlotComponent(Cell.Slot slot)
    {
        return GetCellComponent(slot.Cell).GetSlotComponent(slot);
    }

    List<Action> FilterActions<T>(List<Action> actions)
    {
        return actions.OfType<T>().OfType<Action>().ToList();
    }

    public void BeginStepVisualization()
    {
        Organism.Membrane.Step();

        List<Action> commands = new List<Action>(),
                     reactions = new List<Action>(),
                     move_actions = new List<Action>(),
                     powered_actions = new List<Action>();

        foreach (CellComponent cell_component in cell_components)
            foreach (Action action in cell_component.Cell.GetActions())
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

        actions.Enqueue(commands);
        actions.Enqueue(reactions);
        actions.Enqueue(move_actions);
        actions.Enqueue(powered_actions);
    }

    public void ResetExperiment(string dna_sequence= "")
    {
        foreach (CellComponent cell_component in cell_components)
        {
            Organism.RemoveCell(cell_component.Cell);
            GameObject.Destroy(cell_component.gameObject);
        }
        cell_components.Clear();

        Cell cell = Organism.GetCell(new Vector2Int(0, 0));

        if (dna_sequence != "")
        {
            cell.Slots[0].AddCompound(new Compound(Ribozyme.GetRibozymeFamily("Interpretase")[0], 1));
            cell.Slots[5].AddCompound(new Compound(new DNA(dna_sequence), 1));
            Organism.Cytozol.AddCompound(new Compound(Molecule.ATP, 10));
        }
    }
}
