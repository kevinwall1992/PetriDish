using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompoundComponent : GoodBehavior
{
    Compound validated_compound;

    [SerializeField]
    MoleculeComponent molecule_component;

    public Compound Compound { get; private set; }

    public MoleculeComponent MoleculeComponent { get { return molecule_component; } }

    public SlotComponent SlotComponent
    {
        get { return GetComponentInParent<SlotComponent>(); }
    }

    private void Awake()
    {
        
    }

    protected override void Start()
    {
        base.Start();   
    }

    protected override void Update()
    {
        base.Update();

        Validate();
    }

    void Validate()
    {
        if (Compound != null && ReferenceEquals(Compound, validated_compound))
            return;

        if (Compound == null)
        {
            molecule_component.SetMolecule(null);
            validated_compound = null;
        }
        else
        {
            molecule_component.SetMolecule(Compound.Molecule);
            validated_compound = Compound;
        }
    }

    public CompoundComponent SetCompound(Compound compound)
    {
        Compound = compound;
        Validate();

        return this;
    }
}