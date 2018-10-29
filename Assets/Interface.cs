using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Interface<AType, BType> : Chronal where AType : Volume where BType : Volume
{
    public abstract AType A { get; }
    public abstract BType B { get; }

    public abstract float SurfaceArea
    {
        get;
    }

    public Interface()
    {
        
    }

    protected abstract float GetQuantityDiffusedByA(Molecule molecule, float time);
    protected abstract float GetQuantityDiffusedByB(Molecule molecule, float time);

    public void Diffuse(float time)
    {
        foreach (Molecule molecule in A.Molecules)
            TransportAtoB(molecule, GetQuantityDiffusedByA(molecule, time));

        foreach (Molecule molecule in B.Molecules)
            TransportBtoA(molecule, GetQuantityDiffusedByB(molecule, time));
    }

    public abstract void TransportAtoB(Compound compound);
    public abstract void TransportBtoA(Compound compound);

    public void TransportAtoB(Molecule molecule, float quantity) { TransportAtoB(new Compound(molecule, quantity)); }
    public void TransportBtoA(Molecule molecule, float quantity) { TransportBtoA(new Compound(molecule, quantity)); }

    public void Step()
    {
        Diffuse(1.0f);
    }
}

public abstract class FixedInterface<AType, BType> : Interface<AType, BType> where AType : Volume where BType : Volume
{
    AType a;
    BType b;

    public override AType A { get { return a; } }
    public override BType B { get { return b; } }

    public FixedInterface(AType a_, BType b_)
    {
        a = a_;
        b = b_;
    }
}
