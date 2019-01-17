using System.Collections.Generic;


public class Ribozyme : DNA, Catalyst
{
    static Dictionary<string, Ribozyme> ribozymes = new Dictionary<string, Ribozyme>();
    static Dictionary<string, List<Ribozyme>> ribozyme_families = new Dictionary<string, List<Ribozyme>>();

    static Ribozyme()
    {
        new Ribozyme(new Interpretase(), 10);
        new Ribozyme(new Rotase(), 6);
        new Ribozyme(new Constructase(), 6);
        new Ribozyme(new Pipase(), 4);
        new Ribozyme(Pumpase.Endo(Hydrogen), 6);
        new Ribozyme(Pumpase.Endo(CarbonDioxide), 6);
        new Ribozyme(Pumpase.Exo(Methane), 6);
        new Ribozyme(new Transcriptase(), 8);
        new Ribozyme(new Actuase(), 6);
        new Ribozyme(new Sporulase(), 10);
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
        if(ribozymes.ContainsKey(dna_sequence))
            return ribozymes[dna_sequence];

        return null;
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


    Catalyst catalyst;

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

    public string Description { get { return catalyst.Description; } }
    public int Price { get { return catalyst.Price; } }
    public Example Example { get { return catalyst.Example; } }

    public Ribozyme(Catalyst catalyst_, int codon_count) : base(GenerateUniqueDNASequence(codon_count))
    {
        catalyst = catalyst_;

        RegisterNamedRibozyme(this, catalyst.Name);
    }

    public Ribozyme(int codon_count)
    {

    }

    public Action Catalyze(Cell.Slot slot)
    {
        return catalyst.Catalyze(slot);
    }
}
