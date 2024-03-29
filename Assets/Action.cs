﻿using System.Collections.Generic;
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

            base_scale = 1;
        }
    }

    public Cell.Slot CatalystSlot { get; private set; }
    public Catalyst Catalyst { get; private set; }

    public Cell Cell { get { return CatalystSlot.Cell; } }
    public Organism Organism { get { return Cell.Organism; } }
    public Cytosol Cytosol { get { return Organism.Cytosol; } }

    float base_scale = 1;
    public virtual float Scale
    {
        get { return Mathf.Min(base_scale * StackScale, MaxScale); }
    }

    public float MaxScale { get; set; }

    float StackScale
    {
        get
        {
            Dictionary<Cell.Slot, float> stack_increases = GetStackIncreases();

            float max_ratio = 1;
            foreach (Cell.Slot slot in stack_increases.Keys)
            {
                float current_stack_size = slot.Compound == null ? 0 : slot.Compound.Quantity;
                float adjusted_stack_increase = Mathf.Pow(300 * stack_increases[slot] * base_scale + Mathf.Pow(current_stack_size, 3), 0.333333f) - current_stack_size;

                max_ratio = Mathf.Min(max_ratio, adjusted_stack_increase / stack_increases[slot]);
            }

            return max_ratio;
        }
    }

    public virtual float Cost
    {
        get { return BaseCost * base_scale; }

        set
        {
            if (BaseCost != 0)
                base_scale = value / BaseCost;
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

        MaxScale = float.MaxValue;

        BaseCost = cost;
    }

    public virtual Dictionary<object, List<Compound>> GetResourceDemands()
    {
        return new Dictionary<object, List<Compound>>();
    }

    //Quantities returned should be unscaled.
    protected virtual Dictionary<Cell.Slot, float> GetStackIncreases()
    {
        return new Dictionary<Cell.Slot, float>();
    }

    public virtual void ScaleByFactor(float factor)
    {
        base_scale *= factor;
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
                            typeof(PumpAction),
                            typeof(Interpretase.CopyCommand),
                            typeof(Interpretase.LoadProgram)),

                new Stage(typeof(Interpretase.GrabCommand),
                            typeof(Interpretase.ReleaseCommand)),

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


//This implementation assumes that component 
//actions do not change cost after creation.
public class CompositeAction : Action
{
    List<Action> actions= new List<Action>();

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

    public override void ScaleByFactor(float factor)
    {
        throw new System.NotImplementedException();
    }

    protected void SetActions(IEnumerable<Action> actions)
    {
        Debug.Assert(this.actions.Count == 0, "CompositeAction : Attempted to set actions more than once.");

        this.actions.AddRange(actions);
        this.actions.RemoveAll(action => action == null);

        BaseCost += MathUtility.Sum(this.actions, (action) => (action.Cost));
    }

    public override Dictionary<object, List<Compound>> GetResourceDemands()
    {
        Dictionary<object, List<Compound>> demands = new Dictionary<object, List<Compound>>();

        foreach (Action action in actions)
        {
            Dictionary<object, List<Compound>> child_demands = action.GetResourceDemands();

            foreach (object resource in child_demands.Keys)
            {
                if (!demands.ContainsKey(resource))
                    demands[resource] = new List<Compound>();

                demands[resource].AddRange(child_demands[resource]);
            }
        }

        return demands;
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

//should we actually be working in kJ/mol, then convert, or just work in nrg equivalents?****
public class EnergeticAction : Action
{
    float base_energy_change;

    public float EnergyChange
    {
        get { return base_energy_change * Scale; }
        protected set { base_energy_change = value; }
    }

    bool IsExergonic { get { return EnergyChange > 0; } }

    public EnergeticAction(Cell.Slot catalyst_slot, float cost, float energy_change)
        : base(catalyst_slot, cost)
    {
        EnergyChange = energy_change;
    }

    public override Dictionary<object, List<Compound>> GetResourceDemands()
    {
        Compound compound = new Compound(EnergyChange < 0 ? Molecule.ChargedNRG : Molecule.DischargedNRG, Mathf.Abs(EnergyChange));

        return Utility.CreateDictionary<object, List<Compound>>(
            Organism.Cytosol, Utility.CreateList(compound));
    }

    public override void Begin()
    {
        if (IsExergonic)
        {
            if (Cytosol.GetQuantity(Molecule.DischargedNRG) < EnergyChange)
                return;

            Cytosol.RemoveCompound(Molecule.DischargedNRG, EnergyChange);
        }
        else
        {
            if (Cytosol.GetQuantity(Molecule.ChargedNRG) < -EnergyChange)
                return;

            Cytosol.RemoveCompound(Molecule.ChargedNRG, -EnergyChange);
        }

        base.Begin();
    }

    public override void End()
    {
        if (IsExergonic)
            Cytosol.AddCompound(Molecule.ChargedNRG, EnergyChange);
        else
            Cytosol.AddCompound(Molecule.DischargedNRG, -EnergyChange);

        base.End();
    }
}

public abstract class MoveAction<T> : EnergeticAction
{
    float DesiredQuantity { get { return Scale; } }

    public Cell.Slot Source { get; private set; }
    public T Destination { get; private set; }

    public Compound MovedCompound { get; private set; }

    public override bool IsLegal
    {
        get
        {
            if (Source.Compound == null)
                return false;

            if (Source.Compound.Quantity < DesiredQuantity)
                return false;

            return base.IsLegal;
        }
    }

    protected MoveAction(Cell.Slot catalyst_slot, Cell.Slot source, T destination, float quantity)
        : base(catalyst_slot, Balance.Actions.CompoundMovement.Cost, Balance.Actions.CompoundMovement.EnergyChange)
    {
        Source = source;

        Destination = destination;

        if (quantity < 0)
            quantity = source.Compound.Quantity;
        ScaleByFactor(quantity);
    }

    public override void Begin()
    {
        base.Begin();

        MovedCompound = Source.Compound.Split(DesiredQuantity);
    }
}

public class MoveToSlotAction : MoveAction<Cell.Slot>
{
    public MoveToSlotAction(Cell.Slot catalyst_slot, 
                            Cell.Slot source, Cell.Slot destination, float quantity = -1)
        : base(catalyst_slot, source, destination, quantity)
    {

    }

    public MoveToSlotAction(Cell.Slot catalyst_slot, 
                            Cell.Slot source, Cell.Slot.Relation direction, float quantity = -1)
        : base(catalyst_slot, source, source.GetAdjacentSlot(direction), quantity)
    {

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

        base.End();
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

        base.End();
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

        base.End();
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
    protected Dictionary<Cell.Slot, Compound> slot_reactants, slot_products;
    protected List<Compound> cytosol_reactants, cytosol_products;
    protected List<Compound> locale_reactants, locale_products;

    public IEnumerable<Cell.Slot> ReactantSlots { get { return slot_reactants.Keys; } }
    public IEnumerable<Cell.Slot> ProductSlots { get { return slot_products.Keys; } }

    public override bool IsLegal
    {
        get
        {
            foreach (Cell.Slot destination in slot_products.Keys)
                if (destination.Compound != null &&
                    !destination.Compound.Molecule.IsStackable(slot_products[destination].Molecule))
                    return false;

            foreach (Cell.Slot source in slot_reactants.Keys)
                if (source.Compound == null || 
                    !source.Compound.Molecule.IsStackable(slot_reactants[source].Molecule))
                    return false;

            return base.IsLegal;
        }
    }

    public ReactionAction(Cell.Slot catalyst_slot,
                    Dictionary<Cell.Slot, Compound> slot_reactants_,
                    Dictionary<Cell.Slot, Compound> slot_products_,
                    List<Compound> cytosol_reactants_,
                    List<Compound> cytosol_products_,
                    List<Compound> locale_reactants_,
                    List<Compound> locale_products_,
                    float energy_change, 
                    float cost = 1) : base(catalyst_slot, cost * Balance.Actions.Reaction.Cost, energy_change)
    {
        slot_reactants = slot_reactants_;
        slot_products = slot_products_;
        if (slot_reactants == null) slot_reactants = new Dictionary<Cell.Slot, Compound>();
        if (slot_products == null) slot_products = new Dictionary<Cell.Slot, Compound>();

        cytosol_reactants = cytosol_reactants_;
        cytosol_products = cytosol_products_;
        if (cytosol_reactants == null) cytosol_reactants = new List<Compound>();
        if (cytosol_products == null) cytosol_products = new List<Compound>();

        locale_reactants = locale_reactants_;
        locale_products = locale_products_;
        if (locale_reactants == null) locale_reactants = new List<Compound>();
        if (locale_products == null) locale_products = new List<Compound>();


        //This adjusts Action.BaseScale based on transport rates of 
        //compounds being moved across the membrane.
        float max_ratio = 1;

        System.Func<Compound, bool, float> GetTransportRate = delegate (Compound compound, bool transport_out)
        {
            Molecule molecule = compound.Molecule;
            float quantity = compound.Quantity;

            float max_rate = Organism.Membrane.GetTransportRate(molecule, false) * quantity;

            return Mathf.Min(quantity, max_rate);
        };

        foreach (Compound compound in locale_reactants)
            max_ratio = Mathf.Min(max_ratio, GetTransportRate(compound, false) / (compound.Quantity));

        foreach (Compound compound in locale_products)
            max_ratio = Mathf.Min(max_ratio, GetTransportRate(compound, true) / (compound.Quantity));

        ScaleByFactor(max_ratio);
    }

    public override Dictionary<object, List<Compound>> GetResourceDemands()
    {
        Dictionary<object, List<Compound>> demands = base.GetResourceDemands();

        foreach (Cell.Slot slot in slot_reactants.Keys)
        {
            if (!demands.ContainsKey(slot))
                demands[slot] = new List<Compound>();

            demands[slot].Add(slot_reactants[slot] * Scale);
        }

        foreach (Compound compound in cytosol_reactants)
            demands[Organism.Cytosol].Add(compound * Scale);

        foreach (Compound compound in locale_reactants)
        {
            if (!demands.ContainsKey(Organism.Locale))
                demands[Organism.Locale] = new List<Compound>();

            demands[Organism.Locale].Add(compound * Scale);
        }

        return demands;
    }

    protected override Dictionary<Cell.Slot, float> GetStackIncreases()
    {
        Dictionary<Cell.Slot, float> stack_increases = new Dictionary<Cell.Slot, float>();

        foreach (Cell.Slot slot in slot_products.Keys)
            stack_increases[slot] = slot_products[slot].Quantity;

        return stack_increases;
    }

    public override void Begin()
    {
        base.Begin();

        foreach (Cell.Slot source in slot_reactants.Keys)
            source.Compound.Split(slot_reactants[source].Quantity * Scale);
         
        foreach (Compound reactant in cytosol_reactants)
            Organism.Cytosol.RemoveCompound(reactant * Scale);

        Debug.Assert(Organism.Locale is WaterLocale);
        foreach (Compound reactant in locale_reactants)
            (Organism.Locale as WaterLocale).Solution.RemoveCompound(reactant * Scale);
    }

    public override void End()
    {
        foreach (Cell.Slot destination in slot_products.Keys)
            destination.AddCompound(slot_products[destination] * Scale);

        foreach (Compound product in cytosol_products)
            Organism.Cytosol.AddCompound(product * Scale);

        foreach (Compound product in cytosol_products)
            (Organism.Locale as WaterLocale).Solution.AddCompound(product * Scale);

        base.End();
    }

    public Compound GetReactant(Cell.Slot catalyst_slot)
    {
        return slot_reactants[catalyst_slot] * Scale;
    }

    public Compound GetProduct(Cell.Slot catalyst_slot)
    {
        return slot_products[catalyst_slot] * Scale;
    }

    //Want to make these two immutable/readonly lists somehow
    public List<Compound> GetCytosolReactants()
    {
        List<Compound> scaled_cytosol_reactants = new List<Compound>();
        foreach (Compound compound in cytosol_reactants)
            scaled_cytosol_reactants.Add(compound * Scale);

        return scaled_cytosol_reactants;
    }

    public List<Compound> GetCytosolProducts()
    {
        List<Compound> scaled_cytosol_products = new List<Compound>();
        foreach (Compound compound in cytosol_products)
            scaled_cytosol_products.Add(compound * Scale);

        return scaled_cytosol_products;
    }

    public List<Compound> GetLocaleReactants()
    {
        List<Compound> scaled_locale_reactants = new List<Compound>();
        foreach (Compound compound in locale_reactants)
            scaled_locale_reactants.Add(compound * Scale);

        return scaled_locale_reactants;
    }

    public List<Compound> GetLocaleProducts()
    {
        List<Compound> scaled_locale_products = new List<Compound>();
        foreach (Compound compound in locale_products)
            scaled_locale_products.Add(compound * Scale);

        return scaled_locale_products;
    }
}