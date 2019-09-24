using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;

public interface Catalyst : Copiable<Catalyst>, Stackable, Encodable
{
    //Same across all instances
    string Name { get; }
    string Description { get; }
    int Price { get; }

    Example Example { get; }

    int Power { get; }

    Dictionary<Cell.Slot.Relation, Attachment> Attachments { get; }

    //State
    Cell.Slot.Relation Orientation { get; set; }
    IEnumerable<Compound> Cofactors { get; }


    T GetFacet<T>() where T : class, Catalyst;

    Cell.Slot.Relation GetAttachmentDirection(Attachment attachment);

    void RotateLeft();
    void RotateRight();

    bool CanAddCofactor(Compound cofactor);
    void AddCofactor(Compound cofactor);

    void Step(Cell.Slot slot);
    void Communicate(Cell.Slot slot, Action.Stage stage);
    Action Catalyze(Cell.Slot slot, Action.Stage stage);

    Catalyst Mutate();

    //Essentially, .Equals() without state
    //(We ignore orientation and cofactors)
    bool IsSame(Catalyst other);
}


public abstract class Attachment
{
    public Cell.Slot GetSlotPointedAt(Cell.Slot catalyst_slot)
    {
        if (catalyst_slot.Compound == null)
            return null;

        Catalyst catalyst = catalyst_slot.Compound.Molecule as Catalyst;
        if (catalyst == null)
            return null;

        return catalyst_slot.GetAdjacentSlot(catalyst.GetAttachmentDirection(this));
    }
}

public class InputAttachment : Attachment
{
    public Molecule Molecule { get; private set; }

    public InputAttachment(Molecule molecule = null)
    {
        Molecule = molecule;
    }

    public Compound Take(Cell.Slot catalyst_slot, float quantity)
    {
        Cell.Slot slot = GetSlotPointedAt(catalyst_slot);
        if (slot == null)
            return null;

        return slot.Compound.Split(quantity);
    }
}

public class OutputAttachment : Attachment
{
    public Molecule Molecule { get; private set; }

    public OutputAttachment(Molecule molecule = null)
    {
        Molecule = molecule;
    }

    public void Put(Cell.Slot catalyst_slot, Compound compound)
    {
        Cell.Slot slot = GetSlotPointedAt(catalyst_slot);
        if (slot == null || (slot.Compound != null && !slot.Compound.Molecule.IsStackable(compound.Molecule)))
            return;

        slot.AddCompound(compound);
    }
}