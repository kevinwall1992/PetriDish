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
            elements[Element.elements["H"]] -= 2;

            return elements;
        }
    }

    public AminoAcid(Molecule side_chain_) : base(Water)
    {
        side_chain = side_chain_;
    }
}
