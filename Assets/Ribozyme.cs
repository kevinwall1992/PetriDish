using Newtonsoft.Json.Linq;
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

    public static Ribozyme GetRibozyme(Catalyst catalyst)
    {
        foreach (Ribozyme ribozyme in ribozymes.Values)
            if (ribozyme.Catalyst.IsSame(catalyst))
                return ribozyme;

        return null;
    }

    static string GenerateDNASequence(int codon_count)
    {
        List<string> starting_codon = new List<string> { "V", "F" };
        List<string> other_codons = new List<string> { "V", "C", "C", "C", "F", "L", "L", "L" };

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
        Ribozyme ribozyme = GetRibozyme(catalyst);
        if (ribozyme != null)
            return ribozyme.Sequence;

        return GenerateDNASequence(catalyst.Power);
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

    public Ribozyme(Catalyst catalyst_)
    {
        if (catalyst_ == null)
            return;

        AppendSequence(GetDNASequence(catalyst_));

        Catalyst = catalyst_;

        if (!ribozymes.ContainsKey(Sequence))
            ribozymes[Sequence] = this;
    }

    public void Step(Cell.Slot slot)
    {
        Catalyst.Step(slot);
    }

    public void Communicate(Cell.Slot slot, Action.Stage stage)
    {
        Catalyst.Communicate(slot, stage);
    }

    public Action Catalyze(Cell.Slot slot, Action.Stage stage)
    {
        return Catalyst.Catalyze(slot, stage);
    }

    public T GetFacet<T>() where T : class, Catalyst
    {
        if (typeof(T) == typeof(Ribozyme))
            return this as T;

        return Catalyst.GetFacet<T>();
    }

    public Cell.Slot.Relation GetAttachmentDirection(Attachment attachment)
    {
        return Catalyst.GetAttachmentDirection(attachment);
    }

    public void RotateLeft() { Catalyst.RotateLeft(); }
    public void RotateRight() { Catalyst.RotateLeft(); }

    public bool CanAddCofactor(Compound cofactor) { return Catalyst.CanAddCofactor(cofactor); }
    public void AddCofactor(Compound cofactor) { Catalyst.AddCofactor(cofactor); }

    public virtual Catalyst Mutate()
    {
        Catalyst mutant_catalyst = Catalyst.Mutate();

        if (MathUtility.Roll(0.9f))
            return new Ribozyme(mutant_catalyst);
        else
            return new Protein(mutant_catalyst);
    }

    public bool IsSame(Catalyst other)
    {
        if (!(other is Ribozyme))
            return false;

        Ribozyme other_ribozyme = other as Ribozyme;

        if (!base.IsStackable(other_ribozyme))
            return false;

        return Catalyst.IsSame(other_ribozyme.Catalyst);
    }

    public override bool IsStackable(object obj)
    {
        if (!(obj is Ribozyme))
            return false;

        Ribozyme other = obj as Ribozyme;

        if (!IsSame(other))
            return false;

        return Catalyst.IsStackable(other.Catalyst);
    }

    public override bool Equals(object obj)
    {
        if (!IsStackable(obj))
            return false;

        if (!base.Equals(obj))
            return false;

        return Catalyst.Equals((obj as Ribozyme).Catalyst);
    }


    Catalyst Copiable<Catalyst>.Copy() { return Copy() as Ribozyme; }

    public override Molecule Copy()
    {
        return new Ribozyme(Catalyst.Copy());
    }

    public override JObject EncodeJson()
    {
        JObject json_ribozyme_object = base.EncodeJson();

        json_ribozyme_object["Type"] = "Ribozyme";
        json_ribozyme_object["Catalyst"] = Catalyst.EncodeJson();

        return json_ribozyme_object;
    }

    public override void DecodeJson(JObject json_object)
    {
        base.DecodeJson(json_object);

        Catalyst = ProgressiveCatalyst.DecodeCatalyst(json_object["Catalyst"] as JObject);
        ribozymes[Sequence] = this;
    }
}
