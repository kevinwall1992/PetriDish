
using System.Collections.Generic;

public class Trashcan : MutableContainer<Compound>
{
    List<Compound> contents = new List<Compound>();
    List<Compound> buried_contents = new List<Compound>();
    List<Compound> anomalies = new List<Compound>();

    public int Capacity { get; private set; }

    public IEnumerable<Compound> Contents { get { return contents; } }
    public IEnumerable<Compound> BuriedContents { get { return buried_contents; } }

    public override List<Compound> Elements
    {
        get
        {
            return contents;
        }
    }

    public Trashcan()
    {
        Capacity = 10;
    }

    public void ThrowAway(Compound compound)
    {
        contents.Add(compound);
        if(contents.Count > Capacity)
            Bury(contents[0]);

        Touch();
    }

    public Compound FishOut(Compound compound)
    {
        if (!contents.Contains(compound))
            return null;

        if (anomalies.Contains(compound))
            anomalies.Remove(compound);

        contents.Remove(compound);
        Touch();

        return compound;
    }

    public void Bury(Compound compound)
    {
        if (!contents.Contains(compound))
            return;

        contents.Remove(compound);
        Touch();

        buried_contents.Add(compound);

        if (compound.Molecule is Catalyst && MathUtility.Flip())
        {
            Compound anomaly = new Compound((compound.Molecule as Catalyst).Mutate() as Molecule, compound.Quantity);

            anomalies.Add(anomaly);
            ThrowAway(anomaly);
        }
    }

    public override void AddElement(Compound element)
    {
        ThrowAway(element);
    }

    public override Compound RemoveElement(Compound element)
    {
        return FishOut(element);
    }

    public bool IsAnomaly(Compound compound)
    {
        return anomalies.Contains(compound);
    }
}
