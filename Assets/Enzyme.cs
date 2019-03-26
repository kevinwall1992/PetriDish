using System.Collections.Generic;


public class Enzyme : Polymer, Catalyst
{
    static Dictionary<string, Enzyme> enzymes = new Dictionary<string, Enzyme>();

    static Dictionary<string, AminoAcid> amino_acid_codon_map = new Dictionary<string, AminoAcid>();

    static Enzyme()
    {
        amino_acid_codon_map["AGC"] = AminoAcid.Alanine;
        amino_acid_codon_map["ATC"] = AminoAcid.Histidine;
        amino_acid_codon_map["GCT"] = AminoAcid.Serine;
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

    public static Enzyme GetEnzyme(Catalyst catalyst, int length = -1)
    {
        foreach (Enzyme enzyme in enzymes.Values)
            if (length < 0 || enzyme.AminoAcidSequence.Count == length && enzyme.Catalyst.Equals(catalyst))
                return enzyme;

        return null;
    }

    public static AminoAcid CodonToAminoAcid(string codon)
    {
        if(amino_acid_codon_map.ContainsKey(codon))
            return amino_acid_codon_map[codon];

        return null;
    }

    public static string AminoAcidToCodon(AminoAcid amino_acid)
    {
        foreach (string codon in amino_acid_codon_map.Keys)
            if (amino_acid_codon_map[codon] == amino_acid)
                return codon;

        throw new System.NotImplementedException();
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

    public static string AminoAcidSequenceToDNASequence(List<AminoAcid> amino_acid_sequence)
    {
        string dna_sequence = "";

        foreach (AminoAcid amino_acid in amino_acid_sequence)
            dna_sequence += AminoAcidToCodon(amino_acid);

        return dna_sequence;
    }


    protected Catalyst Catalyst { get; private set; }

    public override string Name { get { return Catalyst.Name; } }

    public string Description { get { return Catalyst.Description; } }
    public int Price { get { return Catalyst.Price; } }
    public Example Example { get { return Catalyst.Example; } }
    public int Power { get { return Catalyst.Power; } }

    public Cell.Slot.Relation Orientation
    {
        get { return Catalyst.Orientation; }
        set { Catalyst.Orientation = value; }
    }

    public IEnumerable<Compound> Cofactors { get { return Catalyst.Cofactors; } }

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

    public string DNASequence { get { return AminoAcidSequenceToDNASequence(AminoAcidSequence); } }

    public Enzyme(Catalyst catalyst_)
    {
        Catalyst = catalyst_;

        int length = Catalyst.Power / 2;
        List<AminoAcid> amino_acid_sequence = GenerateAminoAcidSequence(length);
        Enzyme enzyme = GetEnzyme(Catalyst, length);
        if (enzyme != null)
            amino_acid_sequence = enzyme.AminoAcidSequence;

        foreach (AminoAcid amino_acid in amino_acid_sequence)
            AddMonomer(amino_acid);

        if (!enzymes.ContainsKey(DNASequence))
            enzymes[DNASequence] = this;
    }

    public override void AddMonomer(Monomer monomer)
    {
        if (monomer is AminoAcid)
            base.AddMonomer(monomer);
    }

    public Action Catalyze(Cell.Slot slot, Action.Stage stage)
    {
        return Catalyst.Catalyze(slot, stage);
    }

    public virtual Catalyst Mutate()
    {
        Catalyst mutant_catalyst = Catalyst.Mutate();

        if (MathUtility.Roll(0.9f))
            return new Enzyme(mutant_catalyst);
        else
            return new Ribozyme(mutant_catalyst);
    }

    public T GetFacet<T>() where T : class, Catalyst
    {
        if (typeof(T) == typeof(Enzyme))
            return this as T;

        return Catalyst.GetFacet<T>();
    }

    public void RotateLeft() { Catalyst.RotateLeft(); }
    public void RotateRight() { Catalyst.RotateLeft(); }

    public bool CanAddCofactor(Compound cofactor) { return Catalyst.CanAddCofactor(cofactor); }
    public void AddCofactor(Compound cofactor) { Catalyst.AddCofactor(cofactor); }

    Catalyst Copiable<Catalyst>.Copy() { return Copy() as Ribozyme; }

    public override Molecule Copy()
    {
        return new Enzyme(Catalyst.Copy());
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
