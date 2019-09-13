using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class Molecule : Copiable<Molecule>, Stackable, Encodable
{
    static Dictionary<string, Molecule> molecules = new Dictionary<string, Molecule>();

    public static IEnumerable<Molecule> Molecules { get { return molecules.Values; } }

    public static Molecule Water { get; private set; }
    public static ChargeableMolecule NRG { get; private set; }

    static void LoadMolecules(string filename)
    {
        JObject molecules_file = JObject.Parse(Resources.Load<TextAsset>("Molecules/" + filename).text);

        if (molecules_file["Molecules"] != null)
        {
            JObject molecules = molecules_file["Molecules"] as JObject;
            foreach (var molecule in molecules)
                Molecule.RegisterNamedMolecule(molecule.Key, new SimpleMolecule(molecule.Value["Formula"].ToString(),
                                                                                Utility.JTokenToFloat(molecule.Value["Enthalpy"])));
        }
    }

    static Molecule()
    {
        LoadMolecules("default");

        Water = GetMolecule("Water");
        NRG = RegisterNamedMolecule("NRG", new ChargeableMolecule(GetMolecule("NRG"), 1000));

        RegisterNamedMolecule("Valanine", Nucleotide.Valanine);
        RegisterNamedMolecule("Comine", Nucleotide.Comine);
        RegisterNamedMolecule("Funcosine", Nucleotide.Funcosine);
        RegisterNamedMolecule("Locomine", Nucleotide.Locomine);

        RegisterNamedMolecule("Phlorodine", AminoAcid.Phlorodine);
        RegisterNamedMolecule("Umine", AminoAcid.Umine);
        RegisterNamedMolecule("Aquine", AminoAcid.Aquine);
    }

    public static T RegisterNamedMolecule<T>(string name, T molecule) where T : Molecule
    {
        molecules[name] = molecule;

        return molecule;
    }

    public static bool DoesMoleculeExist(string name)
    {
        return molecules.ContainsKey(name);
    }

    public static Molecule GetMolecule(string name)
    {
        if (!DoesMoleculeExist(name))
            return null;

        return molecules[name].Copy();
    }


    public int Mass
    {
        get
        {
            int total_mass = 0;

            foreach (Element element in Elements.Keys)
                total_mass += element.Mass * Elements[element];

            return total_mass;
        }
    }

    public abstract float Enthalpy { get; }

    public virtual string Name
    {
        get
        {
            foreach (string name in molecules.Keys)
                if (Equals(molecules[name], this))
                    return name;

            return "Unnamed";
        }
    }

    public abstract Dictionary<Element, int> Elements
    {
        get;
    }

    public int AtomCount
    {
        get { return MathUtility.Sum(new List<int>(Elements.Values), (count) => (count)); }
    }

    public Molecule()
    {

    }

    public virtual bool IsStackable(object obj)
    {
        if (this == obj)
            return true;

        if (!(obj is Molecule))
            return false;

        if (obj is Polymer)
            return false;

        Molecule other = obj as Molecule;

        foreach (Element element in this.Elements.Keys)
            if (!other.Elements.ContainsKey(element) ||
                this.Elements[element] != other.Elements[element])
                return false;

        foreach (Element element in other.Elements.Keys)
            if (!this.Elements.ContainsKey(element) ||
                other.Elements[element] != this.Elements[element])
                return false;

        return true;
    }

    public override bool Equals(object obj)
    {
        return IsStackable(obj);
    }

    public override int GetHashCode()
    {
        int hash = 17;

        foreach (Element element in Elements.Keys)
            hash = hash * 23 + element.GetHashCode() * Elements[element];

        return hash;
    }

    //In most cases, molecules are immutable singletons,
    //So here we simply return this
    public virtual Molecule Copy()
    {
        return this;
    }


    public abstract JObject EncodeJson();
    public abstract void DecodeJson(JObject json_object);

    public static Molecule DecodeMolecule(JObject json_object)
    {
        Molecule molecule;

        switch (Utility.JTokenToString(json_object["Type"]))
        {
            case "Simple Molecule": molecule = new SimpleMolecule("", 0); break;
            case "Chargeable Molecule": molecule = new ChargeableMolecule(null, 0); break;
            case "Ribozyme": molecule = new Ribozyme(null); break;
            case "Enzyme": molecule = new Enzyme(null); break;
            case "DNA": molecule = new DNA(); break;
            case "Nucleotide": molecule = new Nucleotide(); break;
            case "Amino Acid": molecule = new AminoAcid(null, ""); break;
            case "Mess": molecule = new Mess(); break;

            default: return null;
        }

        molecule.DecodeJson(json_object);

        return molecule;
    }
}

public class SimpleMolecule : Molecule
{
    Dictionary<Element, int> elements = new Dictionary<Element, int>();
    float enthalpy;

    public override float Enthalpy { get { return enthalpy; } }

    public override Dictionary<Element, int> Elements { get { return elements; } }

    public SimpleMolecule(string formula, float enthalpy_)
    {
        MatchCollection match_collection = Regex.Matches(formula, "([A-Z][a-z]?)([0-9]*) *");
        foreach (Match match in match_collection)
        {
            string element_key = match.Groups[1].Value;
            int count;
            if (!System.Int32.TryParse(match.Groups[2].Value, out count))
                count = 1;

            Element element = Element.elements[element_key];

            if (!elements.ContainsKey(element))
                elements[element] = 0;

            elements[element] += count;
        }

        enthalpy = enthalpy_;
    }

    public override JObject EncodeJson()
    {
        return JObject.FromObject(Utility.CreateDictionary<string, string>("Type", "Simple Molecule",
                                                                           "Name", Name));
    }

    public override void DecodeJson(JObject json_object)
    {
        SimpleMolecule other = GetMolecule(Utility.JTokenToString(json_object["Name"])) as SimpleMolecule;
        Debug.Assert(other != null);

        elements = new Dictionary<Element, int>(other.elements);
        enthalpy = other.enthalpy;
    }
}
