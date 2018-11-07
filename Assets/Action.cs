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