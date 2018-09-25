using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

public class OrganismComponent : MonoBehaviour
{
    Organism organism = new Organism();

    List<CellComponent> cell_components= new List<CellComponent>();

    Queue<List<Action>> actions= new Queue<List<Action>>();

    public Organism Organism
    {
        get { return organism; }
    }

    void Start()
    {

    }

    void Update()
    {
        ValidateCells();

        if (GetComponents<ActionComponent>().Length > 0)
            return;

        if (actions.Count == 0)
        {
            if(World.TheWorld.IsTakingTurn(this))
                World.TheWorld.FinishTurn(this);
            return;
        }

        foreach (Action action in actions.Dequeue())
            gameObject.AddComponent<ActionComponent>().SetAction(action, action is PoweredAction ? 2 : 2);
    }

    void SetCellTransformations()
    {
        foreach (CellComponent cell_component in cell_components)
        {
            Vector2Int position = Organism.GetCellPosition(cell_component.Cell);

            cell_component.transform.parent = transform;
            cell_component.transform.Translate(new Vector3(position.x, position.y, 0));
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

    public void TakeTurn()
    {
        List<Action> new_actions= new List<Action>();

        foreach (CellComponent cell_component in cell_components)
            new_actions.AddRange(cell_component.Cell.GetActions());

        actions.Enqueue(new_actions.OfType<Interpretase.Command>().OfType<Action>().ToList());
        actions.Enqueue(new_actions.OfType<Reaction>().OfType<Action>().ToList());
        actions.Enqueue(new_actions.OfType<PipeAction>().OfType<Action>().ToList());
        actions.Enqueue(new_actions.OfType<PoweredAction>().OfType<Action>().ToList());
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
        SetCellTransformations();

        if (dna_sequence != "")
        {
            cell.GetSlot(0).AddCompound(new Compound(Molecule.GetMolecule("Interpretase"), 1));
            cell.GetSlot(0).AddCompound(new Compound(new DNA(dna_sequence), 1));
            Organism.Cytozol.AddCompound(new Compound(Molecule.GetMolecule("ATP"), 10));
        }
    }
}
