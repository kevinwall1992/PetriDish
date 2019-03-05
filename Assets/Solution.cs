using UnityEngine;
using System.Collections.Generic;

//Need to add support for arbitrarily large amounts of water/ solvent
public class Solution : MutableContainer<Compound>, Volume
{
    PrecisionSolution precision_solution;

    public float Mass { get { return (float)precision_solution.Mass; } }
    public float Temperature { get { return (float)precision_solution.Temperature; } }
    public float pH { get { return (float)precision_solution.pH; } }
    public float pOH { get { return (float)precision_solution.pOH; } }
    public float Liters { get { return (float)precision_solution.Liters; } }

    public List<Molecule> Molecules { get { return precision_solution.Molecules; } }

    public Solution(float water_quantity)
    {
        precision_solution = new PrecisionSolution((decimal)water_quantity);
    }

    public float GetQuantity(Molecule molecule)
    {
        return (float)precision_solution.GetQuantity(molecule);
    }

    public void AddCompound(Compound compound)
    {
        AddCompound(compound.Molecule, compound.Quantity);
    }

    public void AddCompound(Molecule molecule, float quantity)
    {
        precision_solution.AddCompound(molecule, (decimal)quantity);
        Modify();
    }

    public Compound RemoveCompound(Compound compound)
    {
        return RemoveCompound(compound.Molecule, compound.Quantity);
    }

    public Compound RemoveCompound(Molecule molecule, float quantity = -1)
    {
        Compound compound = new Compound(molecule, (float)precision_solution.RemoveCompound(molecule, (decimal)quantity).quantity);
        Modify();

        return compound;
    }

    public float GetConcentration(Molecule molecule)
    {
        return (float)precision_solution.GetConcentration(molecule);
    }

    public float GetQuantityPerArea(Molecule molecule)
    {
        return precision_solution.GetQuantityPerArea(molecule);
    }


    class PrecisionSolution : Volume
    {
        public class PrecisionCompound
        {
            public Molecule molecule;
            public decimal quantity;

            public PrecisionCompound(Molecule molecule_, decimal quantity_)
            {
                molecule = molecule_;
                quantity = quantity_;
            }

            public PrecisionCompound Split(decimal removed_quantity)
            {
                removed_quantity = System.Math.Min(removed_quantity, quantity);

                quantity -= removed_quantity;

                return new PrecisionCompound(molecule, removed_quantity);
            }
        }

        class Buffer
        {
            Molecule molecule;
            Molecule conjugate;

            public Molecule Molecule { get { return molecule; } }
            public Molecule Conjugate { get { return conjugate; } }

            public bool IsAcid
            {
                get { return (conjugate.Charge - molecule.Charge) < 0; }
            }

            public Buffer(Molecule molecule_, Molecule conjugate_)
            {
                molecule = molecule_;
                conjugate = conjugate_;

                Debug.Assert((int)(System.Math.Abs(molecule.Charge - conjugate.Charge) + 0.5f) == 1);
            }
        }

        static List<Buffer> buffers = Utility.CreateList(new Buffer(Molecule.Water, Molecule.Hydronium),
                                                         new Buffer(Molecule.Water, Molecule.Hydroxide),
                                                         new Buffer(Molecule.CarbonicAcid, Molecule.Bicarbonate));

        Dictionary<Molecule, PrecisionCompound> compounds = new Dictionary<Molecule, PrecisionCompound>();

        decimal heat = 0;

        public List<Molecule> Molecules { get { return new List<Molecule>(compounds.Keys); } }

        public decimal Mass
        {
            get
            {
                decimal mass = 0;

                foreach (Molecule molecule in compounds.Keys)
                    mass += GetQuantity(molecule) * molecule.Mass;

                return mass;
            }
        }

        public decimal Temperature
        {
            get { return heat / Mass; }
        }

        public decimal pH
        {
            get
            {
                return GetQuantity(Molecule.Hydroxide) == 0 ?
                         -(decimal)System.Math.Log10((double)GetConcentration(Molecule.Hydronium) + System.Math.Pow(10, -7)) :
                         14 - pOH;
            }
        }

        public decimal pOH
        {
            get
            {
                return GetQuantity(Molecule.Hydronium) == 0 ?
                         -(decimal)System.Math.Log10((double)GetConcentration(Molecule.Hydroxide) + System.Math.Pow(10, -7)) :
                         14 - pH;
            }
        }

        public decimal WaterQuantity
        {
            get { return GetQuantity(Molecule.Water); }
        }

        public decimal Liters
        {
            get { return WaterQuantity / Measures.SmolesOfWaterPerLiter; }
        }

        public PrecisionSolution(decimal water_quantity)
        {
            AddCompound(Molecule.Water, water_quantity);
        }

        PrecisionCompound GetCompound(Molecule molecule)
        {
            if (!compounds.ContainsKey(molecule))
                compounds[molecule] = new PrecisionCompound(molecule, 0);

            return compounds[molecule];
        }

        public decimal GetQuantity(Molecule molecule)
        {
            return GetCompound(molecule).quantity;
        }

        public void AddCompound(PrecisionCompound compound)
        {
            if (compound.quantity == 0)
                return;

            bool is_proton = compound.molecule == Molecule.Proton;
            bool is_proton_or_hydronium = is_proton || compound.molecule == Molecule.Hydronium;
            bool is_primitive_ion = is_proton_or_hydronium || compound.molecule == Molecule.Hydroxide;

            if (is_primitive_ion)
                foreach (Buffer buffer in buffers)
                {
                    bool consume_conjugate = is_proton_or_hydronium ? buffer.IsAcid : !buffer.IsAcid;

                    Molecule consumed = consume_conjugate ? buffer.Conjugate : buffer.Molecule;
                    Molecule produced = consume_conjugate ? buffer.Molecule : buffer.Conjugate;

                    if (produced == compound.molecule)
                        continue;

                    decimal quantity = compound.Split(GetQuantity(consumed)).quantity;

                    RemoveCompound(consumed, quantity);
                    if (!is_proton)
                        AddCompound(Molecule.Water, quantity);
                    AddCompound(produced, quantity);

                    if (compound.quantity == 0)
                        break;
                }

            GetCompound(compound.molecule).quantity += compound.quantity;

            //Just assume temperature of compounds added for now.
            heat += 298 * compound.molecule.Mass * compound.quantity;

            if (!is_primitive_ion)
            {
                AddCompound(RemoveCompound(Molecule.Proton));
                AddCompound(RemoveCompound(Molecule.Hydronium));
                AddCompound(RemoveCompound(Molecule.Hydroxide));
            }
        }

        public void AddCompound(Molecule molecule, decimal quantity)
        {
            AddCompound(new PrecisionCompound(molecule, quantity));
        }

        public PrecisionCompound RemoveCompound(PrecisionCompound compound)
        {
            PrecisionCompound removed_compound = GetCompound(compound.molecule).Split(compound.quantity);
            heat -= removed_compound.molecule.Mass * removed_compound.quantity;

            return removed_compound;
        }

        public PrecisionCompound RemoveCompound(Molecule molecule, decimal quantity = -1)
        {
            return RemoveCompound(new PrecisionCompound(molecule, quantity < 0 ? GetQuantity(molecule) : quantity));
        }

        public decimal GetConcentration(Molecule molecule)
        {
            return Measures.SmolesToMoles(GetQuantity(molecule)) / Liters;
        }


        //This attempts to approximate the number of molecules
        //in contact with the surface of the solution by assuming # of atoms
        //approximates the relative volume of a molecule (obviously a poor approximation).
        //The magic number is the number of moles of water that would fit on surface of 
        //a 3.3e-11 mole drop of water, which is the volume of our model cell.
        //The 3 is because water has three atoms. 
        //Disclaimer : My calculations that resulted in the magic number may not be correct ;)
        public float GetQuantityPerArea(Molecule molecule)
        {
            return (float)((Measures.MolesToSmoles(8.3e-17m) *
                   molecule.AtomCount * GetQuantity(molecule) /
                   MathUtility.Sum(Molecules, (molecule_) => (molecule_.AtomCount * GetQuantity(molecule_)))));
        }
    }

    public override IEnumerable<Compound> Items
    {
        get
        {
            List<Compound> compounds = new List<Compound>();

            foreach (Molecule molecule in Molecules)
                compounds.Add(new Compound(molecule, GetQuantity(molecule)));

            return compounds;
        }
    }

    public override void AddItem(Compound compound)
    {
        AddCompound(compound);
    }

    public override Compound RemoveItem(Compound compound)
    {
        return RemoveCompound(compound);
    }
}
