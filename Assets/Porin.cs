using Newtonsoft.Json.Linq;

public class Porin : InstantCatalyst
{
    float size;

    public Molecule Molecule { get; private set; }

    public override int Power { get { return 6; } }


    public Porin(Molecule molecule, float size_ = 1)
        : base("Porin", 1, "Equalizes the concentration of a substance with the environment.")
    {
        Molecule = molecule;
        size = size_;
    }

    protected override Action GetAction(Cell.Slot slot)
    {
        if (Molecule == null && slot.Compound == null)
            return null;

        float cytosol_concentration = slot.Cell.Organism.Cytosol.GetConcentration(Molecule);
        float locale_concentration = (slot.Cell.Organism.Locale as WaterLocale).Solution.GetConcentration(Molecule);

        object source, destination;
        if (cytosol_concentration > locale_concentration)
        {
            source = slot.Cell.Organism.Cytosol;
            destination = slot.Cell.Organism.Locale;
        }
        else
        {
            source = slot.Cell.Organism.Locale;
            destination = slot.Cell.Organism.Cytosol;
        }

        return new PumpAction(slot, Molecule, source, destination, size, true);
    }

    public override Catalyst Copy()
    {
        return new Porin(Molecule, size).CopyStateFrom(this);
    }

    public override JObject EncodeJson()
    {
        JObject json_catalyst_object = base.EncodeJson();

        json_catalyst_object["Size"] = size;
        json_catalyst_object["Molecule"] = Molecule.Name;

        return json_catalyst_object;
    }

    public override void DecodeJson(JObject json_object)
    {
        base.DecodeJson(json_object);

        Molecule = Molecule.GetMolecule(Utility.JTokenToString(json_object["Molecule"]));
        size = Utility.JTokenToFloat(json_object["Size"]);
    }
}