using System.Collections.Generic;
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

    public Cell.Slot CatalystSlot { get; private set; }
    public Catalyst Catalyst { get; private set; }

    public Cell Cell { get { return CatalystSlot.Cell; } }
    public Organism Organism { get { return Cell.Organism; } }
    public Cytosol Cytosol { get { return Organism.Cytosol; } }

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

    //An "Illegal" action is one for which it is _known_
    //that it cannot execute or cannot execute in an 
    //order agnostic way. 
    //Be careful however - the opposite does not imply it 
    //is known that it CAN execute. 
    //Actions can still run into trouble, and if they do, 
    //are expected to resolve it in the End() function. 
    public virtual bool IsLegal { get { return true; } }

    public bool HasBegun { get; private set; }

    public Action(Cell.Slot catalyst_slot, float cost)
    {
        CatalystSlot = catalyst_slot;
        Catalyst = (CatalystSlot.Compound.Molecule as Catalyst);

        BaseCost = cost;
    }

    public virtual void Begin() { HasBegun = true; }
    public virtual void End() { }


    public static List<Stage> Stages
    {
        get
        {
            return Utility.CreateList(
                new Stage(typeof(Interpretase.GoToCommand)),

                new Stage(typeof(ReactionAction),
                            typeof(Pumpase),
                            typeof(Interpretase.GrabCommand),
                            typeof(Interpretase.ReleaseCommand),
                            typeof(Interpretase.ExciseCommand)),

                new Stage(typeof(Constructase.ConstructCell)),

                new Stage(typeof(Separatase.SeparateCell)), 

                new Stage(typeof(Interpretase.MoveCommand)), 
                new Stage(typeof(Interpretase.SpinCommand)));
        }
    }

    public class Stage
    {
        System.Predicate<Action> predicate;

        public Stage(params System.Type[] action_types)
        {
            predicate = delegate (Action action)
            {
                if (action is Interpretase.TryCommand)
                    action = (action as Interpretase.TryCommand).Command;

                if (action == null)
                    return false;

                foreach (System.Type type in action_types)
                    if (type.IsAssignableFrom(action.GetType()))
                        return true;

                return false;
            };
        }

        public bool Includes(Action action)
        {
            return predicate(action);
        }
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

    public override bool IsLegal
    {
        get
        {
            foreach (Action action in actions)
                if (!action.IsLegal)
                    return false;

            return base.IsLegal;
        }
    }

    public CompositeAction(Cell.Slot catalyst_slot, float additional_cost, params Action[] actions_)
        : base(catalyst_slot, additional_cost)
    {
        SetActions(actions_);
    }

    public CompositeAction(Cell.Slot catalyst_slot, params Action[] actions_)
        : base(catalyst_slot, 0)
    {
        SetActions(actions_);
    }

    protected void SetActions(IEnumerable<Action> actions)
    {
        Debug.Assert(this.actions.Count == 0, "CompositeAction : Attempted to set actions more than once.");

        this.actions.AddRange(actions);
        this.actions.RemoveAll(action => action == null);

        BaseCost += MathUtility.Sum(this.actions, (action) => (action.Cost));
    }

    public override void Begin()
    {
        base.Begin();

        foreach (Action action in actions)
            action.Begin();
    }

    public override void End()
    {
        foreach (Action action in actions)
            action.End();
    }
}

public class EnergeticAction : Action
{
    float atp_balance_per_cost;

    public float EnergyBalance
    {
        get { return atp_balance_per_cost * Cost; }
        protected set
        {
            if (value == 0 || Cost == 0)
                atp_balance_per_cost = value;
            else
                atp_balance_per_cost = value / Cost;
        }
    }

    bool IsExergonic { get { return EnergyBalance > 0; } }

    public EnergeticAction(Cell.Slot catalyst_slot, float cost, float energy_balance)
        : base(catalyst_slot, cost)
    {
        EnergyBalance = energy_balance;
    }

    public override void Begin()
    {
        if (IsExergonic)
        {
            if (Cytosol.GetQuantity(Molecule.ADP) < EnergyBalance &&
                Cytosol.GetQuantity(Molecule.Phosphate) < EnergyBalance)
                return;

            Cytosol.RemoveCompound(Molecule.ADP, EnergyBalance);
            Cytosol.RemoveCompound(Molecule.Phosphate, EnergyBalance);
        }
        else
        {
            if (Cytosol.GetQuantity(Molecule.ATP) < -EnergyBalance &&
                Cytosol.GetQuantity(Molecule.Water) < -EnergyBalance)
                return;

            Cytosol.RemoveCompound(Molecule.ATP, -EnergyBalance);
            Cytosol.RemoveCompound(Molecule.Water, -EnergyBalance);
        }

        base.Begin();
    }

    public override void End()
    {
        if (IsExergonic)
        {
            Cytosol.AddCompound(Molecule.ATP, EnergyBalance);
            Cytosol.AddCompound(Molecule.Water, EnergyBalance);
        }
        else
        {
            Cytosol.AddCompound(Molecule.ADP, -EnergyBalance);
            Cytosol.AddCompound(Molecule.Phosphate, -EnergyBalance);
        }

        base.End();
    }
}

public abstract class MoveAction<T> : EnergeticAction
{
    Compound source_compound_copy;

    public Cell.Slot Source { get; private set; }
    public T Destination { get; private set; }

    public Compound MovedCompound { get; private set; }

    public virtual bool IsInvalidated
    {
        get
        {
            return !source_compound_copy.Equals(Source.Compound);
        }
    }

    public override bool IsLegal
    {
        get
        {
            if (Source.Compound == null)
                return false;

            if (IsInvalidated)
                return false;

            return base.IsLegal;
        }
    }

    protected MoveAction(Cell.Slot catalyst_slot, Cell.Slot source, T destination, float quantity)
        : base(catalyst_slot, 1, -0.5f)
    {
        Source = source;
        source_compound_copy = source.Compound.Copy();

        Destination = destination;

        if (quantity < 0)
            quantity = source.Compound.Quantity;
        Cost = source.Compound == null ? 0 : Mathf.Min(quantity, source.Compound.Quantity);
    }

    public override void Begin()
    {
        base.Begin();

        MovedCompound = Source.Compound.Split(Scale);
    }
}

public class MoveToSlotAction : MoveAction<Cell.Slot>
{
    Compound destination_compound_copy = null;

    public override bool IsInvalidated
    {
        get
        {
            if (destination_compound_copy == null)
            {
                if (Destination.Compound != null)
                    return true;
            }
            else if (Destination.Compound == null)
                return true;
            else if (!destination_compound_copy.Equals(Destination.Compound))
                return true;
                
            return base.IsInvalidated;
        }
    }

    public MoveToSlotAction(Cell.Slot catalyst_slot, 
                            Cell.Slot source, Cell.Slot destination, float quantity = -1)
        : base(catalyst_slot, source, destination, quantity)
    {
        if (Destination.Compound != null)
            destination_compound_copy = Destination.Compound.Copy();
    }

    public MoveToSlotAction(Cell.Slot catalyst_slot, 
                            Cell.Slot source, Cell.Slot.Relation direction, float quantity = -1)
        : base(catalyst_slot, source, source.GetAdjacentSlot(direction), quantity)
    {
        if (Destination.Compound != null)
            destination_compound_copy = Destination.Compound.Copy();
    }

    public override void End()
    {
        if (Destination.Compound == null || Destination.Compound.Molecule.Equals(MovedCompound.Molecule))
            Destination.AddCompound(MovedCompound);

        //Collision
        else
        {
            Mess mess = new Mess(Destination.RemoveCompound(), MovedCompound);

            Destination.AddCompound(new Compound(mess, 1));
        }
    }
}

public class MoveToCytosolAction : MoveAction<Cytosol>
{
    public MoveToCytosolAction(Cell.Slot catalyst_slot, 
                               Cell.Slot source, float quantity = -1)
        : base(catalyst_slot, source, source.Cell.Organism.Cytosol, quantity)
    {

    }

    public override void End()
    {
        Destination.AddCompound(MovedCompound);
    }
}

public class MoveToLocaleAction : MoveAction<Locale>
{
    public MoveToLocaleAction(Cell.Slot catalyst_slot, 
                              Cell.Slot source, float quantity = -1)
        : base(catalyst_slot, source, source.Cell.Organism.Locale, quantity)
    {

    }

    public override void End()
    {
        if (!(Destination is WaterLocale))
            throw new System.NotImplementedException();

        (Destination as WaterLocale).Solution.AddCompound(MovedCompound);
    }
}

public class PushAction : CompositeAction
{
    MoveToSlotAction pushing_move_action = null;
    public Compound PushingCompound
    {
        get
        {
            if (pushing_move_action == null)
                return null;

            return pushing_move_action.MovedCompound;
        }
    }

    public bool IsFullPush { get; private set; }

    public override bool IsLegal
    {
        get
        {
            foreach (Action action in Actions)
                if (action is MoveToSlotAction)
                    if ((action as MoveToSlotAction).IsInvalidated)
                        return false;

            return true;
        }
    }

    public PushAction(Cell.Slot catalyst_slot, Cell.Slot source, Cell.Slot.Relation direction)
        : base(catalyst_slot)
    {
        List<Action> actions = new List<Action>();

        if (direction == Cell.Slot.Relation.Across)
        {
            if (source.AcrossSlot == null)
                actions.Add(new MoveToLocaleAction(catalyst_slot, source));
            else
            {
                MoveToSlotAction move_action = new MoveToSlotAction(catalyst_slot, source, direction);
                actions.Add(move_action);
                pushing_move_action = move_action;

                Cell.Slot destination = source.AcrossSlot;
                if (destination.Compound != null && !destination.Compound.Molecule.Equals(source.Compound.Molecule))
                    actions.Add(new MoveToCytosolAction(catalyst_slot, destination));
            }
        }
        else
        {
            Cell.Slot current_source = source;

            while (true)
            {
                MoveToSlotAction move_action = new MoveToSlotAction(catalyst_slot, current_source, direction);
                actions.Add(move_action);
                if (pushing_move_action == null)
                    pushing_move_action = move_action;

                Cell.Slot destination = move_action.Destination;
                if (destination.Compound == null)
                    break;
                if (destination.Compound.Molecule.Equals(current_source.Compound.Molecule))
                    break;

                current_source = (direction == Cell.Slot.Relation.Right) ? current_source.NextSlot : 
                                                                           current_source.PreviousSlot;
                if (current_source == source)
                {
                    IsFullPush = true;
                    break;
                }
            }
        }

        SetActions(actions);
    }
}


public class ReactionAction : EnergeticAction
{
    protected Dictionary<Cell.Slot, Compound> slot_reactants = new Dictionary<Cell.Slot, Compound>(),
                                              slot_products = new Dictionary<Cell.Slot, Compound>();
    protected List<Compound> cytosol_reactants = new List<Compound>(),
                             cytosol_products = new List<Compound>();

    public IEnumerable<Cell.Slot> ReactantSlots { get { return slot_reactants.Keys; } }
    public IEnumerable<Cell.Slot> ProductSlots { get { return slot_products.Keys; } }

    public override bool IsLegal
    {
        get
        {
            foreach (Cell.Slot destination in slot_products.Keys)
                if (destination.Compound != null &&
                    destination.Compound.Molecule != slot_products[destination].Molecule)
                    return false;

            foreach (Cell.Slot source in slot_reactants.Keys)
                if (source.Compound == null ||
                    source.Compound.Molecule != slot_reactants[source].Molecule ||
                    source.Compound.Quantity < slot_reactants[source].Quantity)
                    return false;

            foreach (Compound reactant in cytosol_reactants)
                if (Organism.Cytosol.GetQuantity(reactant.Molecule) < reactant.Quantity)
                    return false;

            return base.IsLegal;
        }
    }

    public ReactionAction(Cell.Slot catalyst_slot,
                    Dictionary<Cell.Slot, Compound> slot_reactants_, 
                    Dictionary<Cell.Slot, Compound> slot_products_, 
                    List<Compound> cytosol_reactants_, 
                    List<Compound> cytosol_products_, 
                    float atp_balance,
                    float cost = 1) : base(catalyst_slot, cost, atp_balance)
    {
        if(slot_reactants_ != null)
            slot_reactants = slot_reactants_;

        if (slot_products_ != null)
            slot_products = slot_products_;

        if (cytosol_reactants_ != null)
            cytosol_reactants = cytosol_reactants_;

        if (cytosol_products_ != null)
            cytosol_products = cytosol_products_;
    }

    public override void Begin()
    {
        base.Begin();

        foreach (Cell.Slot source in slot_reactants.Keys)
            source.Compound.Split(slot_reactants[source].Quantity);

        foreach (Compound reactant in cytosol_reactants)
            Organism.Cytosol.RemoveCompound(reactant);
    }

    public override void End()
    {
        foreach (Cell.Slot destination in slot_products.Keys)
            destination.AddCompound(slot_products[destination]);

        foreach (Compound product in cytosol_products)
            Organism.Cytosol.AddCompound(product);
    }

    public List<Cell.Slot> GetReactantSlots()
    {
        return slot_reactants.Keys.ToList();
    }

    public List<Cell.Slot> GetProductSlots()
    {
        return slot_products.Keys.ToList();
    }

    public Compound GetReactant(Cell.Slot catalyst_slot)
    {
        return slot_reactants[catalyst_slot];
    }

    public Compound GetProduct(Cell.Slot catalyst_slot)
    {
        return slot_products[catalyst_slot];
    }

    //Want to make these two immutable/readonly lists somehow
    public List<Compound> GetCytosolReactants()
    {
        return cytosol_reactants;
    }

    public List<Compound> GetCytosolProducts()
    {
        return cytosol_products;
    }
}