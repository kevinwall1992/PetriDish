using System.Collections.Generic;


public class Polymer : Molecule
{
    List<Monomer> monomers = new List<Monomer>();

    public List<Monomer> Monomers { get { return monomers; } }

    public override int Charge
    {
        get
        {
            return MathUtility.Sum(monomers, delegate (Monomer monomer) { return monomer.Charge; });
        }
    }

    public override float Enthalpy
    {
        get { return MathUtility.Sum(monomers, delegate (Monomer monomer) { return monomer.Enthalpy; }); }
    }

    public override Dictionary<Element, int> Elements
    {
        get
        {
            Dictionary<Element, int> elements = new Dictionary<Element, int>();

            foreach (Molecule monomer in monomers)
                foreach (Element element in monomer.Elements.Keys)
                    elements[element] += monomer.Elements[element];

            return elements;
        }
    }



    public Polymer(List<Monomer> monomers_)
    {
        foreach (Monomer monomer in monomers_)
            AddMonomer(monomer);
    }

    public Polymer()
    {

    }

    public virtual void AddMonomer(Monomer monomer)
    {
        if (monomers.Count > 0)
            monomer.Condense();

        monomers.Add(monomer);
    }

    public virtual Monomer RemoveMonomer(int index)
    {
        Monomer monomer = monomers[index];
        monomers.RemoveAt(index);

        return monomer;
    }

    public int GetLength()
    {
        return monomers.Count;
    }

    public override bool CompareMolecule(Molecule other)
    {
        if (!(other is Polymer))
            return false;

        Polymer other_polymer = (Polymer)other;
        if (other_polymer.monomers.Count != this.monomers.Count)
            return false;

        for (int i = 0; i < monomers.Count; i++)
            if (!monomers[i].CompareMolecule(other_polymer.monomers[i]))
                return false;

        return true;
    }


    public abstract class Monomer : Molecule
    {
        Molecule condensate;
        bool is_condensed = false;

        public Molecule Condensate { get { return condensate; } }
        public bool IsCondensed { get { return is_condensed; } }

        public override Dictionary<Element, int> Elements
        {
            get
            {
                if (!is_condensed)
                    return Elements;
                else
                {
                    Dictionary<Element, int> elements = new Dictionary<Element, int>(Elements);

                    foreach (Element element in condensate.Elements.Keys)
                        elements[element] -= condensate.Elements[element];

                    return elements;
                }
            }
        }

        public Monomer(Molecule condensate_)
        {
            condensate = condensate_;
        }

        public void Condense()
        {
            is_condensed = true;
        }
    }

    public class WrapperMonomer : Monomer
    {
        Molecule molecule;

        public override int Charge { get { return molecule.Charge; } }

        public override float Enthalpy
        {
            get { return molecule.Enthalpy - (IsCondensed ? Condensate.Enthalpy : 0); }
        }

        public override Dictionary<Element, int> Elements { get { return molecule.Elements; } }

        public WrapperMonomer(Molecule molecule_, Molecule condensate) : base(condensate)
        {
            molecule = molecule_;
        }

        public override bool CompareMolecule(Molecule other)
        {
            if (!(other is WrapperMonomer))
                return false;

            return ((WrapperMonomer)other).molecule.CompareMolecule(this.molecule);
        }
    }
}
