using Newtonsoft.Json.Linq;
using UnityEngine;


//Quantities are in "Smoles"
//See Measures.cs for conversion to Moles
public class Compound : Copiable<Compound>, Encodable
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

    public static Compound operator *(Compound compound, float scalar)
    {
        return new Compound(compound.Molecule, compound.Quantity * scalar);
    }

    public static Compound operator /(Compound compound, float scalar)
    {
        return compound * (1 / scalar);
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Compound))
            return false;

        Compound other = obj as Compound;

        return other.Molecule.Equals(Molecule) && 
               other.Quantity == Quantity;
    }

    public Compound Copy()
    {
        return new Compound(Molecule.Copy(), Quantity);
    }


    public JObject EncodeJson()
    {
        return JObject.FromObject(Utility.CreateDictionary<string, object>("Molecule", Molecule.EncodeJson(), "Quantity", Quantity));
    }

    public void DecodeJson(JObject json_compound_object)
    {
        molecule = Molecule.DecodeMolecule(json_compound_object["Molecule"] as JObject);
        quantity = Utility.JTokenToFloat(json_compound_object["Quantity"]);
    }

    public static Compound DecodeCompound(JObject json_compound_object)
    {
        Compound compound = new Compound(null, 0);
        compound.DecodeJson(json_compound_object);

        return compound;
    }
}
