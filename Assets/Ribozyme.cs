using System.Collections.Generic;

public class Ribozyme : DNA, Catalyst
{
    static Dictionary<string, Ribozyme> ribozymes = new Dictionary<string, Ribozyme>();

    public static Ribozyme GetRibozyme(string dna_sequence)
    {
        if(ribozymes.ContainsKey(dna_sequence))
            return ribozymes[dna_sequence];

        return null;
    }

    public static Ribozyme GetRibozyme(Catalyst catalyst, int codon_count = -1)
    {
        foreach (Ribozyme ribozyme in ribozymes.Values)
            if (codon_count < 0 || ribozyme.CodonCount == codon_count && ribozyme.Catalyst.Equals(catalyst))
                return ribozyme;

        return null;
    }

    static string GenerateDNASequence(int codon_count)
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

    static string GetDNASequence(Catalyst catalyst)
    {
        int codon_count = catalyst.Power;

        Ribozyme ribozyme = GetRibozyme(catalyst, codon_count);
        if (ribozyme != null)
            return ribozyme.Sequence;

        return GenerateDNASequence(codon_count);
    }


    protected Catalyst Catalyst { get; private set; }

    public override string Name { get { return Catalyst.Name; } }

    public string Description { get { return Catalyst.Description; } }
    public int Price { get { return Catalyst.Price; } }
    public Example Example { get { return Catalyst.Example; } }
    public int Power { get { return Catalyst.Power; } }
    public Dictionary<Cell.Slot.Relation, Attachment> Attachments { get { return Catalyst.Attachments; } }

    public Cell.Slot.Relation Orientation
    {
        get { return Catalyst.Orientation; }
        set { Catalyst.Orientation = value; }
    }

    public IEnumerable<Compound> Cofactors { get { return Catalyst.Cofactors; } }

    public Ribozyme(Catalyst catalyst_) : base(GetDNASequence(catalyst_))
    {
        Catalyst = catalyst_;

        if (!ribozymes.ContainsKey(Sequence))
            ribozymes[Sequence] = this;
    }

    public void Step(Cell.Slot slot)
    {
        Catalyst.Step(slot);
    }

    public Action Catalyze(Cell.Slot slot, Action.Stage stage)
    {
        return Catalyst.Catalyze(slot, stage);
    }

    public virtual Catalyst Mutate()
    {
        Catalyst mutant_catalyst = Catalyst.Mutate();

        if (MathUtility.Roll(0.9f))
            return new Ribozyme(mutant_catalyst);
        else
            return new Enzyme(mutant_catalyst);
    }

    public T GetFacet<T>() where T : class, Catalyst
    {
        if (typeof(T) == typeof(Ribozyme))
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
        return new Ribozyme(Catalyst.Copy());
    }
}
