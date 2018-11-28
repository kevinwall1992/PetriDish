using System.Collections.Generic;


public abstract class Ribozyme : DNA, Catalyst
{
    static Dictionary<string, Ribozyme> ribozymes = new Dictionary<string, Ribozyme>();
    static Dictionary<string, List<Ribozyme>> ribozyme_families = new Dictionary<string, List<Ribozyme>>();

    public static Interpretase Interpretase { get; private set; }
    public static Rotase Rotase { get; private set; }
    public static Constructase Constructase { get; private set; }
    public static Pipase Pipase { get; private set; }
    public static Exopumpase Exopumpase { get; private set; }
    public static Endopumpase Endopumpase { get; private set; }
    public static Transcriptase Transcriptase { get; private set; }
    public static Actuase Actuase { get; private set; }
    public static Sporulase Sporulase { get; private set; }

    static Ribozyme()
    {
        Interpretase = new Interpretase();
        Rotase = new Rotase();
        Constructase = new Constructase();
        Pipase = new Pipase();
        Exopumpase = new Exopumpase();
        Endopumpase = new Endopumpase();
        Transcriptase = new Transcriptase();
        Actuase = new Actuase();
        Sporulase = new Sporulase();
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
