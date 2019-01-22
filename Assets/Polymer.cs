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
                {
                    if (!elements.ContainsKey(element))
                        elements[element] = 0;

                    elements[element] += monomer.Elements[element];
                }

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

    public override bool Equals(object obj)
    {
        if (obj == this)
            return true;

        if (!(obj is Polymer))
            return false;

        Polymer other = obj as Polymer;

        if (Monomers.Count != other.Monomers.Count)
            return false;

        for (int i = 0; i < Monomers.Count; i++)
            if (Monomers[i] != other.Monomers[i])
                return false;

        return true;
    }

    public override int GetHashCode()
    {
        int hash = 17;

        foreach (Monomer monomer in Monomers)
            hash = hash * 23 + monomer.GetHashCode();

        return hash;
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
    }
}
