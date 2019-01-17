using System.Collections.Generic;


public class Enzyme : Polymer, Catalyst
{
    static Dictionary<string, Enzyme> enzymes = new Dictionary<string, Enzyme>();
    static Dictionary<string, List<Enzyme>> enzyme_families = new Dictionary<string, List<Enzyme>>();

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
            amino_acid_string += amino_acid.Abbreviation;

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

    public static Enzyme GetEnzyme(List<AminoAcid> amino_acid_sequence)
    {
        if (amino_acid_sequence == null)
            return null;

        string amino_acid_sequence_string = AminoAcidSequenceToString(amino_acid_sequence);

        if (enzymes.ContainsKey(amino_acid_sequence_string))
            return enzymes[amino_acid_sequence_string];

        return null;
    }

    public static List<Enzyme> GetEnzymeFamily(string name)
    {
        return enzyme_families[name];
    }

    public static AminoAcid CodonToAminoAcid(string codon)
    {
        switch(codon)
        {
            case "AGC": return AminoAcid.Alanine;
            case "ATC": return AminoAcid.Histidine;
            case "GCT": return AminoAcid.Serine;
        }

        return null;
    }

    public static List<AminoAcid> DNASequenceToAminoAcidSequence(string dna_sequence)
    {
        List<AminoAcid> amino_acid_sequence = new List<AminoAcid>();

        for (int i = 0; i < dna_sequence.Length / 3; i++)
        {
            AminoAcid amino_acid = CodonToAminoAcid(dna_sequence.Substring(i * 3, 3));

            if (amino_acid == null)
                return null;

            amino_acid_sequence.Add(amino_acid);
        }

        return amino_acid_sequence;
    }


    Catalyst catalyst;

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

    public string Description { get { return catalyst.Description; } }
    public int Price { get { return catalyst.Price; } }
    public Example Example { get { return catalyst.Example; } }

    public List<AminoAcid> AminoAcidSequence
    {
        get
        {
            List<AminoAcid> amino_acid_sequence = new List<AminoAcid>();

            foreach (Monomer monomer in Monomers)
                amino_acid_sequence.Add(monomer as AminoAcid);

            return amino_acid_sequence;
        }
    }

    public Enzyme(Catalyst catalyst_, int length)
    {
        catalyst = catalyst_;

        foreach (AminoAcid amino_acid in GenerateAminoAcidSequence(length))
            AddMonomer(amino_acid);

        RegisterNamedEnzyme(this, catalyst.Name);
    }

    public Enzyme()
    {

    }

    public override void AddMonomer(Monomer monomer)
    {
        if (monomer is AminoAcid)
            base.AddMonomer(monomer);
    }

    public Action Catalyze(Cell.Slot slot)
    {
        return catalyst.Catalyze(slot);
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
        Histidine = new AminoAcid(Imidazole, "His");
        Alanine = new AminoAcid(Methane, "Ala");
        Serine = new AminoAcid(Water, "Ser");
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
            elements[Element.elements["H"]] -= 2;

            return elements;
        }
    }

    public string Abbreviation { get; private set; }

    public AminoAcid(Molecule side_chain_, string abbreviation) : base(Water)
    {
        side_chain = side_chain_;

        Abbreviation = abbreviation;
    }
}
