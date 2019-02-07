using System.Collections.Generic;


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

            for (int i = 0; i < CodonCount; i++)
                sequence += GetCodon(i);

            return sequence;
        }
    }

    public int CodonCount { get { return Monomers.Count / 3; } }

    public DNA(string sequence)
    {
        AddSequence(sequence);
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
        string codon = "";

        for (int i = 0; i < 3; i++)
        {
            Monomer monomer = Monomers[(codon_index * 3 + i)];

            if (monomer == Nucleotide.AMP)
                codon += "A";
            else if (monomer == Nucleotide.CMP)
                codon += "C";
            else if (monomer == Nucleotide.GMP)
                codon += "G";
            else if (monomer == Nucleotide.TMP)
                codon += "T";
        }

        return codon;
    }

    public string GetSubsequence(int starting_index, int length)
    {
        string subsequence = "";

        for (int i = 0; i < length && (i + starting_index) < CodonCount; i++)
            subsequence += GetCodon(starting_index + i);

        return subsequence;
    }

    public DNA RemoveStrand(int starting_index, int length)
    {
        DNA removed_dna = new DNA();

        for (int i = 0; i < length; i++)
        {
            removed_dna.AddMonomer(RemoveMonomer(starting_index * 3));
            removed_dna.AddMonomer(RemoveMonomer(starting_index * 3));
            removed_dna.AddMonomer(RemoveMonomer(starting_index * 3));
        }

        return removed_dna;
    }

    public void AddSequence(string sequence)
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

    public override Molecule Copy()
    {
        if (this is Ribozyme)
            return this;

        return new DNA(Sequence);
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
            elements[Element.elements["H"]] -= 2;//Just assume this for now

            return elements;
        }
    }

    Nucleotide(Molecule nucleobase_) : base(Water)
    {
        nucleobase = nucleobase_;
    }
}
