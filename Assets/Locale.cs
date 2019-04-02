using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Locale : Volume, Chronal
{
    List<Organism> organisms = new List<Organism>();

    public List<Organism> Organisms { get { return new List<Organism>(organisms); } }

    public void AddOrganism(Organism organism)
    {
        organisms.Add(organism);

        organism.Locale = this;
    }

    public Organism RemoveOrganism(Organism organism)
    {
        organisms.Remove(organism);

        organism.Locale = null;

        return organism;
    }

    public abstract List<Molecule> Molecules { get; }
    public abstract float GetQuantityPerArea(Molecule molecule);

    public void Step()
    {
        foreach (Organism organism in organisms)
            organism.Step();
    }
}

public class WaterLocale : Locale
{
    //This number selected so that roughly 25 organisms with 
    //an average of 10 cells each could live in a WaterLocale 
    //while only making up 5% of total volume
    //(See Organism.cs Cytosol volume)
    static float resting_water_quantity = Measures.MolesToSmoles(1.7e-10f);

    public Solution Solution
    {
        get;
        private set;
    }

    public float Turbulence
    {
        get
        {
            return 1;
        }
    }

    public override List<Molecule> Molecules{ get{ return Solution.Molecules; } }

    public WaterLocale()
    {
        Solution = new Solution(resting_water_quantity);
    }

    public override float GetQuantityPerArea(Molecule molecule)
    {
        return Solution.GetQuantityPerArea(molecule);
    }

    static WaterLocale CreateCustomWaterLocale(Dictionary<Molecule, float> solute_grams_per_liter)
    {
        WaterLocale water_locale = new WaterLocale();

        foreach (Molecule molecule in solute_grams_per_liter.Keys)
        {
            float grams = solute_grams_per_liter[molecule] * resting_water_quantity / (float)Measures.SmolesOfWaterPerLiter;
            water_locale.Solution.AddCompound(molecule, (float)Measures.GramsToSmoles(molecule, (decimal)grams));
        }

        return water_locale;
    }

    //This is semi-realistic for seawater in general, but I don't have any useful 
    //data for vent seawater so, in particular, the amounts listed
    //for Hydrogen Sulfide, Methane, and Hydrogen gas are completely made up.
    public static WaterLocale CreateVentLocale()
    {
        return CreateCustomWaterLocale(Utility.CreateDictionary<Molecule, float>(
            Molecule.GetMolecule("Chloride"), 18.9f,
            Molecule.GetMolecule("Sodium"), 10.6f,
            Molecule.GetMolecule("Sulfate"), 2.65f,
            Molecule.GetMolecule("Magnesium"), 1.27f,
            Molecule.GetMolecule("Calcium"), 0.4f,
            Molecule.GetMolecule("Potassium"), 0.38f,
            Molecule.GetMolecule("Bicarbonate"), 0.14f,
            Molecule.GetMolecule("Bromide"), 0.065f,
            Molecule.GetMolecule("Boric Acid"), 0.026f,
            Molecule.GetMolecule("Strontium"), 0.013f,
            Molecule.GetMolecule("Floride"), 0.001f,
            Molecule.GetMolecule("Nitrogen"), 0.0084f,
            Molecule.GetMolecule("Carbon Dioxide"), 0.001f,
            Molecule.GetMolecule("Phosphate"), 0.0037f,
            Molecule.GetMolecule("Hydrogen Sulfide"), 0.004f,
            Molecule.GetMolecule("Methane"), 0.002f,
            Molecule.GetMolecule("Hydrogen"), 0.007f));
    }
}

public class WaterWaterInterface : FixedInterface<WaterLocale, WaterLocale>
{
    static float base_diffusion_rate= 5.0f;

    float surface_area;

    public override float SurfaceArea { get{ return surface_area; } }

    public WaterWaterInterface(WaterLocale a, WaterLocale b, float surface_area_) : base(a, b)
    {
        surface_area = surface_area_;
    }

    float GetQuantityDiffused(Molecule molecule, float time, WaterLocale water_locale)
    {
        return base_diffusion_rate * 
               water_locale.GetQuantityPerArea(molecule) * 
               SurfaceArea * 
               time * 
               water_locale.Turbulence;
    }

    protected override float GetQuantityDiffusedByA(Molecule molecule, float time)
    {
        return GetQuantityDiffused(molecule, time, A);
    }

    protected override float GetQuantityDiffusedByB(Molecule molecule, float time)
    {
        return GetQuantityDiffused(molecule, time, B);
    }

    public override void TransportAtoB(Compound compound)
    {
        B.Solution.AddCompound(A.Solution.RemoveCompound(compound));
    }

    public override void TransportBtoA(Compound compound)
    {
        A.Solution.AddCompound(B.Solution.RemoveCompound(compound));
    }
}
