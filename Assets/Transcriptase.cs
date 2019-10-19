
public class Transcriptase : InstantCatalyst
{
    public override int Power { get { return 9; } }

    public InputAttachment Feed { get; private set; }
    public Transcriptor Transcriptor { get; private set; }

    public Transcriptase() : base("Transcriptase", 3, "Copies DNA")
    {
        Attachments[Cell.Slot.Relation.Left] = Feed = new InputAttachment(Molecule.GetMolecule("Genes"));
        Attachments[Cell.Slot.Relation.Right] = Transcriptor = new Transcriptor();
    }

    protected override Action GetAction(Cell.Slot slot)
    {
        DNA dna = Transcriptor.GetDNA(slot);
        float length_units = dna.Monomers.Count / Balance.Actions.Transcription.UnitLength;

        return new ReactionAction(
            slot,
            Utility.CreateDictionary<Cell.Slot, Compound>(Feed.GetSlotPointedAt(slot), new Compound(Feed.Molecule, dna.Monomers.Count)),
            Utility.CreateDictionary<Cell.Slot, Compound>(Transcriptor.GetSlotPointedAt(slot), new Compound(dna, 1)),
            null,
            Utility.CreateList(new Compound(Molecule.Water, dna.Monomers.Count - 1)),
            null, null,
            Balance.Actions.Transcription.Cost * length_units,
            Balance.Actions.Transcription.EnergyChange * length_units);
    }

    public override Catalyst Copy()
    {
        return new Transcriptase().CopyStateFrom(this);
    }
}


public class Transcriptor : Attachment
{
    public Transcriptor()
    {

    }

    public DNA GetDNA(Cell.Slot catalyst_slot)
    {
        Compound compound = GetSlotPointedAt(catalyst_slot).Compound;
        if (compound == null)
            return null;

        return compound.Molecule as DNA;
    }
}