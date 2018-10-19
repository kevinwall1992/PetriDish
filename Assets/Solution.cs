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
        get { return -Mathf.Log(GetConcentration("Hydronium")); }
    }

    public Compound Water
    {
        get { return GetCompound("Water"); }
    }

    public Compound GetCompound(Molecule molecule)
    {
        if (!compounds.ContainsKey(molecule))
            compounds[molecule] = new Compound(molecule, 0);

        return compounds[molecule];
    }

    public Compound GetCompound(string name)
    {
        return GetCompound(Molecule.GetMolecule(name));
    }

    public void AddCompound(Compound compound)
    {
        GetCompound(compound.Molecule).Quantity += compound.Quantity;
        heat +=  Temperature * compound.Molecule.Mass* compound.Quantity;
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
        return GetCompound(molecule).Quantity/ (Water.Quantity/ 55);
    }

    public float GetConcentration(string name)
    {
        return GetConcentration(Molecule.GetMolecule(name));
    }
}
