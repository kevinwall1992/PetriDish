using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;

public class Element
{
    public static Dictionary<string, Element> elements= new Dictionary<string, Element>();

    static Element()
    {
        elements["H"] = new Element("Hydrogen", 1);
        elements["C"] = new Element("Carbon", 6);
        elements["N"] = new Element("Nitrogen", 7);
        elements["O"] = new Element("Oxygen", 8);
        elements["Na"] = new Element("Sodium", 11);
        elements["Si"] = new Element("Silicon", 14);
        elements["P"] = new Element("Phosphorus", 15);
        elements["S"] = new Element("Sulfur", 16);
        elements["Cl"] = new Element("Chlorine", 17);
        elements["Fe"] = new Element("Iron", 26);
    }

    string name;
    int atomic_number;

    public Element(string name_, int atomic_number_)
    {
        name = name_;
        atomic_number = atomic_number_;
    }
}

public abstract class Molecule
{
    static Dictionary<string, Molecule> molecules= new Dictionary<string, Molecule>();
    public const int mini_mole = 1000000000;

    static Molecule()
    {
        RegisterNamedMolecule("Salt", new SimpleMolecule("Na Cl"));
        RegisterNamedMolecule("Sand", new SimpleMolecule("Si O2"));
        RegisterNamedMolecule("Water", new SimpleMolecule("H2 0"));
        RegisterNamedMolecule("Carbon Dioxide", new SimpleMolecule("C O2"));
        RegisterNamedMolecule("Iron", new SimpleMolecule("Fe"));
        RegisterNamedMolecule("Oxygen", new SimpleMolecule("O2"));
        RegisterNamedMolecule("Hydrogen Sulfide", new SimpleMolecule("H2 S2"));
        RegisterNamedMolecule("Imidazole", new SimpleMolecule("C2 N3 H4"));
        RegisterNamedMolecule("Methane", new SimpleMolecule("C2 N3 H4"));
        RegisterNamedMolecule("Benzene", new SimpleMolecule("C2 N3 H4"));
        RegisterNamedMolecule("ATP", new SimpleMolecule("C10 H16 N5 O13 P3"));
        RegisterNamedMolecule("ADP", new SimpleMolecule("C10 H16 N5 O10 P2"));
        RegisterNamedMolecule("Phosphate", new SimpleMolecule("S O4"));
        RegisterNamedMolecule("Glycerol", new SimpleMolecule("C3 H8 O3"));
        RegisterNamedMolecule("Palmitic Acid", new SimpleMolecule("C16 H32 O2"));
        RegisterNamedMolecule("Oleic Acid", new SimpleMolecule("C18 H34 O2"));
        RegisterNamedMolecule("Choline", new SimpleMolecule("C5 H14 N O"));
        RegisterNamedMolecule("Phospholipid", new SimpleMolecule("C42 H82 N O8 P"));
    }

    protected static T RegisterNamedMolecule<T>(string name, T molecule) where T : Molecule
    {
        molecules[name] = molecule;

        return molecule;
    }

    public static Molecule GetMolecule(string name)
    {
        return molecules[name];
    }


    public Molecule()
    {

    }

    public string GetName()
    {
        foreach (string name in molecules.Keys)
            if (molecules[name] == this)
                return name;

        return "not found";
    }

    public abstract List<Element> GetElements();
    public abstract bool CompareMolecule(Molecule other);
}

public class SimpleMolecule : Molecule
{
    List<Element> elements = new List<Element>();

    public SimpleMolecule(string formula)
    {
        MatchCollection match_collection= Regex.Matches(formula, "([A-Z][a-z]?)([0-9]*) *");
        foreach(Match match in match_collection)
        {
            string element_key = match.Groups[1].Value;
            int count;
            if (!Int32.TryParse(match.Groups[2].Value, out count))
                count = 1;

            for(int i= 0; i< count; i++)
                elements.Add(Element.elements[element_key]);
        }
    }

    public override List<Element> GetElements()
    {
        return elements;
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
        bool condensed = false;

        public Monomer(Molecule condensate_)
        {
            condensate = condensate_;
        }

        public void Condense()
        {
            condensed = true;
        }

        public override List<Element> GetElements()
        {
            if (!condensed)
                return GetElements();
            else
            {
                List<Element> elements = GetElements();

                foreach (Element element in condensate.GetElements())
                    for (int i = 0; i < elements.Count; i++)
                        if (elements[i] == element)
                        {
                            elements.RemoveAt(i);
                            i--;
                        }

                return elements;
            }
        }
    }

    public class WrapperMonomer : Monomer
    {
        Molecule molecule;

        public WrapperMonomer(Molecule molecule_, Molecule condensate) : base(condensate)
        {
            molecule = molecule_;
        }

        public override List<Element> GetElements()
        {
            return molecule.GetElements();
        }

        public override bool CompareMolecule(Molecule other)
        {
            if (!(other is WrapperMonomer))
                return false;

            return ((WrapperMonomer)other).molecule.CompareMolecule(this.molecule);
        }
    }


    static Dictionary<string, Polymer> polymers= new Dictionary<string, Polymer>();

    public static void RegisterNamedPolymer(string name, Polymer polymer)
    {
        polymers[name] = polymer;
        RegisterNamedMolecule(name, polymer);
    }

    public static Polymer GetPolymer(List<Monomer> monomers)
    {
        foreach (string name in polymers.Keys)
            if (polymers[name].monomers.SequenceEqual(monomers))
                return polymers[name];

        return null;
    }

    public static Polymer GetPolymer(string name)
    {
        return polymers[name];
    }

    static Polymer()
    {
        new Interpretase();
        new Rotase();
        new Constructase();
    }


    List<Monomer> monomers = new List<Monomer>();

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

    public Monomer GetMonomer(int index)
    {
        return monomers[index];
    }

    public List<Monomer> GetMonomers()
    {
        return monomers;
    }

    public override List<Element> GetElements()
    {
        List<Element> elements = new List<Element>();

        foreach (Molecule monomer in monomers)
            elements.AddRange(monomer.GetElements());

        return elements;
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
    //These names refer here to the nucleotides containing adenine, cytosine, etc
    public static Nucleotide adenine, cytosine, guanine, thymine;

    static Nucleotide()
    {
        SimpleMolecule pyrophosphate = RegisterNamedMolecule("Pyrophosphate", new SimpleMolecule("P2 O7"));

        adenine = RegisterNamedMolecule("Adenine", new Nucleotide(new SimpleMolecule("N5 C5 H4")));
        cytosine = RegisterNamedMolecule("Adenine", new Nucleotide(new SimpleMolecule("N3 C4 H4 O")));
        guanine = RegisterNamedMolecule("Adenine", new Nucleotide(new SimpleMolecule("N5 C5 H4 O")));
        thymine = RegisterNamedMolecule("Adenine", new Nucleotide(new SimpleMolecule("N2 C4 H5 O2")));
    }


    Molecule common_structure = new SimpleMolecule("P O4 H C C5 O2 H9");
    Molecule nucleobase;

    Nucleotide(Molecule nucleobase_) : base(GetMolecule("Pyrophosphate"))
    {
        nucleobase = nucleobase_;
    }

    public override List<Element> GetElements()
    {
        List<Element> elements = common_structure.GetElements();
        elements.AddRange(nucleobase.GetElements());
        elements.Remove(Element.elements["H"]);//Just assume this for now

        return elements;
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
    static DNA()
    {

    }


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
        if (monomer == Nucleotide.adenine ||
            monomer == Nucleotide.cytosine ||
            monomer == Nucleotide.guanine ||
            monomer == Nucleotide.thymine)
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
        AddMonomer(Nucleotide.adenine);
    }

    public void AddCytosine()
    {
        AddMonomer(Nucleotide.cytosine);
    }

    public void AddGuanine()
    {
        AddMonomer(Nucleotide.guanine);
    }

    public void AddThymine()
    {
        AddMonomer(Nucleotide.thymine);
    }

    public string GetCodon(int codon_index)
    {
        string codon= "";

        for(int i= 0; i< 3; i++)
        {
            Monomer monomer = GetMonomer(codon_index * 3 + i);

            if (monomer.CompareMolecule(Nucleotide.adenine))
                codon += "A";
            else if (monomer.CompareMolecule(Nucleotide.cytosine))
                codon += "C";
            else if (monomer.CompareMolecule(Nucleotide.guanine))
                codon += "G";
            else if (monomer.CompareMolecule(Nucleotide.thymine))
                codon += "T";
        }

        return codon;
    }

    public string GetSequence()
    {
        string sequence = "";

        for (int i = 0; i < GetCodonCount(); i++)
            sequence += GetCodon(i) + " ";

        sequence= sequence.TrimEnd(' ');

        return sequence;
    }

    public int GetCodonCount()
    {
        return GetMonomers().Count / 3;
    }
}

public interface Catalyst
{
    Action Catalyze(Cell.Slot slot);
}

public class AminoAcid : Polymer.Monomer
{
    static Molecule common_structure = new SimpleMolecule("C2 O2 N H4");

    public static AminoAcid histadine;
    public static AminoAcid alanine;
    public static AminoAcid serine;
    public static AminoAcid phenylalanine;

    static AminoAcid()
    {
        //These aren't _exactly_ chemically accurate
        RegisterNamedMolecule("Histadine", new AminoAcid(GetMolecule("Imidazole")));
        RegisterNamedMolecule("Alanine", new AminoAcid(GetMolecule("Methane")));
        RegisterNamedMolecule("Serine", new AminoAcid(GetMolecule("Water")));
        RegisterNamedMolecule("Phenylalanine", new AminoAcid(GetMolecule("Benzene")));
    }


    Molecule side_chain;

    public AminoAcid(Molecule side_chain_) : base(GetMolecule("Water"))
    {
        side_chain = side_chain_;
    }

    public override List<Element> GetElements()
    {
        List<Element> elements = common_structure.GetElements();
        elements.AddRange(side_chain.GetElements());
        elements.Remove(Element.elements["H"]);//Just assume this for now

        return elements;
    }

    public override bool CompareMolecule(Molecule other)
    {
        if (!(other is AminoAcid))
            return false;

        return ((AminoAcid)other).side_chain.CompareMolecule(this.side_chain);
    }
}


public abstract class Ribozyme : DNA, Catalyst
{
    public Ribozyme(string name, string sequence) : base(sequence)
    {
        RegisterNamedPolymer(name, this);
    }

    public Ribozyme()
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
                    Polymer polymer = Polymer.GetPolymer(dna.GetMonomers());
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

            if (!IsMoleculeValidForOutput(Molecule.GetMolecule("ATP")))
                Fail();
            else
                OutputtedCompound = Cell.Organism.Cytozol.RemoveCompound(Molecule.GetMolecule("ATP"), activation_count);
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


    public Interpretase() : base("Interpretase", "AGG GCT AAG GTG")
    {
        
    }

    public override Action Catalyze(Cell.Slot slot)
    {
        if (!(slot.Compound.Molecule is DNA))
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
            return dna_slot.Cell.GetSlot((dna_slot.Index + value - 48) % 6);
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

                return query_slot.Compound == null ? 0 : query_slot.Compound.Quantity;

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
    public Rotase() : base("Rotase", "ACC GGA ATC GGC")
    {

    }

    //Should Catalysts check if action is possible? Or should Actions? Both?
    public override Action Catalyze(Cell.Slot slot)
    {
        Cell.Slot atp_slot = slot;

        if (atp_slot.Compound == null || atp_slot.Compound.Molecule != Molecule.GetMolecule("ATP"))
            return null;

        return new PoweredAction(slot, atp_slot, new RotateAction(slot));
    }
}

public class Constructase : Ribozyme
{
    public class ConstructCell : Reaction
    {
        public ConstructCell(Cell.Slot slot) : base(
            slot, 
            Utility.CreateDictionary<Cell.Slot, Molecule>(slot, GetMolecule("Phospholipid")), 
            null, 
            Utility.CreateList<Molecule>(Molecule.GetMolecule("ATP")), 
            Utility.CreateList<Molecule>(Molecule.GetMolecule("ADP"), Molecule.GetMolecule("Phosphate")))
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
        

    public Constructase() : base("Constructase", "ACT GTA ATC GGT")
    {

    }

    public override Action Catalyze(Cell.Slot slot)
    {
        if (slot.Cell.Organism.GetNeighbor(slot.Cell, slot.Direction)!= null)
            return null;

        return new ConstructCell(slot);
    }
}

public abstract class Enzyme : Polymer, Catalyst
{
    public static Dictionary<string, Enzyme> enzymes;

    public Enzyme(string name, params AminoAcid[] amino_acids)
    {
        foreach (AminoAcid amino_acid in amino_acids)
            AddMonomer(amino_acid);

        RegisterNamedPolymer(name, this);
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