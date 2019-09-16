using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

public class Mess : Molecule
{
    Dictionary<Molecule, Compound> compounds = new Dictionary<Molecule, Compound>();

    public IEnumerable<Compound> Compounds { get { return compounds.Values; } }

    public override float Enthalpy
    {
        get { return MathUtility.Sum(Compounds, (compound) => (compound.Molecule.Enthalpy * compound.Quantity)); }
    }

    public override Dictionary<Element, int> Elements
    {
        get
        {
            Dictionary<Element, float> elements = new Dictionary<Element, float>();

            foreach(Compound compound in Compounds)
                foreach(Element element in compound.Molecule.Elements.Keys)
                {
                    if (!elements.ContainsKey(element))
                        elements[element] = 0;

                    elements[element] += compound.Molecule.Elements[element] * compound.Quantity;
                }

            Dictionary<Element, int> rounded_elements = new Dictionary<Element, int>();
            foreach (Element element in elements.Keys)
                rounded_elements[element] = (int)elements[element];

            return rounded_elements;
        }
    }

    public Mess(params Compound[] compounds_)
    {
        foreach (Compound compound in compounds_)
            AddToMess(compound);
    }

    public void AddToMess(Compound compound)
    {
        if (!compounds.ContainsKey(compound.Molecule))
            compounds[compound.Molecule] = compound;
        else
            compounds[compound.Molecule].Quantity += compound.Quantity;
    }

    public override bool IsStackable(object obj)
    {
        if (!base.IsStackable(obj))
            return false;

        if (!(obj is Mess))
            return false;

        return Utility.SetEquality(Compounds, (obj as Mess).Compounds);
    }

    public override JObject EncodeJson()
    {
        JArray json_compound_array = new JArray();
        foreach (Compound compound in Compounds)
            json_compound_array.Add(compound.EncodeJson());

        return JObject.FromObject(Utility.CreateDictionary<string, object>("Type", "Mess", "Compounds", json_compound_array));
    }

    public override void DecodeJson(JObject json_mess_object)
    {
        foreach (var json_compound_token in json_mess_object["Compounds"] as JArray)
            AddToMess(Compound.DecodeCompound(json_compound_token as JObject));
    }
}
