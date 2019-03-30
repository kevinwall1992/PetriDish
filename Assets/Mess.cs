using System.Collections.Generic;
using System.Linq;

public class Mess : Molecule, Catalyst
{
    Dictionary<Molecule, Compound> compounds = new Dictionary<Molecule, Compound>();

    public override int Charge { get { return 0; } }

    public override float Enthalpy { get { return 0; } }

    public override Dictionary<Element, int> Elements { get { return new Dictionary<Element, int>(); } }

    public string Description { get { return "A pile of mixed molecules"; } }
    public int Price { get { return 0; } }
    public Example Example { get { return null; } }
    public int Power { get { return 0; } }
    public Dictionary<Cell.Slot.Relation, Attachment> Attachments
    {
        get { return new Dictionary<Cell.Slot.Relation, Attachment>(); }
    }

    public Cell.Slot.Relation Orientation { get { return Cell.Slot.Relation.Across; } set { } }

    public IEnumerable<Compound> Cofactors { get { return compounds.Values; } }

    public Mess(params Compound[] compounds)
    {
        foreach (Compound compound in compounds)
            AddCofactor(compound);
    }

    public void AddCofactor(Compound cofactor)
    {
        if (compounds.ContainsKey(cofactor.Molecule))
            compounds[cofactor.Molecule].Quantity += cofactor.Quantity;
        else
            compounds[cofactor.Molecule] = cofactor.Copy();
    }

    public bool CanAddCofactor(Compound cofactor)
    {
        return true;
    }

    public Action Catalyze(Cell.Slot slot, Action.Stage stage)
    {
        return null;
    }

    public Catalyst Mutate()
    {
        return null;
    }

    public void Reset()
    {
        
    }

    public T GetFacet<T>() where T : class, Catalyst
    {
        return this as T;
    }

    public void RotateLeft()
    {
        
    }

    public void RotateRight()
    {
        
    }

    public void Step(Cell.Slot slot)
    {
        
    }

    Catalyst Copiable<Catalyst>.Copy()
    {
        return new Mess(compounds.Values.ToArray());
    }
}
