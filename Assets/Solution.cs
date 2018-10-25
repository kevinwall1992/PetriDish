using UnityEngine;
using System.Collections.Generic;

public class Solution
{
    Dictionary<Molecule, Compound> compounds= new Dictionary<Molecule, Compound>();

    float heat;

    public float Mass
    {
        get
        {
            float mass = 0;

            foreach (Molecule molecule in compounds.Keys)
                mass += compounds[molecule].Quantity* molecule.Mass;

            return mass;
        }
    }

    public float Temperature
    {
        get { return heat / Mass; }
    }

    public float pH
    {
        get
        {
            return GetCompound(Molecule.Hydroxide).Quantity == 0 ?
                     -Mathf.Log10(GetConcentration(Molecule.Hydronium) + Mathf.Pow(10, -7)) :
                     14 - pOH;
        }
    }

    public float pOH
    {
        get
        {
            return GetCompound(Molecule.Hydronium).Quantity == 0 ?
                     -Mathf.Log10(GetConcentration(Molecule.Hydroxide) + Mathf.Pow(10, -7)) :
                     14 - pOH;
        }
    }

    public Compound Water
    {
        get { return GetCompound(Molecule.Water); }
    }

    public float MiniLiters
    {
        get { return Water.Quantity / 55; }
    }

    float GetChargeBalance()
    {
        float charge_balance = 0;

        foreach (Molecule molecule in compounds.Keys)
            if(!molecule.CompareMolecule(Molecule.Hydronium) && !molecule.CompareMolecule(Molecule.Hydroxide))
                charge_balance += molecule.Charge * compounds[molecule].Quantity;

        return charge_balance;
    }

    Compound GetCompound(Molecule molecule)
    {
        if (!compounds.ContainsKey(molecule))
            compounds[molecule] = new Compound(molecule, 0);

        return compounds[molecule];
    }

    public float GetQuantity(Molecule molecule)
    {
        return GetCompound(molecule).Quantity;
    }

    public void AddCompound(Compound compound)
    {
        if(compound.Molecule.CompareMolecule(Molecule.Proton))
        {
            AddCompound(Molecule.Hydronium, 
                        RemoveCompound(Molecule.Water, 
                                       compound.Split(Mathf.Min(compound.Quantity, Water.Quantity)).Quantity).Quantity);
        }
        else if(compound.Molecule.CompareMolecule(Molecule.Hydronium) && pH > 7)
        {
            float hydroxides_consumed = compound.Split(Mathf.Min(compound.Quantity, GetChargeBalance())).Quantity;

            RemoveCompound(Molecule.Hydroxide, hydroxides_consumed);
            AddCompound(Molecule.Water, hydroxides_consumed * 2);
        }
        else if (compound.Molecule.CompareMolecule(Molecule.Hydroxide) && pH < 7)
        {
            float hydroniums_consumed = compound.Split(Mathf.Min(compound.Quantity, -GetChargeBalance())).Quantity;

            RemoveCompound(Molecule.Hydronium, hydroniums_consumed);
            AddCompound(Molecule.Water, hydroniums_consumed * 2);
        }

        GetCompound(compound.Molecule).Quantity += compound.Quantity;
        heat +=  Temperature * compound.Molecule.Mass* compound.Quantity;

        if (compound.Molecule.CompareMolecule(Molecule.Water))
            AddCompound(RemoveCompound(Molecule.Proton, GetCompound(Molecule.Proton).Quantity));
    }

    public void AddCompound(Molecule molecule, float quantity)
    {
        AddCompound(new Compound(molecule, quantity));
    }

    public Compound RemoveCompound(Compound compound)
    {
        Compound removed_compound = GetCompound(compound.Molecule).Split(compound.Quantity);
        heat -= removed_compound.Molecule.Mass * removed_compound.Quantity;

        return removed_compound;
    }

    public Compound RemoveCompound(Molecule molecule, float quantity)
    {
        return RemoveCompound(new Compound(molecule, quantity));
    }

    public float GetConcentration(Molecule molecule)
    {
        return GetCompound(molecule).Quantity/ MiniLiters;
    }
}
