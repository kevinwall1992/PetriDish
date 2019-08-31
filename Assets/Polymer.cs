using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class Polymer : Molecule
{
    List<Monomer> monomers = new List<Monomer>();

    public List<Monomer> Monomers { get { return monomers; } }

    public override float Enthalpy
    {
        get { return MathUtility.Sum(monomers, (monomer) => (monomer.Enthalpy)); }
    }

    public override Dictionary<Element, int> Elements
    {
        get
        {
            Dictionary<Element, int> elements = new Dictionary<Element, int>();

            foreach (Molecule monomer in monomers)
                foreach (Element element in monomer.Elements.Keys)
                {
                    if (!elements.ContainsKey(element))
                        elements[element] = 0;

                    elements[element] += monomer.Elements[element];
                }

            return elements;
        }
    }



    public Polymer(List<Monomer> monomers_)
    {
        foreach (Monomer monomer in monomers_)
            AppendMonomer(monomer);
    }

    public Polymer()
    {

    }

    public virtual void InsertMonomer(Monomer monomer, int index)
    {
        if (monomers.Count > 0)
            monomer.Condense();

        monomers.Insert(index, monomer);
    }

    public void AppendMonomer(Monomer monomer)
    {
        InsertMonomer(monomer, Monomers.Count);
    }

    public virtual Monomer RemoveMonomer(int index)
    {
        Monomer monomer = monomers[index];
        monomers.RemoveAt(index);

        return monomer;
    }

    public override bool IsStackable(object obj)
    {
        if (this == obj)
            return true;

        if (!(obj is Polymer))
            return false;

        Polymer other = obj as Polymer;

        if (Monomers.Count != other.Monomers.Count)
            return false;

        for (int i = 0; i < Monomers.Count; i++)
            if (Monomers[i] != other.Monomers[i])
                return false;

        return true;
    }

    public override int GetHashCode()
    {
        int hash = 17;

        foreach (Monomer monomer in Monomers)
            hash = hash * 23 + monomer.GetHashCode();

        return hash;
    }

    public override JObject EncodeJson()
    {
        JObject json_polymer_object = new JObject();

        json_polymer_object["Type"] = "Polymer";

        List<JObject> json_monomer_objects = new List<JObject>();
        foreach (Monomer monomer in Monomers)
            json_monomer_objects.Add(monomer.EncodeJson());
        json_polymer_object["Monomers"] = new JObject(json_monomer_objects);

        return json_polymer_object;
    }

    public override void DecodeJson(JObject json_object)
    {
        foreach (JToken json_token in json_object["Monomers"])
            Monomers.Add(Molecule.DecodeMolecule(json_token as JObject) as Monomer);
    }


    public abstract class Monomer : Molecule
    {
        Molecule condensate;
        bool is_condensed = false;

        public Molecule Condensate { get { return condensate; } }
        public bool IsCondensed { get { return is_condensed; } }

        public override Dictionary<Element, int> Elements
        {
            get
            {
                if (!is_condensed)
                    return Elements;
                else
                {
                    Dictionary<Element, int> elements = new Dictionary<Element, int>(Elements);

                    foreach (Element element in condensate.Elements.Keys)
                        elements[element] -= condensate.Elements[element];

                    return elements;
                }
            }
        }

        public Monomer(Molecule condensate_)
        {
            condensate = condensate_;
        }

        public void Condense()
        {
            is_condensed = true;
        }


    }
}
