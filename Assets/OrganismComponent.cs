using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using UnityEngine.EventSystems;

public class OrganismComponent : GoodBehavior
{
    Queue<List<Action>> actions= new Queue<List<Action>>();

    IEnumerable<CellComponent> CellComponents { get { return GetComponentsInChildren<CellComponent>(); } }

    public Organism Organism { get; private set; }

    public override bool IsPointedAt { get { return CellComponentPointedAt != null; } }

    public CellComponent CellComponentPointedAt
    {
        get
        {
            foreach (CellComponent cell_component in CellComponents)
                if (cell_component.IsPointedAt)
                    return cell_component;

            return null;
        }
    }

    public CellComponent CellComponentTouched
    {
        get
        {
            if (CellComponentPointedAt.IsTouched)
                return CellComponentPointedAt;

            return null;
        }
    }

    public bool IsVisualizingStep
    {
        get
        {
            return actions.Count > 0 || 
                   GetComponents<ActionComponent>().Length > 0;
        }
    }

    [SerializeField]
    public Color color;

    [SerializeField]
    GameObject north, east, south, west;

    public GameObject North { get { return north; } }
    public GameObject East { get { return east; } }
    public GameObject South { get { return south; } }
    public GameObject West { get { return west; } }

    DetailPanel cytozol_detail_panel;
    public DetailPanel CytozolDetailPanel
    {
        get
        {
            if (cytozol_detail_panel == null)
                cytozol_detail_panel = CompoundGridPanel.Create(Organism.Cytozol);

            return cytozol_detail_panel;
        }
    }

    DetailPanel deck_detail_panel;
    public DetailPanel DeckDetailPanel
    {
        get
        {
            if (deck_detail_panel == null)
                deck_detail_panel = DeckPanel.Create(Organism);

            return deck_detail_panel;
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

        foreach (CellComponent cell_component in CellComponents)
            cell_component.GetComponent<SpriteRenderer>().color = color;

        if (GetComponents<ActionComponent>().Length > 0)
            return;

        if (actions.Count > 0)
            foreach (Action action in actions.Dequeue())
            {
                float length = 1.5f;
                if (action is PoweredAction)
                    length = 3;

                gameObject.AddComponent<ActionComponent>().SetAction(action, length);
            }
    }

    void SetCellTransformations()
    {
        foreach (CellComponent cell_component in CellComponents)
        {
            Vector2Int position = Organism.GetCellPosition(cell_component.Cell);

            cell_component.transform.position= new Vector3(position.x* 4.2f* 0.87f, (position.y+ (position.x % 2 == 1 ? 0.5f : 0)) * 4.2f);
        }
    }

    void ValidateCells()
    {
        List<Cell> cells = Organism.GetCells();

        foreach (CellComponent cell_component in CellComponents)
            if (!cells.Contains(cell_component.Cell))
                GameObject.Destroy(cell_component.gameObject);

        if (CellComponents.Count() != Organism.GetCellCount())
            foreach (Cell cell in cells)
            {
                bool found_cell = false;

                foreach (CellComponent cell_component in CellComponents)
                    if (cell_component.Cell == cell)
                        found_cell = true;

                if (!found_cell)
                {
                    CellComponent cell_component = Instantiate(Scene.Micro.Prefabs.CellComponent).SetCell(cell);
                    cell_component.transform.SetParent(transform);
                }
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

        foreach (CellComponent cell_component in CellComponents)
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

        foreach (CellComponent cell_component in CellComponents)
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
        foreach (CellComponent cell_component in CellComponents)
        {
            Organism.RemoveCell(cell_component.Cell);
            GameObject.Destroy(cell_component.gameObject);
        }

        Cell cell = Organism.GetCell(new Vector2Int(0, 0));

        if (dna_sequence != "")
        {
            cell.Slots[0].AddCompound(new Compound(Ribozyme.GetRibozymeFamily("Interpretase")[0], 1));
            cell.Slots[5].AddCompound(new Compound(new DNA(dna_sequence), 1));
            Organism.Cytozol.AddCompound(new Compound(Molecule.ATP, 10));
            Organism.Cytozol.AddCompound(new Compound(Molecule.Glucose, 10));
            Organism.Cytozol.AddCompound(new Compound(Molecule.Phosphate, 10));
        }
    }
}
