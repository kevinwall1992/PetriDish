using UnityEngine;


//Quantities are in "Smoles"
//See Measures.cs for conversion to Moles
public class Compound : Copiable<Compound>
{
    Molecule molecule;
    float quantity;

    public Molecule Molecule
    {
        get
        {
            return molecule;
        }
    }

    public float Quantity
    {
        get { return quantity; }

        set { quantity = value; }
    }

    public Compound(Molecule molecule_, float quantity_)
    {
        molecule = molecule_;
        quantity = quantity_;
    }

    public Compound Split(float quantity)
    {
        quantity = Mathf.Min(quantity, Quantity);

        Quantity -= quantity;

        return new Compound(Molecule, quantity);
    }

    public Compound Copy()
    {
        return new Compound(Molecule.Copy(), Quantity);
    }
}
