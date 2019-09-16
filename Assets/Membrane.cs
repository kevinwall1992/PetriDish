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

    public Membrane(Organism organism_)
    {
        organism = organism_;

        permeability[Molecule.Water] = 1;
    }

    float GetPermeability(Molecule molecule)
    {
        if (permeability.ContainsKey(molecule))
            return permeability[molecule];

        return 0;
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

        float locale_quantity = (organism.Locale as WaterLocale).Solution.GetQuantity(molecule);
        float organism_quantity = organism.Cytosol.GetQuantity(molecule);
        foreach (Cell cell in organism.GetCells())
            foreach (Cell.Slot slot in cell.Slots)
                if (slot.Compound != null && slot.Compound.Molecule.Equals(molecule))
                    organism_quantity += slot.Compound.Quantity;

        float locale_concentration = (organism.Locale as WaterLocale).Solution.GetConcentration(molecule);
        float organism_concentration = Measures.SmolesToMoles(organism_quantity) / organism.Cytosol.Liters;
        float concentration_gradient = transport_out ? (organism_concentration + 0.000001f) / (locale_concentration + 0.000001f) :
                                                       (locale_concentration + 0.000001f) / (organism_concentration + 0.000001f);


        return 4 / (1 + ((float)molecule.Mass / Molecule.Water.Mass) / concentration_gradient);
    }
}
