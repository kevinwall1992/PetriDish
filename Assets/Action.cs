using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public abstract class Action
{
    float base_cost;

    protected float BaseCost
    {
        get { return base_cost; }

        set
        {
            base_cost = value;
            Scale = 1;
        }
    }

    public Cell.Slot Slot { get; private set; }

    public Cell Cell
    {
        get { return Slot.Cell; }
    }

    public Organism Organism
    {
        get { return Cell.Organism; }
    }

    public virtual float Scale { get; set; }

    public float Cost
    {
        get { return Scale * BaseCost; }

        set
        {
            if(BaseCost != 0)
                Scale = value / BaseCost;
        }
    }

    public float AmountPaid { get; private set; }

    public bool IsPaidFor
    {
        get
        {
            return AmountPaid >= Cost;
        }
    }

    public bool HasFailed { get; private set; }

    public Action(Cell.Slot slot, float cost)
    {
        Slot = slot;
        BaseCost = cost;
    }

    public void Pay(float payment)
    {
        AmountPaid += payment;
    }

    public abstract bool Prepare();
    public abstract void Begin();
    public abstract void End();

    protected void Fail()
    {
        HasFailed = true;
    }
}


public class CompositeAction : Action
{
    List<Action> actions= new List<Action>();

    public override float Scale
    {
        set
        {
            float ratio = value / Scale;

            base.Scale = value;

            foreach (Action action in actions)
                action.Scale *= ratio;
        }
    }

    public List<Action> Actions { get { return actions; } }

    public CompositeAction(Cell.Slot slot, params Action[] actions_) 
        : base(slot, MathUtility.Sum(actions_, delegate(Action action) { return action.Cost; }))
    {
        actions.AddRange(actions_);
    }

    public CompositeAction(Cell.Slot slot, float cost, params Action[] actions_) : base(slot, cost)
    {
        actions.AddRange(actions_);
    }

    public override bool Prepare()
    {
        foreach (Action action in actions)
            if (!action.Prepare())
                Fail();

        return !HasFailed;
    }

    public override void Begin()
    {
        foreach (Action action in actions)
            action.Begin();
    }

    public override void End()
    {
        foreach (Action action in actions)
            action.End();
    }
}

public class WrapperAction : CompositeAction
{
    public WrapperAction(Cell.Slot slot, Action action, float cost) : base(slot, cost, action)
    {

    }
}


public class MoveAction : Action
{
    public Cell.Slot InputSlot { get; private set; }
    public Cell.Slot OutputSlot { get; private set; }

    public Compound Compound { get; private set; }

    public MoveAction(Cell.Slot slot, Cell.Slot input_slot, Cell.Slot output_slot, float quantity)
        : base(slot, 1)
    {
        InputSlot = input_slot;
        OutputSlot = output_slot;

        if (InputSlot.Compound == null)
            Cost = 0;
        else
            Cost = Mathf.Min(quantity, InputSlot.Compound.Quantity);
    }

    public override bool Prepare()
    {
        if (InputSlot.Compound == null || 
            (OutputSlot.Compound != null && OutputSlot.Compound.Molecule != InputSlot.Compound.Molecule))
            Fail();

        return !HasFailed;
    }

    public override void Begin()
    {
        Compound = InputSlot.Compound.Split(Scale);
    }

    public override void End()
    {
        OutputSlot.AddCompound(Compound);
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
                    List<Compound> cytozol_products_,
                    float cost = 1) : base(slot, cost)
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

    protected ReactionAction(Cell.Slot slot) : base(slot, 1)
    {

    }

    public override bool Prepare()
    {
        foreach (Cell.Slot destination in slot_products.Keys)
            if (destination.Compound != null &&
                destination.Compound.Molecule != slot_products[destination].Molecule)
                Fail();

        foreach (Cell.Slot source in slot_reactants.Keys)
            if (source.Compound == null ||
                source.Compound.Molecule != slot_reactants[source].Molecule ||
                source.Compound.Quantity < slot_reactants[source].Quantity)
                Fail();

        foreach (Compound reactant in cytozol_reactants)
            if (Organism.Cytozol.GetQuantity(reactant.Molecule) < reactant.Quantity)
                Fail();

        return !HasFailed;
    }

    public override void Begin()
    {
        foreach (Cell.Slot source in slot_reactants.Keys)
            source.Compound.Split(slot_reactants[source].Quantity);

        foreach (Compound reactant in cytozol_reactants)
            Organism.Cytozol.RemoveCompound(reactant);
    }

    public override void End()
    {
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

public class EnergeticReactionAction : CompositeAction
{
    public EnergeticReactionAction(Cell.Slot slot, ReactionAction reaction_action, float atp_balance)
        : base(slot,
              reaction_action,
              atp_balance > 0 ? (Action)new ATPProductionAction(slot, atp_balance) :
                                (Action)new ATPConsumptionAction(slot, -atp_balance))
    {

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
        BaseCost = 0;
    }

    public ATPConsumptionAction(Cell.Slot slot, float quantity, Cell.Slot atp_slot)
        : base(slot, 
                Utility.CreateDictionary<Cell.Slot, Compound>(atp_slot, new Compound(Molecule.ATP, quantity)), null,
                Utility.CreateList<Compound>(new Compound(Molecule.Water, quantity)),
                Utility.CreateList<Compound>(new Compound(Molecule.ADP, quantity),
                                             new Compound(Molecule.Phosphate, quantity)))
    {
        BaseCost = 0;
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
        BaseCost = 0;
    }
}


public class PoweredAction : CompositeAction
{
    public PoweredAction(Cell.Slot slot, Cell.Slot atp_slot, float atp_cost, Action action) 
        : base(slot, action, new ATPConsumptionAction(slot, atp_cost, atp_slot))
    {
        
    }
}

public class NullAction : Action
{
    public NullAction() : base(null, 0)
    {

    }

    public override bool Prepare()
    {
        return true;
    }

    public override void Begin()
    {
        
    }

    public override void End()
    {
        
    }
}