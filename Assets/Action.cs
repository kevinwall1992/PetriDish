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

    //Consider name change to Begin()
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
        if (GetMoment() < 1)
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

//This needs an overhaul to reduce coupling with Action, or at least manage it better
public class ActionComponent : MonoBehaviour
{
    Action action;

    List<ActionAnimation> animations = new List<ActionAnimation>();

    float time_elapsed = 0;
    float length = 1;

    OrganismComponent OrganismComponent
    {
        get { return Scene.Micro.Visualization.GetOrganismComponent(action.Organism); }
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
            else if (action is CompositeAction)
                foreach (Action component_action in (action as CompositeAction).Actions)
                    actions.Enqueue(component_action);

            if (action is RotateAction)
                animations.Add(gameObject.AddComponent<RotationAnimation>().SetParameters(CellComponent, 1).SetLength(length));

            if (action is ReactionAction)
            {
                ReactionAction reaction = action as ReactionAction;

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
                                                                .SetParameters(false, true)
                                                                .SetLength(0.2f * length, 0.4f * length));
                }

                foreach (Compound compound in reaction.GetCytozolProducts())
                {
                    GameObject source = CellComponent.GetSlotComponent(action.Slot).CompoundComponent.gameObject;
                    if (reaction.GetCytozolProducts().Count== 2)
                    {
                        if (reaction.GetCytozolProducts().IndexOf(compound) == 0)
                            source = CellComponent.GetSlotComponent(action.Slot).LeftCorner;
                        else
                            source = CellComponent.GetSlotComponent(action.Slot).RightCorner;
                    }

                    CompoundComponent compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
                    compound_component.SetCompound(compound);

                    animations.Add(compound_component.gameObject.AddComponent<MoveAnimation>()
                                                                .SetParameters(source, CellComponent.gameObject)
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

            if(!action.HasFailed())
                action.End();

            GameObject.Destroy(this);
        }
    }
}

public class CompositeAction : Action
{
    List<Action> actions= new List<Action>();

    public List<Action> Actions { get { return actions; } }

    public CompositeAction(Cell.Slot slot, params Action[] actions_) : base(slot)
    {
        actions.AddRange(actions_);
    }

    public void AddAction(Action action)
    {
        actions.Add(action);
    }

    public override void Beginning()
    {
        base.Beginning();

        foreach (Action action in actions)
        {
            action.Beginning();

            if (action.HasFailed())
                Fail();
        }
    }

    public override void End()
    {
        base.End();

        foreach (Action action in actions)
            action.End();
    }
}

//Should this inherit from PoweredAction, or is there some value in it being separate?
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

public class ReactionAction : Action
{
    protected Dictionary<Cell.Slot, Compound> slot_reactants = new Dictionary<Cell.Slot, Compound>(),
                                              slot_products = new Dictionary<Cell.Slot, Compound>();
    protected List<Compound> cytozol_reactants = new List<Compound>(),
                             cytozol_products = new List<Compound>();

    public ReactionAction(Cell.Slot slot,
                    Dictionary<Cell.Slot, Compound> slot_reactants_, 
                    Dictionary<Cell.Slot, Compound> slot_products_, 
                    List<Compound> cytozol_reactants_, 
                    List<Compound> cytozol_products_) : base(slot)
    {
        if(slot_reactants_ != null)
            slot_reactants = slot_reactants_;

        if (slot_products_ != null)
            slot_products = slot_products_;

        if (cytozol_reactants_ != null)
            cytozol_reactants = cytozol_reactants_;

        if (cytozol_products_ != null)
            cytozol_products = cytozol_products_;
    }

    protected ReactionAction(Cell.Slot slot) : base(slot)
    {

    }

    public override void Beginning()
    {
        base.Beginning();

        foreach (Cell.Slot destination in slot_products.Keys)
            if (destination.Compound != null && 
                !destination.Compound.Molecule.CompareMolecule(slot_products[destination].Molecule))
                Fail();

        foreach (Cell.Slot source in slot_reactants.Keys)
            if (source.Compound == null ||
                !source.Compound.Molecule.CompareMolecule(slot_reactants[source].Molecule) ||
                source.Compound.Quantity < slot_reactants[source].Quantity)
                Fail();

        foreach (Compound reactant in cytozol_reactants)
            if (Organism.Cytozol.GetQuantity(reactant.Molecule) < reactant.Quantity)
                Fail();

        if (HasFailed())
            return;


        foreach (Cell.Slot source in slot_reactants.Keys)
            source.Compound.Split(slot_reactants[source].Quantity);

        foreach (Compound reactant in cytozol_reactants)
            Organism.Cytozol.RemoveCompound(reactant);
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

public class ATPConsumptionAction : ReactionAction
{
    public ATPConsumptionAction(Cell.Slot slot, float quantity) 
        : base(slot, 
                null, null, 
                Utility.CreateList<Compound>(new Compound(Molecule.ATP, quantity), 
                                             new Compound(Molecule.Water, quantity)), 
                Utility.CreateList<Compound>(new Compound(Molecule.ADP, quantity), 
                                             new Compound(Molecule.Phosphate, quantity)))
    {

    }

    public ATPConsumptionAction(Cell.Slot slot, float quantity, Cell.Slot ATP_slot)
        : base(slot, 
                Utility.CreateDictionary<Cell.Slot, Compound>(ATP_slot, new Compound(Molecule.ATP, quantity)), null,
                Utility.CreateList<Compound>(new Compound(Molecule.Water, quantity)),
                Utility.CreateList<Compound>(new Compound(Molecule.ADP, quantity),
                                             new Compound(Molecule.Phosphate, quantity)))
    {

    }
}

public class ATPProductionAction : ReactionAction
{
    public ATPProductionAction(Cell.Slot slot, float quantity)
        : base(slot, 
                null, null,
                Utility.CreateList<Compound>(new Compound(Molecule.ADP, quantity),
                                             new Compound(Molecule.Phosphate, quantity)),
                Utility.CreateList<Compound>(new Compound(Molecule.ATP, quantity),
                                             new Compound(Molecule.Water, quantity)))
    {

    }
}

//This probably can't be a reaction; 1) The original "reactant" shouldn't be destroyed,
//2) Causes Pipes to occur in the Reaction step. 
public class PipeAction : ReactionAction
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
    CompositeAction composite_action;

    public PoweredAction(Cell.Slot slot, Cell.Slot atp_slot, Action action_) : base(slot)
    {
        composite_action = new CompositeAction(slot, action_, new ATPConsumptionAction(slot, 1, atp_slot));
    }

    public override void Beginning()
    {
        base.Beginning();

        composite_action.Beginning();
        if (composite_action.HasFailed())
            Fail();
    }

    public override void End()
    {
        base.End();

        composite_action.End();
    }

    public Action GetAction()
    {
        return composite_action.Actions[0];
    }

    public ReactionAction GetReaction()
    {
        return composite_action.Actions[1] as ReactionAction;
    }
}