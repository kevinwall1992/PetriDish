using System.Collections.Generic;


//This Catalyst always returns an action (if able), 
//scaling the effect of the action to fit the 
//productivity of the Catalyst. 
public abstract class InstantCatalyst : ProgressiveCatalyst
{
    public InstantCatalyst(string name, int price, string description = "") : base(name, price, description)
    {

    }

    protected InstantCatalyst()
    {

    }

    public override void Communicate(Cell.Slot slot, Action.Stage stage)
    {
        Action action = GetAction(slot);
        if (action == null || !stage.Includes(action) || !action.IsLegal)
            return;

        action.Cost = slot.Compound.Quantity;

        Dictionary<object, List<Compound>> demands = action.GetResourceDemands();
        foreach (object source in demands.Keys)
            foreach (Compound compound in demands[source])
                MakeClaim(source, compound);
    }

    //Enforce productivity from above?
    public override Action Catalyze(Cell.Slot slot, Action.Stage stage)
    {
        Action action = GetAction(slot);

        if (action == null)
            return null;

        if (!stage.Includes(action))
            return null;

        action.Cost = slot.Compound.Quantity;

        float claim_yield = GetNormalizedClaimYield();
        if (claim_yield == 0)
            return null;
        action.MaxScale = action.Scale * claim_yield;

        if (!action.IsLegal)
            return null;

        return action;
    }
}