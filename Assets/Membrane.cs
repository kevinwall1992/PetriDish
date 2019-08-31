using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Membrane : Interface<Cytosol, Locale>
{
    static float base_diffusion_rate = 1.0f;

    Organism organism;

    Dictionary<Molecule, float> permeability = Utility.CreateDictionary<Molecule, float>();

    public override Cytosol A { get { return organism.Cytosol; } }
    public override Locale B { get { return organism.Locale; } }

    public Cytosol Inside { get { return A; } }
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


    //1 unit would be transferred at a concentration of 10ppb and 
    //1:1 concentration across membrane. 2 units at 20ppb, 0.5 units 
    //if destination 2x concentrated, etc. up to 10 units maximum
    public float GetTransportRate(Molecule molecule, bool transport_out)
    {
        Debug.Assert(organism.Locale is WaterLocale);

        Solution source = transport_out ? organism.Cytosol : (organism.Locale as WaterLocale).Solution;
        Solution destination = transport_out ? (organism.Locale as WaterLocale).Solution : organism.Cytosol;

        float source_concentration = source.GetConcentration(molecule);
        float destination_concentration = destination.GetConcentration(molecule);

        return source_concentration * 10000000 *
                Mathf.Min(source_concentration / destination_concentration, 10);
    }
}
