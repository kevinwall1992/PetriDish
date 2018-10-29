using UnityEngine;
using System.Collections.Generic;

//Need to add support for arbitrarily large amounts of water/ solvent
public class Solution : Volume
{
    class Buffer
    {
        Molecule molecule;
        Molecule conjugate;

        public Molecule Molecule { get { return molecule; } }
        public Molecule Conjugate { get { return conjugate; } }

        public bool IsAcid
        {
            get { return (conjugate.Charge - molecule.Charge) < 0; }
        }

        public Buffer(Molecule molecule_, Molecule conjugate_)
        {
            molecule = molecule_;
            conjugate = conjugate_;

            Debug.Assert((int)(Mathf.Abs(molecule.Charge - conjugate.Charge) + 0.5f) == 1);
        }
    }

    static List<Buffer> buffers = Utility.CreateList(new Buffer(Molecule.Water, Molecule.Hydronium), 
                                                     new Buffer(Molecule.Water, Molecule.Hydroxide), 
                                                     new Buffer(Molecule.CarbonicAcid, Molecule.Bicarbonate));

    Dictionary<Molecule, Compound> compounds= new Dictionary<Molecule, Compound>();

    float heat = 0;

    public List<Molecule> Molecules { get { return new List<Molecule>(compounds.Keys); } }

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
                     14 - pH;
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

    public Solution(float water_volume)
    {
        AddCompound(Molecule.Water, water_volume);
        heat = 298 * water_volume * Molecule.Water.Mass;
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
        if (compound.Quantity == 0)
            return;

        bool is_proton = compound.Molecule.CompareMolecule(Molecule.Proton);
        bool is_proton_or_hydronium = is_proton || compound.Molecule.CompareMolecule(Molecule.Hydronium);
        bool is_primitive_ion = is_proton_or_hydronium || compound.Molecule.CompareMolecule(Molecule.Hydroxide);

        if (is_primitive_ion)
            foreach (Buffer buffer in buffers)
            {
                bool consume_conjugate = is_proton_or_hydronium ? buffer.IsAcid : !buffer.IsAcid;

                Molecule consumed = consume_conjugate ? buffer.Conjugate : buffer.Molecule;
                Molecule produced = consume_conjugate ? buffer.Molecule : buffer.Conjugate;

                if (produced.CompareMolecule(compound.Molecule))
                    continue;

                float quantity = compound.Split(GetCompound(consumed).Quantity).Quantity;

                RemoveCompound(consumed, quantity);
                if(!is_proton)
                    AddCompound(Molecule.Water, quantity);
                AddCompound(produced, quantity);

                if (compound.Quantity == 0)
                    break;
            }

        GetCompound(compound.Molecule).Quantity += compound.Quantity;
        heat +=  Temperature * compound.Molecule.Mass* compound.Quantity;

        if (!is_primitive_ion)
        {
            AddCompound(RemoveCompound(Molecule.Proton));
            AddCompound(RemoveCompound(Molecule.Hydronium));
            AddCompound(RemoveCompound(Molecule.Hydroxide));
        }
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

    public Compound RemoveCompound(Molecule molecule, float quantity= -1)
    {
        return RemoveCompound(new Compound(molecule, quantity < 0 ? GetQuantity(molecule) : quantity));
    }

    public float GetConcentration(Molecule molecule)
    {
        return GetCompound(molecule).Quantity/ MiniLiters;
    }

    public float GetQuantityPerArea(Molecule molecule)
    {
        return (50000000.0f / 3) * 
               molecule.AtomCount * GetQuantity(molecule) /
               MathUtility.Sum(Molecules, delegate (Molecule molecule_) { return molecule_.AtomCount * GetQuantity(molecule_); });
    }
}
