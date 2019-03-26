using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using UnityEngine.EventSystems;
using System;

public class OrganismComponent : GoodBehavior
{
    Queue<Action.Stage> stage_queue = new Queue<Action.Stage>();

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
            return stage_queue.Count > 0 || 
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

    DetailPanel cytosol_detail_panel;
    public DetailPanel CytozolDetailPanel
    {
        get
        {
            if (cytosol_detail_panel == null)
                cytosol_detail_panel = CompoundGridPanel.Create(Organism.Cytozol);

            return cytosol_detail_panel;
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

        if (stage_queue.Count > 0)
            foreach (Action action in Organism.GetActions(stage_queue.Dequeue()))
            {
                float length = 1.5f;
                if(action is ReactionAction || action is EnergeticReactionAction)
                    length = 3;

                gameObject.AddComponent<ActionComponent>().SetAction(action, length);
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
                    cell_component.transform.position = CellPositionToWorldPosition(Organism.GetCellPosition(cell_component.Cell));
                }
            }
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

    public Vector2 CellPositionToWorldPosition(Vector2Int position)
    {
        Func<Vector2Int, Vector2> hexagon_tiler = (p) => (new Vector2(p.x * 4.2f * 0.87f,
                                                                     (p.y + (p.x % 2 == 0 ? 0 : 0.5f)) * 4.2f));

        Vector2 displacement = Vector2.zero;

        if(CellComponents.Count() > 0)
        {
            CellComponent cell_component = CellComponents.First();
            displacement = (Vector2)cell_component.transform.position -
                           hexagon_tiler(Organism.GetCellPosition(cell_component.Cell));
        }

        Vector2 world_position = transform.TransformPoint(hexagon_tiler(position));

        return  world_position + displacement;
    }

    List<Action> FilterActions<T>(List<Action> actions)
    {
        return actions.OfType<T>().OfType<Action>().ToList();
    }

    public void BeginStepVisualization()
    {
        Organism.Membrane.Step();

        stage_queue = new Queue<Action.Stage>(Action.Stages);
    }

    public void ResetExperiment(string dna_sequence= "")
    {
        foreach (CellComponent cell_component in CellComponents)
        {
            Organism.SeparateCell(cell_component.Cell);
            GameObject.Destroy(cell_component.gameObject);
        }

        Cell cell = Organism.GetCell(new Vector2Int(0, 0));

        if (dna_sequence != "")
        {
            Organism.AddCell(cell, Cell.Relation.Up);

            Ribozyme interpretase = new Ribozyme(new Interpretase());
            interpretase.AddCofactor(new Compound(new DNA(dna_sequence), 1));
            cell.Slots[0].AddCompound(new Compound(interpretase, 1));

            cell.Slots[1].AddCompound(new Compound(Molecule.Glucose, 2));
            cell.Slots[2].AddCompound(new Compound(Molecule.Glucose, 1));
            cell.Slots[3].AddCompound(new Compound(Molecule.ATP, 1));

            Organism.Cytozol.AddCompound(new Compound(Molecule.ATP, 10));
            Organism.Cytozol.AddCompound(new Compound(Molecule.Phosphate, 10));
        }
    }
}
