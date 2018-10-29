using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Membrane : Interface<Cytozol, Locale>
{
    static float base_diffusion_rate = 1.0f;

    Organism organism;

    Dictionary<Molecule, float> permeability = Utility.CreateDictionary<Molecule, float>(Molecule.ATP, 0.1f, Molecule.ADP, 0.1f);

    public override Cytozol A { get { return organism.Cytozol; } }
    public override Locale B { get { return organism.Locale; } }

    public Cytozol Inside { get { return A; } }
    public Locale Outside { get { return B; } }

    public override float SurfaceArea{ get{ return organism.SurfaceArea; } }

    public Membrane(Organism organism_, Dictionary<Molecule, float> permeability_)
    {
        organism = organism_;

        foreach (Molecule molecule in permeability_.Keys)
            permeability[molecule] = permeability_[molecule];
    }

    float GetPermeability(Molecule molecule)
    {
        if (permeability.ContainsKey(molecule))
            return permeability[molecule];

        return 1;
    }

    void SetOutside()
    {

    }

    protected override float GetQuantityDiffusedByA(Molecule molecule, float time)
    {
        if (Outside is WaterLocale)
            return base_diffusion_rate * 
                   Inside.GetQuantityPerArea(molecule) * 
                   SurfaceArea * 
                   time * 
                   GetPermeability(molecule);

        throw new System.NotImplementedException();
    }

    protected override float GetQuantityDiffusedByB(Molecule molecule, float time)
    {
        if (Outside is WaterLocale)
            return base_diffusion_rate * 
                   Outside.GetQuantityPerArea(molecule) * 
                   SurfaceArea * 
                   time * 
                   (Outside as WaterLocale).Turbulence *
                   GetPermeability(molecule);

        throw new System.NotImplementedException();
    }

    public override void TransportAtoB(Compound compound)
    {
        if(Outside is WaterLocale)
            (Outside as WaterLocale).Solution.AddCompound(Inside.RemoveCompound(compound));
        else
            throw new System.NotImplementedException();
    }

    public override void TransportBtoA(Compound compound)
    {
        if (Outside is WaterLocale)
            Inside.AddCompound((Outside as WaterLocale).Solution.RemoveCompound(compound));
        else
            throw new System.NotImplementedException();
    }
}
