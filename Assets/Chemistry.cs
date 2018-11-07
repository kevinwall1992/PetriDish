using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;

public class Element
{
    public static Dictionary<string, Element> elements = new Dictionary<string, Element>();

    static Element()
    {
        elements["H"] = new Element("Hydrogen", 1);
        elements["He"] = new Element("Helium", 2);
        elements["Li"] = new Element("Lithium", 3);
        elements["Be"] = new Element("Beryllium", 4);
        elements["B"] = new Element("Boron", 5);
        elements["C"] = new Element("Carbon", 6);
        elements["N"] = new Element("Nitrogen", 7);
        elements["O"] = new Element("Oxygen", 8);
        elements["F"] = new Element("Flourine", 9);
        elements["Ne"] = new Element("Neon", 10);
        elements["Na"] = new Element("Sodium", 11);
        elements["Mg"] = new Element("Magnesium", 12);
        elements["Al"] = new Element("Aluminium", 13);
        elements["Si"] = new Element("Silicon", 14);
        elements["P"] = new Element("Phosphorus", 15);
        elements["S"] = new Element("Sulfur", 16);
        elements["Cl"] = new Element("Chlorine", 17);
        elements["Ar"] = new Element("Argon", 18);
        elements["K"] = new Element("Potassium", 19);
        elements["Ca"] = new Element("Calcium", 20);
        elements["Sc"] = new Element("Scandium", 21);
        elements["Ti"] = new Element("Titanium", 22);
        elements["V"] = new Element("Vanadium", 23);
        elements["Cr"] = new Element("Chromium", 24);
        elements["Mn"] = new Element("Manganese", 25);
        elements["Fe"] = new Element("Iron", 26);
        elements["Co"] = new Element("CobaLT", 27);
        elements["Ni"] = new Element("Nickel", 28);
        elements["Cu"] = new Element("Copper", 29);
        elements["Zn"] = new Element("Zinc", 30);
        elements["Ga"] = new Element("Gallium", 31);
        elements["Ge"] = new Element("Germanium", 32);
        elements["As"] = new Element("Arsenic", 33);
        elements["Se"] = new Element("Selenium", 34);
        elements["Br"] = new Element("Bromine", 35);
        elements["Kr"] = new Element("Krypton", 36);
        elements["Rb"] = new Element("Rubidium", 37);
        elements["Sr"] = new Element("Strontium", 38);
        elements["Y"] = new Element("Yttrium", 39);
        elements["Zr"] = new Element("Zirconium", 40);
        elements["Nb"] = new Element("Niobium", 41);
        elements["Mo"] = new Element("Molybdenum", 42);
        elements["Tc"] = new Element("Technetium", 43);
        elements["Ru"] = new Element("Ruthenium", 44);
        elements["Rh"] = new Element("Rhodium", 45);
        elements["Pd"] = new Element("Palladium", 46);
        elements["Ag"] = new Element("Silver", 47);
        elements["Cd"] = new Element("Cadmium", 48);
        elements["In"] = new Element("Indium", 49);
        elements["Sn"] = new Element("Tin", 50);
        elements["Sb"] = new Element("Antimony", 51);
        elements["Te"] = new Element("Tellurium", 52);
        elements["I"] = new Element("Iodine", 53);
        elements["Xe"] = new Element("Xenon", 54);
        elements["Cs"] = new Element("Caesium", 55);
        elements["Ba"] = new Element("Barium", 56);
        elements["La"] = new Element("Lanthanum", 57);
        elements["Ce"] = new Element("Cerium", 58);
        elements["Pr"] = new Element("Praseodymium", 59);
        elements["Nd"] = new Element("Neodymium", 60);
        elements["Pm"] = new Element("Promethium", 61);
        elements["Sm"] = new Element("Samarium", 62);
        elements["Eu"] = new Element("Europium", 63);
        elements["Gd"] = new Element("Gadolinium", 64);
        elements["Tb"] = new Element("Terbium", 65);
        elements["Dy"] = new Element("Dysprosium", 66);
        elements["Ho"] = new Element("Holmium", 67);
        elements["Er"] = new Element("Erbium", 68);
        elements["Tm"] = new Element("Thulium", 69);
        elements["Yb"] = new Element("Ytterbium", 70);
        elements["Lu"] = new Element("Lutetium", 71);
        elements["Hf"] = new Element("Hafnium", 72);
        elements["Ta"] = new Element("Tantalum", 73);
        elements["W"] = new Element("Tungsten", 74);
        elements["Re"] = new Element("Rhemium", 75);
        elements["Os"] = new Element("Osmium", 76);
        elements["Ir"] = new Element("Iridium", 77);
        elements["Pt"] = new Element("Platinum", 78);
        elements["Au"] = new Element("Gold", 79);
        elements["Hg"] = new Element("Mercury", 80);
        elements["Tl"] = new Element("Thallium", 81);
        elements["Pb"] = new Element("Lead", 82);
        elements["Bi"] = new Element("Bismuth", 83);
        elements["Po"] = new Element("Polonium", 84);
        elements["At"] = new Element("Astatine", 85);
        elements["Rn"] = new Element("Radon", 86);
        elements["Fr"] = new Element("Francium", 87);
        elements["Ra"] = new Element("Radium", 88);
        elements["Ac"] = new Element("Actinium", 89);
        elements["Th"] = new Element("Thorium", 90);
        elements["Pa"] = new Element("Protactinium", 91);
        elements["U"] = new Element("Uranium", 92);
        elements["Np"] = new Element("Neptunium", 93);
        elements["Pu"] = new Element("Plutonium", 94);
        elements["Am"] = new Element("Americium", 95);
        elements["Cm"] = new Element("Curium", 96);
        elements["Bk"] = new Element("Berkelium", 97);
        elements["Cf"] = new Element("Californium", 98);
        elements["Es"] = new Element("Einsteinium", 99);
        elements["Fm"] = new Element("Fermium", 100);
        elements["Md"] = new Element("Mendelevium", 101);
        elements["No"] = new Element("Nobelium", 102);
        elements["Lr"] = new Element("Lawrencium", 103);
        elements["Rf"] = new Element("Rutherfordium", 104);
        elements["Db"] = new Element("Dubnium", 105);
        elements["Sg"] = new Element("Seaborgium", 106);
        elements["Bh"] = new Element("Bohrium", 107);
        elements["Hs"] = new Element("Hasium", 108);
        elements["Mt"] = new Element("Meitnerium", 109);
        elements["Ds"] = new Element("Darmstadtium", 110);
        elements["Rg"] = new Element("Roentgenium", 111);
        elements["Cn"] = new Element("Copernicium", 112);
        elements["Nh"] = new Element("Nihonium", 113);
        elements["Fl"] = new Element("Flerovium", 114);
        elements["Mc"] = new Element("Moscovium", 115);
        elements["Lv"] = new Element("Livermorium", 116);
        elements["Ts"] = new Element("Tennessine", 117);
        elements["Og"] = new Element("Ogenesson", 118);
    }

    string name;
    int atomic_number;

    public string Name
    {
        get { return name; }
    }

    public int AtomicNumber
    {
        get { return atomic_number; }
    }

    //simplification, not sure its matters though
    public int Mass
    {
        get { return atomic_number * 2; }
    }

    public Element(string name_, int atomic_number_)
    {
        name = name_;
        atomic_number = atomic_number_;
    }
}

public abstract class Molecule
{
    static Dictionary<string, Molecule> molecules= new Dictionary<string, Molecule>();

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
                total_mass += element.Mass* Elements[element];

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
                if (molecules[name] == this)
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

    public abstract bool CompareMolecule(Molecule other);
}

public class SimpleMolecule : Molecule
{
    Dictionary<Element, int> elements = new Dictionary<Element, int>();
    int charge;
    float enthalpy;

    public override int Charge { get { return charge; } }

    public override float Enthalpy{ get{ return enthalpy; } }

    public override Dictionary<Element, int> Elements { get { return elements; } }

    public SimpleMolecule(string formula, float enthalpy_, int charge_= 0)
    {
        MatchCollection match_collection= Regex.Matches(formula, "([A-Z][a-z]?)([0-9]*) *");
        foreach(Match match in match_collection)
        {
            string element_key = match.Groups[1].Value;
            int count;
            if (!Int32.TryParse(match.Groups[2].Value, out count))
                count = 1;

            Element element = Element.elements[element_key];

            if (!elements.ContainsKey(element))
                elements[element] = 0;

            elements[element] += count;
        }

        charge = charge_;
        enthalpy = enthalpy_;
    }

    public override bool CompareMolecule(Molecule other)
    {
        if (!(other is SimpleMolecule))
            return false;

        return elements.SequenceEqual((other as SimpleMolecule).elements);
    }
}

public class Polymer : Molecule
{
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

    public class WrapperMonomer : Monomer
    {
        Molecule molecule;

        public override int Charge { get { return molecule.Charge; } }

        public override float Enthalpy
        {
            get { return molecule.Enthalpy - (IsCondensed ? Condensate.Enthalpy : 0); }
        }

        public override Dictionary<Element, int> Elements { get{ return molecule.Elements; } }

        public WrapperMonomer(Molecule molecule_, Molecule condensate) : base(condensate)
        {
            molecule = molecule_;
        }

        public override bool CompareMolecule(Molecule other)
        {
            if (!(other is WrapperMonomer))
                return false;

            return ((WrapperMonomer)other).molecule.CompareMolecule(this.molecule);
        }
    }


    List<Monomer> monomers = new List<Monomer>();

    public List<Monomer> Monomers { get { return monomers; } }

    public override int Charge
    {
        get
        {
            return MathUtility.Sum(monomers, delegate (Monomer monomer) { return monomer.Charge; });
        }
    }

    public override float Enthalpy
    {
        get { return MathUtility.Sum(monomers, delegate (Monomer monomer) { return monomer.Enthalpy; }); }
    }

    public override Dictionary<Element, int> Elements
    {
        get
        {
            Dictionary<Element, int> elements = new Dictionary<Element, int>();

            foreach (Molecule monomer in monomers)
                foreach (Element element in monomer.Elements.Keys)
                    elements[element] += monomer.Elements[element];

            return elements;
        }
    }

    

    public Polymer(List<Monomer> monomers_)
    {
        foreach (Monomer monomer in monomers_)
            AddMonomer(monomer);
    }

    public Polymer()
    {

    }

    public virtual void AddMonomer(Monomer monomer)
    {
        if (monomers.Count > 0)
            monomer.Condense();

        monomers.Add(monomer);
    }

    public virtual Monomer RemoveMonomer(int index)
    {
        Monomer monomer = monomers[index];
        monomers.RemoveAt(index);

        return monomer;
    }

    public int GetLength()
    {
        return monomers.Count;
    }

    public override bool CompareMolecule(Molecule other)
    {
        if (!(other is Polymer))
            return false;

        Polymer other_polymer = (Polymer)other;
        if (other_polymer.monomers.Count != this.monomers.Count)
            return false;

        for (int i = 0; i < monomers.Count; i++)
            if (!monomers[i].CompareMolecule(other_polymer.monomers[i]))
                return false;

        return true;
    }
}

public class Nucleotide : Polymer.Monomer
{
    public static Nucleotide AMP { get; private set; }
    public static Nucleotide CMP { get; private set; }
    public static Nucleotide GMP { get; private set; }
    public static Nucleotide TMP { get; private set; }

    static Nucleotide()
    {
        AMP = new Nucleotide(new SimpleMolecule("C5 H5 N5", 96.9f));
        CMP = new Nucleotide(new SimpleMolecule("C4 H4 N3 O", -235.4f));
        GMP = new Nucleotide(new SimpleMolecule("C5 H4 N5 O", -183.9f));
        TMP = new Nucleotide(new SimpleMolecule("C4 H5 N2 O2", -462.8f));
    }


    Molecule common_structure = new SimpleMolecule("P O7 C5 H10", -1113.8f, -1);
    Molecule nucleobase;

    public override int Charge { get { return common_structure.Charge; } }

    public override float Enthalpy
    {
        get { return common_structure.Enthalpy + nucleobase.Enthalpy - (IsCondensed ? Condensate.Enthalpy : 0); }
    }

    public override Dictionary<Element, int> Elements
    {
        get
        {
            Dictionary<Element, int> elements = new Dictionary<Element, int>(common_structure.Elements);
            foreach (Element element in nucleobase.Elements.Keys)
            {
                if (!elements.ContainsKey(element))
                    elements[element] = 0;

                elements[element] += nucleobase.Elements[element];
            }
            elements[Element.elements["H"]]-= 2;//Just assume this for now

            return elements;
        }
    }

    Nucleotide(Molecule nucleobase_) : base(Water)
    {
        nucleobase = nucleobase_;
    }

    public override bool CompareMolecule(Molecule other)
    {
        if (!(other is Nucleotide))
            return false;

        return ((Nucleotide)other).nucleobase.CompareMolecule(this.nucleobase);
    }
}

public class DNA : Polymer
{
    int active_codon_index = 0;

    public int ActiveCodonIndex
    {
        get { return active_codon_index; }
        set { active_codon_index = value; }
    }

    public string ActiveCodon
    {
        get { return GetCodon(ActiveCodonIndex); }
    }

    public string Sequence
    {
        get
        {
            string sequence = "";

            for (int i = 0; i < GetCodonCount(); i++)
                sequence += GetCodon(i);

            return sequence;
        }
    }

    public DNA(string sequence)
    {
        foreach (char character in sequence)
            switch (character)
            {
                case 'A': AddAdenine(); break;
                case 'C': AddCytosine(); break;
                case 'G': AddGuanine(); break;
                case 'T': AddThymine(); break;
                default: break;
            }
    }

    public DNA()
    {

    }

    public override void AddMonomer(Monomer monomer)
    {
        if (monomer == Nucleotide.AMP ||
            monomer == Nucleotide.CMP ||
            monomer == Nucleotide.GMP ||
            monomer == Nucleotide.TMP)
            base.AddMonomer(monomer);
    }

    public override Monomer RemoveMonomer(int index)
    {
        if (index < ActiveCodonIndex)
            ActiveCodonIndex--;

        return base.RemoveMonomer(index);
    }

    public void AddAdenine()
    {
        AddMonomer(Nucleotide.AMP);
    }

    public void AddCytosine()
    {
        AddMonomer(Nucleotide.CMP);
    }

    public void AddGuanine()
    {
        AddMonomer(Nucleotide.GMP);
    }

    public void AddThymine()
    {
        AddMonomer(Nucleotide.TMP);
    }

    public string GetCodon(int codon_index)
    {
        string codon= "";

        for(int i= 0; i< 3; i++)
        {
            Monomer monomer = Monomers[(codon_index * 3 + i)];

            if (monomer.CompareMolecule(Nucleotide.AMP))
                codon += "A";
            else if (monomer.CompareMolecule(Nucleotide.CMP))
                codon += "C";
            else if (monomer.CompareMolecule(Nucleotide.GMP))
                codon += "G";
            else if (monomer.CompareMolecule(Nucleotide.TMP))
                codon += "T";
        }

        return codon;
    }

    public int GetCodonCount()
    {
        return Monomers.Count / 3;
    }
}


public interface Catalyst
{
    Action Catalyze(Cell.Slot slot);
}

public abstract class Ribozyme : DNA, Catalyst
{
    static Dictionary<string, Ribozyme> ribozymes = new Dictionary<string, Ribozyme>();
    static Dictionary<string, List<Ribozyme>> ribozyme_families = new Dictionary<string, List<Ribozyme>>();

    public static Interpretase Interpretase { get; private set; }
    public static Rotase Rotase { get; private set; }
    public static Constructase Constructase { get; private set; }

    static Ribozyme()
    {
        Interpretase = new Interpretase();
        Rotase = new Rotase();
        Constructase = new Constructase();
    }

    public static void RegisterNamedRibozyme(Ribozyme ribozyme, string name)
    {
        ribozymes[ribozyme.Sequence] = ribozyme;

        if (!ribozyme_families.ContainsKey(name))
            ribozyme_families[name] = new List<Ribozyme>();
        ribozyme_families[name].Add(ribozyme);
    }

    public static Ribozyme GetRibozyme(string dna_sequence)
    {
        return ribozymes[dna_sequence];
    }

    public static List<Ribozyme> GetRibozymeFamily(string name)
    {
        return ribozyme_families[name];
    }

    static string GenerateUniqueDNASequence(int codon_count)
    {
        List<string> starting_codon = new List<string> { "A", "G" };
        List<string> other_codons = new List<string> { "A", "C", "C", "C", "G", "T", "T", "T" };

        string dna_sequence;

        do
        {
            dna_sequence = "";

            for (int i = 0; i < codon_count; i++)
            {
                dna_sequence += MathUtility.RandomElement(starting_codon);
                dna_sequence += MathUtility.RandomElement(other_codons);
                dna_sequence += MathUtility.RandomElement(other_codons);
            }
        }
        while (ribozymes.ContainsKey(dna_sequence));

        return dna_sequence;
    }


    public override string Name
    {
        get
        {
            foreach (string name in ribozyme_families.Keys)
                if (ribozyme_families[name].Contains(this))
                    return name;

            return "Unnamed";
        }
    }

    public Ribozyme(string name, int codon_count) : base(GenerateUniqueDNASequence(codon_count))
    {
        RegisterNamedRibozyme(this, name);
    }

    public Ribozyme(int codon_count)
    {

    }

    public abstract Action Catalyze(Cell.Slot slot);
}

public class Interpretase : Ribozyme
{
    public class Command : Action
    {
        public Command(Cell.Slot slot) : base(slot)
        {
            
        }

        public Interpretase GetInterpretase()
        {
            return Slot.CatalystCompound.Molecule as Interpretase;
        }

        public override void End()
        {
            base.End();

            GetDNA().ActiveCodonIndex++;
        }

        protected DNA GetDNA()
        {
            Debug.Assert(Slot.Compound.Molecule is DNA);

            return Slot.Compound.Molecule as DNA;
        }
    }

    public class OutputCommand : Command
    {
        Cell.Slot output_slot;
        Compound outputted_compound;

        public Cell.Slot OutputSlot
        {
            get { return output_slot; }
        }

        public Compound OutputtedCompound
        {
            get { return outputted_compound; }
            protected set { outputted_compound= value; }
        }

        public OutputCommand(Cell.Slot slot, Cell.Slot output_slot_) : base(slot)
        {
            output_slot = output_slot_;
        }

        public override void End()
        {
            base.End();

            output_slot.AddCompound(outputted_compound);
        }

        protected bool IsMoleculeValidForOutput(Molecule molecule)
        {
            if (output_slot.Compound == null)
                return true;

            return molecule.CompareMolecule(output_slot.Compound.Molecule);
        }
    }

    public class CutCommand : OutputCommand
    {
        string marker;

        public CutCommand(Cell.Slot slot, Cell.Slot output_slot, string marker_) : base(slot, output_slot)
        {
            marker = marker_;
        }

        public override void Beginning()
        {
            base.Beginning();

            if (OutputSlot.Compound != null && !(OutputSlot.Compound.Molecule is DNA))
                Fail();
            else
            {
                DNA dna = GetInterpretase().Cut(GetDNA(), marker);
                if (dna == null)
                    Fail();
                else
                {
                    Polymer polymer = GetRibozyme(dna.Sequence);
                    if (polymer!= null)
                        dna = polymer as DNA;

                    OutputtedCompound = new Compound(dna, 1);
                }
            }
        }
    }

    public class ActivateCommand : OutputCommand
    {
        int activation_count= 1;

        public int ActivationCount
        {
            get { return activation_count; }
        }

        public ActivateCommand(Cell.Slot slot, Cell.Slot output_slot, int activation_count_) : base(slot, output_slot)
        {
            activation_count = activation_count_;
        }

        public override void Beginning()
        {
            base.Beginning();

            if (!IsMoleculeValidForOutput(Molecule.ATP))
                Fail();
            else
                OutputtedCompound = Cell.Organism.Cytozol.RemoveCompound(Molecule.ATP, activation_count);
        }
    }

    public class MoveCommand : OutputCommand
    {
        Cell.Slot input_slot;
        bool move_entire_stack;

        public Cell.Slot InputSlot
        {
            get { return input_slot; }
        }

        public MoveCommand(Cell.Slot slot, Cell.Slot output_slot, Cell.Slot input_slot_, bool move_entire_stack_) : base(slot, output_slot)
        {
            input_slot = input_slot_;
            move_entire_stack = move_entire_stack_;
        }

        public override void Beginning()
        {
            base.Beginning();

            if (!IsMoleculeValidForOutput(input_slot.Compound.Molecule))
                Fail();
            else
                OutputtedCompound = move_entire_stack ? input_slot.RemoveCompound() : input_slot.Compound.Split(1);
        }
    }

    public class GoToCommand : Command
    {
        string marker;
        int seek_count;

        public GoToCommand(Cell.Slot slot, string marker_, int seek_count_) : base(slot)
        {
            marker = marker_;
            seek_count = seek_count_;
        }

        public override void End()
        {
            for(int i= 0; i< seek_count; i++)
                GetInterpretase().SeekToMarker(GetDNA(), marker, seek_count< 0);
        }
    }

    class NullCommand : Command
    {
        public NullCommand(Cell.Slot slot) : base(slot)
        {

        }
    }


    public Interpretase() : base("Interpretase", 6)
    {
        
    }

    public override Action Catalyze(Cell.Slot slot)
    {
        if (slot.Compound == null || !(slot.Compound.Molecule is DNA))
            return null;

        DNA dna = slot.Compound.Molecule as DNA;

        SeekToCommand(dna);
        if (dna.ActiveCodonIndex >= dna.GetCodonCount())
            return null;

        string codon = dna.ActiveCodon;
        string subcodon = codon.Substring(1);

        switch(codon[0])
        {
            case 'A':
                break;

            case 'C':
                switch(subcodon)
                {
                    case "AA":
                    case "CC":
                        object source_location = CodonToLocation(slot, dna.GetCodon(dna.ActiveCodonIndex + 1));
                        if (!(source_location is Cell.Slot))
                            return new NullCommand(slot);

                        object destination_location = CodonToLocation(slot, dna.GetCodon(dna.ActiveCodonIndex + 2));
                        if (!(destination_location is Cell.Slot))
                            return new NullCommand(slot);

                        return new MoveCommand(slot, destination_location as Cell.Slot, source_location as Cell.Slot, subcodon== "AA");

                    case "AC":
                        object activation_location = CodonToLocation(slot, dna.GetCodon(dna.ActiveCodonIndex + 1));
                        if (!(activation_location is Cell.Slot))
                            return new NullCommand(slot);

                        int activation_count = CodonToValue(dna.GetCodon(dna.ActiveCodonIndex + 2));

                        return new ActivateCommand(slot, activation_location as Cell.Slot, activation_count);

                    case "AG":
                        object goto_location = CodonToLocation(slot, dna.GetCodon(dna.ActiveCodonIndex + 1));
                        if (!(goto_location is string))
                            return new NullCommand(slot);

                        int condition_value = 1;
                        if ((dna.ActiveCodonIndex + 2) < dna.GetCodonCount())
                        {
                            string condition_codon = dna.GetCodon(dna.ActiveCodonIndex + 2);
                            if (condition_codon[0] == 'A')
                                condition_value = CodonToValue(condition_codon);
                            else if (condition_codon[0] == 'G')
                                condition_value = ComputeFunction(slot, dna.ActiveCodonIndex + 2);
                        }

                        if (condition_value != 0)
                            return new GoToCommand(slot, goto_location as string, condition_value);
                        else
                            return new NullCommand(slot);

                    case "AT":
                        object cut_location = CodonToLocation(slot, dna.GetCodon(dna.ActiveCodonIndex + 1));
                        if (!(cut_location is string))
                            return new NullCommand(slot);

                        object paste_location= CodonToLocation(slot, dna.GetCodon(dna.ActiveCodonIndex + 2));
                        if (!(paste_location is Cell.Slot))
                            return new NullCommand(slot);

                        return new CutCommand(slot, paste_location as Cell.Slot, cut_location as string);

                    case "CA":
                        break;


                }
                break;

            case 'G':
                break;

            case 'T':
                break;

            default: throw new System.InvalidOperationException();
        }

        return null;
    }

    public void SeekToCommand(DNA dna, bool seek_backwards = false)
    {
        while (dna.ActiveCodonIndex < dna.GetCodonCount() &&
                dna.ActiveCodonIndex >= 0 && 
                dna.ActiveCodon[0] != 'C')

            if (seek_backwards)
                dna.ActiveCodonIndex--;
            else
                dna.ActiveCodonIndex++;

        dna.ActiveCodonIndex= Mathf.Clamp(dna.ActiveCodonIndex, 0, dna.GetCodonCount() - 1);
    }

    public void SeekToMarker(DNA dna, string marker, bool seek_backwards= false)
    {
        int original_codon_index = dna.ActiveCodonIndex;

        while (true)
        {
            do
            {
                if (seek_backwards)
                    dna.ActiveCodonIndex--;
                else
                    dna.ActiveCodonIndex++;

                if (dna.ActiveCodonIndex < 0)
                    dna.ActiveCodonIndex = dna.GetCodonCount() - 1;
                else if (dna.ActiveCodonIndex >= dna.GetCodonCount())
                    dna.ActiveCodonIndex = 0;

                if (dna.ActiveCodonIndex == original_codon_index)
                    return;
            }
            while (dna.ActiveCodon != marker);

            int t_codon_index = dna.ActiveCodonIndex;
            SeekToCommand(dna, true);
            int operand_count = 0;
            if (dna.ActiveCodon[0] == 'C')
                switch (dna.ActiveCodon.Substring(1))
                {
                    case "AA":
                    case "CC":
                    case "AC":
                    case "AT":
                        operand_count = 2;
                        break;

                    case "AG":
                    case "CA":
                        operand_count = 1;
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            else
            {
                dna.ActiveCodonIndex = t_codon_index;
                return;
            }

            if ((dna.ActiveCodonIndex + operand_count) < t_codon_index)
            {
                dna.ActiveCodonIndex = t_codon_index;
                return;
            }
            else if (!seek_backwards)
                dna.ActiveCodonIndex += operand_count;
        }
    }

    DNA Cut(DNA dna, string marker)
    {
        DNA dna_segment= new DNA();

        int current_codon_index = dna.ActiveCodonIndex;
        SeekToMarker(dna, marker);
        int marker_codon_index = dna.ActiveCodonIndex+ 1;
        dna.ActiveCodonIndex = current_codon_index;

        int monomer_index = marker_codon_index * 3;
        while (marker_codon_index < dna.GetCodonCount() && dna.GetCodon(marker_codon_index) != "TTT")
        {
            dna_segment.AddMonomer(dna.RemoveMonomer(monomer_index));
            dna_segment.AddMonomer(dna.RemoveMonomer(monomer_index));
            dna_segment.AddMonomer(dna.RemoveMonomer(monomer_index));
        }

        return dna_segment;
    }

    public static int CodonToValue(string codon)
    {
        int total = 0;
        int exponent = 2;

        foreach(char character in codon)
        {
            int value = 0;

            switch(character)
            {
                case 'A': value = 0; break;
                case 'C': value = 1; break;
                case 'G': value = 2; break;
                case 'T': value = 3; break;
            }

            int base_ = 4;
            int power = 1;
            for (int i = 0; i < exponent; i++)
                power *= base_;
            
            total += power * value;

            exponent--;
        }

        return total;
    }

    object CodonToLocation(Cell.Slot dna_slot, string codon)
    {
        int value = CodonToValue(codon);

        if (value < 48)
            return null;

        if (value < 54)
            return dna_slot.Cell.GetSlot(dna_slot.Index + value - 48);
        else if (value == 54)
            return dna_slot.Cell.Organism;
        else
            return codon;
    }

    public int ComputeFunction(Cell.Slot dna_slot, int function_codon_index, out int next_codon_index)
    {
        DNA dna = dna_slot.Compound.Molecule as DNA;

        string function_codon = dna.GetCodon(function_codon_index);

        switch(function_codon)
        {
            case "GAA":
                next_codon_index = function_codon_index + 2;
                Cell.Slot query_slot = CodonToLocation(dna_slot, dna.GetCodon(function_codon_index + 1)) as Cell.Slot;

                return query_slot.Compound == null ? 0 : (int)query_slot.Compound.Quantity;

            case "GAC":
            case "GAT":
            case "GAG":
                int operand0 = ComputeFunction(dna_slot, function_codon_index + 1, out next_codon_index);
                int operand1 = ComputeFunction(dna_slot, next_codon_index, out next_codon_index);

                switch(function_codon)
                {
                    case "GAC": return operand0 > operand1 ? 1 : 0;
                    case "GAT": return operand0 < operand1 ? 1 : 0;
                    case "GAG": return operand0 == operand1 ? 1 : 0;
                }
                break;

            default:
                next_codon_index = function_codon_index + 1;
                return CodonToValue(dna.GetCodon(function_codon_index));
        }

        next_codon_index = 0;
        return 0;
    }

    int ComputeFunction(Cell.Slot dna_slot, int function_codon_index)
    {
        int next_codon_index;

        return ComputeFunction(dna_slot, function_codon_index, out next_codon_index);
    }
}

public class Rotase : Ribozyme
{
    public Rotase() : base("Rotase", 6)
    {

    }

    //Should Catalysts check if action is possible? Or should Actions? Both?
    public override Action Catalyze(Cell.Slot slot)
    {
        Cell.Slot atp_slot = slot;

        if (atp_slot.Compound == null || atp_slot.Compound.Molecule != Molecule.ATP)
            return null;

        return new PoweredAction(slot, atp_slot, new RotateAction(slot));
    }
}

public class Constructase : Ribozyme
{
    public class ConstructCell : PoweredAction
    {
        public ConstructCell(Cell.Slot slot) 
            : base(slot, slot, 
                   new ReactionAction(slot, 
                                      null, null, 
                                      Utility.CreateList<Compound>(new Compound(Glucose, 7), new Compound(Phosphate, 1)), null))
        {
            
        }

        public override void Beginning()
        {
            base.Beginning();
        }

        public override void End()
        {
            base.End();

            Organism.AddCell(Cell, Slot.Direction);
        }
    }
        

    public Constructase() : base("Constructase", 6)
    {

    }

    public override Action Catalyze(Cell.Slot slot)
    {
        if (slot.Cell.Organism.GetNeighbor(slot.Cell, slot.Direction)!= null)
            return null;

        return new ConstructCell(slot);
    }
}


public class AminoAcid : Polymer.Monomer
{
    static Molecule common_structure = new SimpleMolecule("C3 H7 N O2", -491.6f);

    public static AminoAcid Histidine { get; private set; }
    public static AminoAcid Alanine { get; private set; }
    public static AminoAcid Serine { get; private set; }

    static AminoAcid()
    {
        //These aren't _exactly_ chemically accurate
        Histidine = new AminoAcid(Imidazole);
        Alanine = new AminoAcid(Methane);
        Serine = new AminoAcid(Water);
    }


    Molecule side_chain;

    public override int Charge { get { return side_chain.Charge; } }

    public override float Enthalpy
    {
        get { return common_structure.Enthalpy + side_chain.Enthalpy - (IsCondensed ? Condensate.Enthalpy : 0); }
    }

    public override Dictionary<Element, int> Elements
    {
        get
        {
            Dictionary<Element, int> elements = new Dictionary<Element, int>(common_structure.Elements);
            foreach (Element element in side_chain.Elements.Keys)
            {
                if (!elements.ContainsKey(element))
                    elements[element] = 0;

                elements[element] += side_chain.Elements[element];
            }
            elements[Element.elements["H"]]-= 2;

            return elements;
        }
    }

    public AminoAcid(Molecule side_chain_) : base(Water)
    {
        side_chain = side_chain_;
    }

    public override bool CompareMolecule(Molecule other)
    {
        if (!(other is AminoAcid))
            return false;

        return ((AminoAcid)other).side_chain.CompareMolecule(this.side_chain);
    }
}

public abstract class Enzyme : Polymer, Catalyst
{
    static Dictionary<string, Enzyme> enzymes = new Dictionary<string, Enzyme>();
    static Dictionary<string, List<Enzyme>> enzyme_families= new Dictionary<string, List<Enzyme>>();

    public static void RegisterNamedEnzyme(Enzyme enzyme, string name)
    {
        enzymes[AminoAcidSequenceToString(enzyme.AminoAcidSequence)] = enzyme;

        if (!enzyme_families.ContainsKey(name))
            enzyme_families[name] = new List<Enzyme>();
        enzyme_families[name].Add(enzyme);
    }

    static string AminoAcidSequenceToString(List<AminoAcid> amino_acid_sequence)
    {
        string amino_acid_string = "";

        foreach (AminoAcid amino_acid in amino_acid_sequence)
            amino_acid_string += amino_acid.Name;

        return amino_acid_string;
    }

    static List<AminoAcid> GenerateAminoAcidSequence(int length)
    {
        List<AminoAcid> amino_acid_sequence;

        List<AminoAcid> amino_acids = new List<AminoAcid> { AminoAcid.Histidine, AminoAcid.Alanine, AminoAcid.Serine };

        do
        {
            amino_acid_sequence = new List<AminoAcid>();

            for (int i = 0; i < length; i++)
                amino_acid_sequence.Add(MathUtility.RandomElement(amino_acids));
        }
        while (enzymes.ContainsKey(AminoAcidSequenceToString(amino_acid_sequence)));

        return amino_acid_sequence;
    }


    public override string Name
    {
        get
        {
            foreach (string name in enzyme_families.Keys)
                if (enzyme_families[name].Contains(this))
                    return name;

            return "Unnamed";
        }
    }

    public List<AminoAcid> AminoAcidSequence
    {
        get
        {
            List<AminoAcid> amino_acid_sequence= new List<AminoAcid>();

            foreach (Monomer monomer in Monomers)
                amino_acid_sequence.Add(monomer as AminoAcid);

            return amino_acid_sequence;
        }
    }

    public Enzyme(string name, int length)
    {
        foreach (AminoAcid amino_acid in GenerateAminoAcidSequence(length))
            AddMonomer(amino_acid);

        RegisterNamedEnzyme(this, name);
    }

    public Enzyme()
    {

    }

    public override void AddMonomer(Monomer monomer)
    {
        if (monomer is AminoAcid)
            base.AddMonomer(monomer);
    }

    public abstract Action Catalyze(Cell.Slot slot);
}