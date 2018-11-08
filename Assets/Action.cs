using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


public abstract class Action
{
    Cell.Slot slot;

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

    

    public bool HasFailed { get; private set; }

    public Action(Cell.Slot slot_)
    {
        slot = slot_;
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

    public List<Action> Actions { get { return actions; } }


    public CompositeAction(Cell.Slot slot, params Action[] actions_) : base(slot)
    {
        actions.AddRange(actions_);
    }

    public void AddAction(Action action)
    {
        actions.Add(action);
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

    public override bool Prepare()
    {
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

    public ATPConsumptionAction(Cell.Slot slot, float quantity, Cell.Slot atp_slot)
        : base(slot, 
                Utility.CreateDictionary<Cell.Slot, Compound>(atp_slot, new Compound(Molecule.ATP, quantity)), null,
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


public class PoweredAction : CompositeAction
{
    public PoweredAction(Cell.Slot slot, Cell.Slot atp_slot, float cost, Action action) : base(slot)
    {
        AddAction(action);
        AddAction(new ATPConsumptionAction(slot, cost, atp_slot));
    }
}