using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public interface Catalyst : Copiable<Catalyst>, Stackable
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

    void RotateLeft();
    void RotateRight();

    bool CanAddCofactor(Compound cofactor);
    void AddCofactor(Compound cofactor);

    void Step(Cell.Slot slot);
    Action Catalyze(Cell.Slot slot, Action.Stage stage);

    Catalyst Mutate();
}

public abstract class Attachment { }

public class InputAttachment : Attachment
{
    public Molecule Molecule { get; private set; }

    public InputAttachment(Molecule molecule)
    {
        Molecule = molecule;
    }
}

public class OutputAttachment : Attachment
{
    public Molecule Molecule { get; private set; }

    public OutputAttachment(Molecule molecule)
    {
        Molecule = molecule;
    }
}

//This Catalyst executes the action in its entirety 
//all at once, and therefore may have to wait several
//turns building up to it. 
public abstract class ProgressiveCatalyst : Catalyst
{
    List<Compound> cofactors = new List<Compound>();
    Dictionary<string, object> aspects = new Dictionary<string, object>();

    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Price { get; private set; }

    public virtual Example Example { get { return null; } }

    public abstract int Power { get; }

    public Dictionary<Cell.Slot.Relation, Attachment> Attachments { get; private set; }

    //Orientation describes the direction the 
    //"front" of the Catalyst is pointing
    public Cell.Slot.Relation Orientation { get; set; }
    public IEnumerable<Compound> Cofactors { get { return cofactors; } }

    public float Progress { get; set; }

    public ProgressiveCatalyst(string name, int price, string description = "")
    {
        Name = name;
        Description = description;
        Price = price;

        Attachments = new Dictionary<Cell.Slot.Relation, Attachment>();

        Orientation = Cell.Slot.Relation.Across;
    }

    protected abstract Action GetAction(Cell.Slot slot);

    public virtual void Step(Cell.Slot slot)
    {
        Progress += slot.Compound.Quantity;
    }

    public virtual Action Catalyze(Cell.Slot slot, Action.Stage stage)
    {
        Action action = GetAction(slot);
        if (action == null)
            return null;

        if (!stage.Includes(action))
            return null;

        if (!action.IsLegal)
        {
            Progress = 0;
            return null;
        }

        if(Progress>= action.Cost)
        {
            Progress = 0;
            return action;
        }

        return null;
    }

    protected static T GetMoleculeInSlotAs<T>(Cell.Slot slot) where T : Molecule
    {
        if (slot.Compound == null)
            return null;

        return slot.Compound.Molecule as T;
    }

    public virtual Catalyst Mutate()
    {
        if (MathUtility.Roll(0.9f))
            return this;
        else
            return MathUtility.RandomElement(Utility.CreateList<Catalyst>(
                new Interpretase(),
                new Constructase(),
                new Transcriptase()));
    }

    public T GetFacet<T>() where T : class, Catalyst
    {
        return this as T;
    }

    public void RotateLeft()
    {
        Orientation = (Cell.Slot.Relation)(((int)Orientation + 2) % 3);
    }

    public void RotateRight()
    {
        Orientation = (Cell.Slot.Relation)(((int)Orientation + 1) % 3);
    }

    public virtual bool CanAddCofactor(Compound cofactor)
    {
        return false;
    }

    public void AddCofactor(Compound cofactor)
    {
        if (!CanAddCofactor(cofactor))
            return;

        foreach (Compound compound in cofactors)
            if (compound.Molecule.Equals(cofactor.Molecule))
            {
                compound.Quantity += cofactor.Quantity;
                return;
            }

        cofactors.Add(cofactor);
    }


    public virtual bool IsStackable(object obj)
    {
        if (this == obj)
            return true;

        if (GetType() != obj.GetType())
            return false;

        Catalyst other = obj as Catalyst;

        foreach (Compound compound in cofactors)
            if (!other.Cofactors.Contains(compound))
                return false;

        foreach (Compound compound in other.Cofactors)
            if (!Cofactors.Contains(compound))
                return false;

        return true;
    }

    public override bool Equals(object obj)
    {
        if (!IsStackable(obj))
            return false;

        Catalyst other = obj as Catalyst;

        if (Orientation != other.Orientation)
            return false;

        if (Progress != (other as ProgressiveCatalyst).Progress)
            return false;

        return true;
    }

    public abstract Catalyst Copy();

    protected virtual ProgressiveCatalyst CopyStateFrom(ProgressiveCatalyst other)
    {
        Orientation = other.Orientation;

        foreach (Compound cofactor in other.cofactors)
            cofactors.Add(cofactor.Copy());

        Progress = other.Progress;

        return this;
    }
}

//This Catalyst always returns an action (if able), 
//scaling the effect of the action to fit the 
//productivity of the Catalyst. 
public abstract class InstantCatalyst : ProgressiveCatalyst
{
    public InstantCatalyst(string name, int price, string description = "") : base(name, price, description)
    {
        
    }

    //Enforce productivity from above?
    public override Action Catalyze(Cell.Slot slot, Action.Stage stage)
    {
        Action action = GetAction(slot);

        if (action != null)
            action.Cost = slot.Compound.Quantity;

        return action;
    }
}


public class Constructase : ProgressiveCatalyst
{
    public override int Power { get { return 7; } }

    public Constructase() : base("Constructase", 1, "Makes new cells")
    {

    }

    protected override Action GetAction(Cell.Slot slot)
    {
        return new ConstructCell(slot);
    }

    public override Catalyst Copy()
    {
        return new Constructase().CopyStateFrom(this);
    }


    public class ConstructCell : EnergeticAction
    {
        public override bool IsLegal
        {
            get
            {
                if (CatalystSlot.AdjacentCell != null)
                    return false;

                return base.IsLegal;
            }
        }

        public ConstructCell(Cell.Slot catalyst_slot)
            : base(catalyst_slot, 2, -2.0f)
        {

        }

        public override void Begin() { }

        public override void End()
        {
            base.Begin();

            if (Cell.GetAdjacentCell(CatalystSlot.Direction) == null)
                Organism.AddCell(Cell, CatalystSlot.Direction);

            //A cell is unexpectedly in the way
            else;
        }
    }
}

public class Separatase : ProgressiveCatalyst
{
    public override int Power { get { return 10; } }

    public Separatase() : base("Separatase", 1, "Separates cells from one another")
    {

    }

    protected override Action GetAction(Cell.Slot slot)
    {
        return new SeparateCell(slot);
    }

    public override Catalyst Copy()
    {
        return new Separatase().CopyStateFrom(this);
    }


    public class SeparateCell : EnergeticAction
    {
        public Compound SeedCompound { get; private set; }

        public override bool IsLegal
        {
            get
            {
                if (CatalystSlot.AdjacentCell == null)
                    return false;

                if (Cytozol.GetQuantity(Molecule.ATP) < (EnergyBalance + 10))
                    return false;

                return base.IsLegal;
            }
        }

        public SeparateCell(Cell.Slot catalyst_slot)
            : base(catalyst_slot, 4, -4)
        {

        }

        public override void Begin()
        {
            if (!IsLegal)
                return;

            base.Begin();

            SeedCompound = Cytozol.RemoveCompound(Molecule.ATP, 10);
        }

        public override void End()
        {
            base.End();

            Organism.Separate(Cell, CatalystSlot.AdjacentCell);

            Organism.Cytozol.AddCompound(SeedCompound);
        }
    }
}

public class Pumpase : InstantCatalyst
{
    bool pump_out;
    Molecule molecule;

    public override int Power { get { return 6; } }

    public Pumpase(bool pump_out_, Molecule molecule_) 
        : base(pump_out_ ? "Exopumpase" : "Endopumpase", 
               1, 
               pump_out_ ? "Removes compounds from cell" : "Draws in compounds from outside")
    {
        pump_out = pump_out_;
        molecule = molecule_;
    }

    Molecule GetMolecule(Cell.Slot slot)
    {
        if (molecule != null)
            return molecule;

        if (slot.Compound == null)
            return null;

        return slot.PreviousSlot.Compound.Molecule;
    }

    protected override Action GetAction(Cell.Slot slot)
    {
        if (molecule == null && slot.Compound == null)
            return null;

        return new PumpAction(slot, pump_out, GetMolecule(slot), 1);
    }

    public static Pumpase Endo(Molecule molecule)
    {
        return new Pumpase(false, molecule);
    }

    public static Pumpase Exo(Molecule molecule)
    {
        return new Pumpase(true, molecule);
    }

    public override Catalyst Mutate()
    {
        if(MathUtility.Roll(0.1f))
            return base.Mutate();
        else
        {
            if (MathUtility.Roll(0.9f))
                return new Pumpase(pump_out, GetRandomMolecule());
            else
                return new Pumpase(!pump_out, molecule);
        }
    }

    public override bool IsStackable(object obj)
    {
        if (!base.IsStackable(obj))
            return false;

        Pumpase other = obj as Pumpase;

        return other.pump_out == pump_out &&
               other.molecule.Equals(molecule);
    }


    static Molecule GetRandomMolecule()
    {
        Dictionary<Molecule, float> weighted_molecules =
            Utility.CreateDictionary<Molecule, float>(Molecule.GetMolecule("Hydrogen"), 10.0f,
                                                      Molecule.GetMolecule("Methane"), 10.0f,
                                                      Molecule.GetMolecule("Carbon Dioxide"), 10.0f,
                                                      Molecule.GetMolecule("Hydrogen Sulfide"), 10.0f,
                                                      Molecule.GetMolecule("Oxygen"), 5.0f,
                                                      Molecule.GetMolecule("Water"), 10.0f,
                                                      Molecule.GetMolecule("Salt"), 10.0f,
                                                      Molecule.GetMolecule("Nitrogen"), 5.0f,
                                                      Molecule.GetMolecule("Glucose"), 10.0f);

        foreach (Molecule molecule in Molecule.Molecules)
            if (!weighted_molecules.ContainsKey(molecule))
                weighted_molecules[molecule] = 1;

        return MathUtility.RandomElement(weighted_molecules);
    }

    public override Catalyst Copy()
    {
        return new Pumpase(pump_out, molecule).CopyStateFrom(this);
    }
}

public class PumpAction : EnergeticAction
{
    bool pump_out;
    Molecule molecule;
    float rate;

    Solution Source
    {
        get
        {
            if (!(Organism.Locale is WaterLocale))
                throw new System.NotImplementedException();

            return pump_out ? Organism.Cytozol : (Organism.Locale as WaterLocale).Solution;
        }
    }

    Solution Destination
    {
        get
        {
            if (!(Organism.Locale is WaterLocale))
                throw new System.NotImplementedException();

            return pump_out ? (Organism.Locale as WaterLocale).Solution : Organism.Cytozol;
        }
    }

    float EffectiveRate
    {
        get
        {
            float source_concentration = Source.GetConcentration(molecule);
            float destination_concentration = Destination.GetConcentration(molecule);

            return rate * 
                source_concentration * 10000000 *
                Mathf.Min(source_concentration / destination_concentration, 10) *
                Scale;
        }
    }

    public override bool IsLegal
    {
        get
        {
            if (!CatalystSlot.IsExposed)
                return false;

            return base.IsLegal;
        }
    }

    public Compound PumpedCompound { get; private set; }

    public PumpAction(Cell.Slot catalyst_slot, 
                      bool pump_out_, Molecule molecule_, float rate_) 
        : base(catalyst_slot, 1, 0.1f)
    {
        pump_out = pump_out_;
        molecule = molecule_;
        rate = rate_;
    }

    public override void Begin()
    {
        base.Begin();

        PumpedCompound = Source.RemoveCompound(new Compound(molecule, EffectiveRate));
    }

    public override void End()
    {
        Destination.AddCompound(PumpedCompound);
    }
}


public class Transcriptase : InstantCatalyst
{
    public override int Power { get { return 9; } }

    public Transcriptase() : base("Transcriptase", 3, "Copies DNA")
    {

    }

    protected override Action GetAction(Cell.Slot slot)
    {
        Cell.Slot dna_slot = slot.PreviousSlot;
        DNA dna = GetMoleculeInSlotAs<DNA>(dna_slot);
        if (dna == null)
            return null;

        int amp_count = 0,
            cmp_count = 0,
            gmp_count = 0,
            tmp_count = 0;
        foreach (Nucleotide nucleotide in dna.Monomers)
        {
            if (nucleotide == Nucleotide.AMP)
                amp_count++;
            else if (nucleotide == Nucleotide.CMP)
                cmp_count++;
            else if (nucleotide == Nucleotide.GMP)
                gmp_count++;
            else if (nucleotide == Nucleotide.TMP)
                tmp_count++;
        }

        Cell.Slot amp_slot = null,
                  cmp_slot = null,
                  gmp_slot = null,
                  tmp_slot = null;

        foreach (Cell.Slot other_slot in slot.Cell.Slots)
            if (other_slot.Compound.Molecule == Nucleotide.AMP)
                amp_slot = other_slot;
            else if (other_slot.Compound.Molecule == Nucleotide.CMP)
                cmp_slot = other_slot;
            else if (other_slot.Compound.Molecule == Nucleotide.GMP)
                gmp_slot = other_slot;
            else if (other_slot.Compound.Molecule == Nucleotide.TMP)
                tmp_slot = other_slot;

        foreach (Cell.Slot other_slot in Utility.CreateList(amp_slot, cmp_slot, gmp_slot, tmp_slot))
            if (other_slot == null)
                return null;

        float cost= (amp_count + cmp_count + gmp_count + tmp_count) / (1.0f / 4.0f);

        return new ReactionAction(
                slot,
                Utility.CreateDictionary<Cell.Slot, Compound>(
                    amp_slot, new Compound(Nucleotide.AMP, amp_count),
                    cmp_slot, new Compound(Nucleotide.CMP, cmp_count),
                    gmp_slot, new Compound(Nucleotide.GMP, gmp_count),
                    tmp_slot, new Compound(Nucleotide.TMP, tmp_count)),
                Utility.CreateDictionary<Cell.Slot, Compound>(
                    slot.AcrossSlot, new Compound(dna_slot.Compound.Molecule, 1)),
                null,
                Utility.CreateList(new Compound(Molecule.Water, dna.Monomers.Count - 1)), 
                cost / 4, 
                cost);
    }

    public override Catalyst Copy()
    {
        return new Transcriptase().CopyStateFrom(this);
    }
}
