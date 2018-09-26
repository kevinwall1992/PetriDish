using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class Action
{
    Cell.Slot slot;

    bool has_begun = false;
    bool has_failed = false;
    
    public Cell.Slot Slot
    {
        get { return slot; }
    }

    public Cell Cell
    {
        get { return Slot.Cell; }
    }

    public Organism Organism
    {
        get { return Cell.Organism; }
    }

    public Action(Cell.Slot slot_)
    {
        slot = slot_;
    }

    public virtual void Beginning()
    {
        has_begun = true;
    }

    public virtual void End()
    {
        
    }

    public bool HasBegun()
    {
        return has_begun;
    }

    public bool HasFailed()
    {
        return has_failed;
    }

    protected void Fail()
    {
        has_failed = true;
    }
}


public class ActionAnimation : MonoBehaviour
{
    float length= 1;
    float elapsed_time= 0;
    float delay = 0;

    protected virtual void Update()
    {
        elapsed_time += Time.deltaTime;
    }

    protected float GetMoment()
    {
        return Mathf.Max((elapsed_time- delay), 0)/ length;
    }

    public ActionAnimation SetLength(float length_, float delay_= 0)
    {
        length = length_;
        elapsed_time = 0;
        delay = delay_;

        return this;
    }
}

public class RotationAnimation : ActionAnimation
{
    CellComponent cell_component;
    int rotation_count;

    protected override void Update()
    {
        base.Update();

        if (cell_component == null)
            return;

        cell_component.transform.rotation = Quaternion.identity;
        cell_component.transform.Rotate(new Vector3(0, 0, rotation_count* -60 * GetMoment()));
    }

    public RotationAnimation SetParameters(CellComponent cell_component_, int rotation_count_)
    {
        cell_component = cell_component_;
        rotation_count = rotation_count_;

        return this;
    }

    private void OnDestroy()
    {
        
    }
}

public class TransformAnimation : ActionAnimation
{

}

public class MoveAnimation : ActionAnimation
{
    GameObject source, target;

    protected override void Update()
    {
        base.Update();

        transform.position = Vector2.Lerp(source.transform.position, target.transform.position, GetMoment());
    }

    public MoveAnimation SetParameters(GameObject source_, GameObject target_)
    {
        source = source_;
        target = target_;

        transform.position = source.transform.position;

        return this;
    }

    private void OnDestroy()
    {
        if(gameObject!= null)
            GameObject.Destroy(gameObject);
    }
}

public class FadeAnimation : ActionAnimation
{
    bool fade_in, fade_out;

    protected override void Update()
    {
        base.Update();

        if (GetMoment() < 0.5f && fade_in)
            GetComponent<SpriteRenderer>().color = Color.Lerp(Color.clear, Color.white, GetMoment() / 0.5f);
        else if(fade_out)
            GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.clear, (GetMoment()- 0.5f) / 0.5f);

    }

    public FadeAnimation SetParameters(bool fade_in_, bool fade_out_)
    {
        fade_in = fade_in_;
        fade_out = fade_out_;

        if (fade_in)
            GetComponent<SpriteRenderer>().color = Color.clear;
        else if(fade_out)
            GetComponent<SpriteRenderer>().color = Color.white;

        return this;
    }

    private void OnDestroy()
    {
        if (gameObject != null)
            GameObject.Destroy(gameObject);
    }
}

public class CytozolAnimation : ActionAnimation
{

}

public class ActionComponent : MonoBehaviour
{
    Action action;

    List<ActionAnimation> animations = new List<ActionAnimation>();

    float time_elapsed = 0;
    float length = 1;

    OrganismComponent OrganismComponent
    {
        get { return World.TheWorld.GetOrganismComponent(action.Organism); }
    }

    CellComponent CellComponent
    {
        get { return OrganismComponent.GetCellComponent(action.Cell); }
    }

    public void SetAction(Action action_, float length_)
    {
        action = action_;
        length = length_;

        action.Beginning();
        if (action.HasFailed())
            return;

        Queue<Action> actions = new Queue<Action>();
        actions.Enqueue(action);

        while (actions.Count > 0)
        {
            Action action = actions.Dequeue();
            if (action is PoweredAction)
            {
                actions.Enqueue((action as PoweredAction).GetReaction());
                actions.Enqueue((action as PoweredAction).GetAction());
            }

            if (action is RotateAction)
                animations.Add(gameObject.AddComponent<RotationAnimation>().SetParameters(CellComponent, 1).SetLength(length));

            if (action is Reaction)
            {
                Reaction reaction = action as Reaction;

                List<Cell.Slot> reactant_slots = reaction.GetReactantSlots();
                foreach (Cell.Slot reactant_slot in reactant_slots)
                {
                    CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                    compound_component.SetCompound(reaction.GetReactant(reactant_slot));

                    animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                                .SetParameters(CellComponent.GetSlotComponent(reactant_slot).CompoundComponent.gameObject, CellComponent.GetSlotComponent(action.Slot).CompoundComponent.gameObject)
                                                                .SetLength(0.5f* length));

                    animations.Add(compound_component.gameObject.AddComponent<FadeAnimation>()
                                                                .SetParameters(false, true)
                                                                .SetLength(0.2f * length, 0.4f* length));
                }

                List<Cell.Slot> product_slots = reaction.GetProductSlots();
                foreach (Cell.Slot product_slot in product_slots)
                {
                    CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                    compound_component.SetCompound(reaction.GetProduct(product_slot));

                    animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                                .SetParameters(CellComponent.GetSlotComponent(action.Slot).CompoundComponent.gameObject, CellComponent.GetSlotComponent(product_slot).CompoundComponent.gameObject)
                                                                .SetLength(0.5f * length, 0.5f * length));

                    animations.Add(compound_component.gameObject.AddComponent<FadeAnimation>()
                                                                .SetParameters(true, false)
                                                                .SetLength(0.25f * length, 0.75f * length));
                }

                foreach (Compound compound in reaction.GetCytozolReactants())
                {
                    CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                    compound_component.SetCompound(compound);

                    animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                                .SetParameters(CellComponent.gameObject, CellComponent.GetSlotComponent(action.Slot).CompoundComponent.gameObject)
                                                                .SetLength(0.5f * length));

                    animations.Add(compound_component.gameObject.AddComponent<FadeAnimation>()
                                                                .SetParameters(true, true)
                                                                .SetLength(0.5f * length, 0.5f * length));
                }

                foreach (Compound compound in reaction.GetCytozolProducts())
                {
                    CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                    compound_component.SetCompound(compound);

                    animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                                .SetParameters(CellComponent.GetSlotComponent(action.Slot).CompoundComponent.gameObject, CellComponent.gameObject)
                                                                .SetLength(0.5f * length, 0.5f * length));

                    animations.Add(compound_component.gameObject.AddComponent<FadeAnimation>()
                                                                .SetParameters(false, true)
                                                                .SetLength(0.1f * length, 0.9f * length));

                    animations.Add(compound_component.gameObject.AddComponent<FadeAnimation>()
                                                                .SetParameters(true, false)
                                                                .SetLength(0.1f * length, 0.5f * length));
                }
            }

            if(action is Interpretase.ActivateCommand)
            {
                Interpretase.ActivateCommand activate_command = action as Interpretase.ActivateCommand;

                CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                compound_component.SetCompound(activate_command.OutputtedCompound);

                animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                            .SetParameters(CellComponent.gameObject, CellComponent.GetSlotComponent(activate_command.OutputSlot).CompoundComponent.gameObject)
                                                            .SetLength(1.0f * length));

                animations.Add(compound_component.gameObject.AddComponent<FadeAnimation>()
                                                            .SetParameters(true, false)
                                                            .SetLength(0.5f));
            }

            if(action is Interpretase.CutCommand)
            {
                Interpretase.CutCommand cut_command = action as Interpretase.CutCommand;

                CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                compound_component.SetCompound(new Compound(new DNA(), 1));

                animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                            .SetParameters(CellComponent.GetSlotComponent(cut_command.Slot).CompoundComponent.gameObject, CellComponent.GetSlotComponent(cut_command.OutputSlot).CompoundComponent.gameObject)
                                                            .SetLength(1.0f * length));

                animations.Add(compound_component.gameObject.AddComponent<FadeAnimation>()
                                                            .SetParameters(true, false)
                                                            .SetLength(0.5f));
            }

            if(action is Interpretase.MoveCommand)
            {
                Interpretase.MoveCommand move_command = action as Interpretase.MoveCommand;

                CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                compound_component.SetCompound(move_command.OutputtedCompound);

                animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                            .SetParameters(CellComponent.GetSlotComponent(move_command.InputSlot).CompoundComponent.gameObject, CellComponent.GetSlotComponent(move_command.OutputSlot).CompoundComponent.gameObject)
                                                            .SetLength(1.0f * length));
            }
        }
    }

    float GetMoment()
    {
        return time_elapsed / length;
    }

    private void Update()
    {
        time_elapsed += Time.deltaTime;

        if (time_elapsed > length)
        {
            foreach (ActionAnimation animation in animations)
                GameObject.Destroy(animation);

            action.End();

            GameObject.Destroy(this);
        }
    }
}

public class RotateAction : Action
{
    public RotateAction(Cell.Slot slot) : base(slot)
    {

    }

    public override void End()
    {
        base.End();

        Cell.Rotate(1);
    }
}

public class Reaction : Action
{
    protected Dictionary<Cell.Slot, Molecule> slot_reactant_molecules = new Dictionary<Cell.Slot, Molecule>(),
                                              slot_product_molecules = new Dictionary<Cell.Slot, Molecule>();
    protected List<Molecule> cytozol_reactant_molecules = new List<Molecule>(),
                             cytozol_product_molecules = new List<Molecule>();

    protected Dictionary<Cell.Slot, Compound> slot_reactants = new Dictionary<Cell.Slot, Compound>(),
                                              slot_products = new Dictionary<Cell.Slot, Compound>();
    protected List<Compound> cytozol_reactants = new List<Compound>(),
                             cytozol_products = new List<Compound>();

    public Reaction(Cell.Slot slot,
                    Dictionary<Cell.Slot, Molecule> slot_reactant_molecules_, 
                    Dictionary<Cell.Slot, Molecule> slot_product_molecules_, 
                    List<Molecule> cytozol_reactant_molecules_, 
                    List<Molecule> cytozol_product_molecules_) : base(slot)
    {
        if(slot_reactant_molecules_ != null)
            slot_reactant_molecules = slot_reactant_molecules_;

        if (slot_product_molecules_ != null)
            slot_product_molecules = slot_product_molecules_;

        if (cytozol_reactant_molecules_ != null)
            cytozol_reactant_molecules = cytozol_reactant_molecules_;

        if (cytozol_product_molecules_ != null)
            cytozol_product_molecules = cytozol_product_molecules_;
    }

    protected Reaction(Cell.Slot slot) : base(slot)
    {

    }

    public override void Beginning()
    {
        base.Beginning();

        foreach (Cell.Slot destination in slot_product_molecules.Keys)
            if (destination.Compound != null && !destination.Compound.Molecule.CompareMolecule(slot_product_molecules[destination]))
                Fail();

        foreach (Cell.Slot source in slot_reactant_molecules.Keys)
            if (source.Compound == null || !source.Compound.Molecule.CompareMolecule(slot_reactant_molecules[source]))
                Fail();

        foreach(Molecule reactant in cytozol_reactant_molecules)
            if (Organism.Cytozol.GetCompound(reactant).Quantity== 0)
                Fail();

        if (HasFailed())
            return;


        foreach (Cell.Slot source in slot_reactant_molecules.Keys)
            slot_reactants[source]= (source.Compound.Split(1));
        foreach (Molecule molecule in cytozol_reactant_molecules)
            cytozol_reactants.Add(Organism.Cytozol.RemoveCompound(molecule, 1));

        foreach (Cell.Slot destination in slot_product_molecules.Keys)
            slot_products[destination]= (new Compound(slot_product_molecules[destination], 1));
        foreach (Molecule molecule in cytozol_product_molecules)
            cytozol_products.Add(new Compound(molecule, 1));
    }

    public override void End()
    {
        base.End();

        foreach (Cell.Slot destination in slot_products.Keys)
            destination.AddCompound(slot_products[destination]);

        foreach (Compound product in cytozol_products)
            Organism.Cytozol.AddCompound(product);
    }

    public List<Cell.Slot> GetReactantSlots()
    {
        return slot_reactants.Keys.ToList();
    }

    public List<Cell.Slot> GetProductSlots()
    {
        return slot_products.Keys.ToList();
    }

    public Compound GetReactant(Cell.Slot slot)
    {
        return slot_reactants[slot];
    }

    public Compound GetProduct(Cell.Slot slot)
    {
        return slot_products[slot];
    }

    //Want to make these two immutable/readonly lists somehow
    public List<Compound> GetCytozolReactants()
    {
        return cytozol_reactants;
    }

    public List<Compound> GetCytozolProducts()
    {
        return cytozol_products;
    }
}

//This probably can't be a reaction; 1) The original "reactant" shouldn't be destroyed,
//2) Causes Pipes to occur in the Reaction step. 
public class PipeAction : Reaction
{
    Cell.Slot input, output;

    public PipeAction(Cell.Slot slot, Cell.Slot input_, Cell.Slot output_) : base(slot)
    {
        input = input_;
        output = output_;
    }

    public override void Beginning()
    {
        if (input.Compound == null)
        {
            Fail();
            return;
        }

        slot_reactants[input] = input.Compound;
        slot_products[output] = slot_reactants[input];

        base.Beginning();
    }
}

public class PoweredAction : Action
{
    Reaction reaction;
    Action action;

    public PoweredAction(Cell.Slot slot, Cell.Slot atp_slot, Action action_) : base(slot)
    {
        action = action_;

        Dictionary<Cell.Slot, Molecule> reactants= new Dictionary<Cell.Slot, Molecule>();
        reactants[atp_slot] = Molecule.GetMolecule("ATP");

        List<Molecule> products = new List<Molecule>();
        products.Add(Molecule.GetMolecule("ADP"));

        reaction = new Reaction(slot, reactants, null, null, products);
    }

    public override void Beginning()
    {
        base.Beginning();

        reaction.Beginning();
        if (reaction.HasFailed())
            Fail();
        else
            action.Beginning();
    }

    public override void End()
    {
        base.End();

        reaction.End();
        action.End();
    }

    public Action GetAction()
    {
        return action;
    }

    public Reaction GetReaction()
    {
        return reaction;
    }
}