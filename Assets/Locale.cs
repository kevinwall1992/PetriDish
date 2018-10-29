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
    static float resting_volume = 100000000000000.0f;

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
        Solution = new Solution(resting_volume);
    }

    public override float GetQuantityPerArea(Molecule molecule)
    {
        return Solution.GetQuantityPerArea(molecule);
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
