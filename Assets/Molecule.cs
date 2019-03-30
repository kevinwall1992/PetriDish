using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class Molecule : Copiable<Molecule>, Stackable
{
    static Dictionary<string, Molecule> molecules = new Dictionary<string, Molecule>();

    public static IEnumerable<Molecule> Molecules { get { return molecules.Values; } }

    public static Molecule Oxygen { get; private set; }
    public static Molecule CarbonDioxide { get; private set; }
    public static Molecule Nitrogen { get; private set; }
    public static Molecule Hydrogen { get; private set; }
    public static Molecule Water { get; private set; }
    public static Molecule Proton { get; private set; }
    public static Molecule Hydronium { get; private set; }
    public static Molecule Hydroxide { get; private set; }
    public static Molecule Salt { get; private set; }
    public static Molecule Glucose { get; private set; }
    public static Molecule ATP { get; private set; }
    public static Molecule ADP { get; private set; }
    public static Molecule Phosphate { get; private set; }
    public static Molecule CarbonicAcid { get; private set; }
    public static Molecule Bicarbonate { get; private set; }
    public static Molecule Imidazole { get; private set; }
    public static Molecule Methane { get; private set; }

    static void LoadMolecules(string filename)
    {
        JObject molecules_file = JObject.Parse(Resources.Load<TextAsset>(filename).text);

        if (molecules_file["Molecules"] != null)
        {
            JObject molecules = molecules_file["Molecules"] as JObject;
            foreach (var molecule in molecules)
                Molecule.RegisterNamedMolecule(molecule.Key, new SimpleMolecule(molecule.Value["Formula"].ToString(),
                                                                                Utility.JTokenToFloat(molecule.Value["Enthalpy"]),
                                                                                Utility.JTokenToInt(molecule.Value["Charge"])));
        }
    }

    static Molecule()
    {
        LoadMolecules("molecules");

        CarbonDioxide = GetMolecule("Carbon Dioxide");
        Oxygen = GetMolecule("Oxygen");
        Nitrogen = GetMolecule("Nitrogen");
        Hydrogen = GetMolecule("Hydrogen");

        Water = GetMolecule("Water");
        Proton = GetMolecule("Proton");
        Hydronium = GetMolecule("Hydronium");
        Hydroxide = GetMolecule("Hydroxide");

        Salt = GetMolecule("Salt");
        Glucose = GetMolecule("Glucose");

        ATP = GetMolecule("ATP");
        ADP = GetMolecule("ADP");
        Phosphate = GetMolecule("Phosphate");

        CarbonicAcid = GetMolecule("CarbonicAcid");
        Bicarbonate = GetMolecule("Bicarbonate");

        Imidazole = GetMolecule("Imidazole");
        Methane = GetMolecule("Methane");

        RegisterNamedMolecule("AMP", Nucleotide.AMP);
        RegisterNamedMolecule("CMP", Nucleotide.CMP);
        RegisterNamedMolecule("GMP", Nucleotide.GMP);
        RegisterNamedMolecule("TMP", Nucleotide.TMP);

        RegisterNamedMolecule("Histidine", AminoAcid.Histidine);
        RegisterNamedMolecule("Alanine", AminoAcid.Alanine);
        RegisterNamedMolecule("Serine", AminoAcid.Serine);
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

        return molecules[name];
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

    public abstract int Charge { get; }

    public abstract float Enthalpy { get; }

    public virtual string Name
    {
        get
        {
            foreach (string name in molecules.Keys)
                if (ReferenceEquals(molecules[name], this))
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

        if (this.Name != "Unnamed" || other.Name != "Unnamed")
            return this.Name == other.Name;

        foreach (Element element in this.Elements.Keys)
            if (!other.Elements.ContainsKey(element) ||
                this.Elements[element] != other.Elements[element])
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
}

public class SimpleMolecule : Molecule
{
    Dictionary<Element, int> elements = new Dictionary<Element, int>();
    int charge;
    float enthalpy;

    public override int Charge { get { return charge; } }

    public override float Enthalpy { get { return enthalpy; } }

    public override Dictionary<Element, int> Elements { get { return elements; } }

    public SimpleMolecule(string formula, float enthalpy_, int charge_ = 0)
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

        charge = charge_;
        enthalpy = enthalpy_;
    }
}
