using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;


public abstract class Molecule
{
    static Dictionary<string, Molecule> molecules = new Dictionary<string, Molecule>();

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

    //Consider phasing this out once we get data for this stuff working
    static Molecule()
    {
        CarbonDioxide = RegisterNamedMolecule("Carbon Dioxide", new SimpleMolecule("C O2", -393.5f));
        Oxygen = RegisterNamedMolecule("Oxygen", new SimpleMolecule("O2", 0));
        Nitrogen = RegisterNamedMolecule("Nitrogen", new SimpleMolecule("N2", 0));
        Hydrogen = RegisterNamedMolecule("Hydrogen", new SimpleMolecule("H2", 0));

        Water = RegisterNamedMolecule("Water", new SimpleMolecule("H2 O", -285.3f));
        Proton = RegisterNamedMolecule("Proton", new SimpleMolecule("H", 0.0f, 1));
        Hydronium = RegisterNamedMolecule("Hydronium", new SimpleMolecule("H3 O", -265.0f, 1));
        Hydroxide = RegisterNamedMolecule("Hydroxide", new SimpleMolecule("H O", -229.9f, -1));

        Salt = RegisterNamedMolecule("Salt", new SimpleMolecule("Na Cl", -411.1f));
        Glucose = RegisterNamedMolecule("Glucose", new SimpleMolecule("C6 H12 O6", -1271));

        ATP = RegisterNamedMolecule("ATP", new SimpleMolecule("C10 H12 N5 O13 P3", -2995.6f, -4));
        ADP = RegisterNamedMolecule("ADP", new SimpleMolecule("C10 H12 N5 O10 P2", -2005.9f, -3));
        Phosphate = RegisterNamedMolecule("Phosphate", new SimpleMolecule("P O4 H2", -1308.0f, -1));

        CarbonicAcid = RegisterNamedMolecule("CarbonicAcid", new SimpleMolecule("C H2 O3", 31.5f));
        Bicarbonate = RegisterNamedMolecule("Bicarbonate", new SimpleMolecule("C H O3", 31.5f, -1));

        Imidazole = RegisterNamedMolecule("Imidazole", new SimpleMolecule("C3 H4 N2", 49.8f));
        Methane = RegisterNamedMolecule("Methane", new SimpleMolecule("C H4", -74.9f));

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
        get { return MathUtility.Sum(new List<int>(Elements.Values), delegate (int count) { return count; }); }
    }

    public Molecule()
    {

    }

    public override bool Equals(object other)
    {
        if (!(other is Molecule))
            return false;

        if (other is Polymer)
            return false;

        Molecule other_molecule = other as Molecule;

        if (Name != "Unnamed" || other_molecule.Name != "Unnamed")
            return Name == other_molecule.Name;

        foreach (Element element in this.Elements.Keys)
            if (!other_molecule.Elements.ContainsKey(element) ||
                this.Elements[element] != other_molecule.Elements[element])
                return false;

        return true;
    }

    public override int GetHashCode()
    {
        int hash = 17;

        foreach (Element element in Elements.Keys)
            hash = hash * 23 + element.GetHashCode() * Elements[element];

        return hash;
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
