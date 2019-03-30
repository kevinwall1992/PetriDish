using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompoundComponent : GoodBehavior
{
    Compound compound_copy;

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

    void Start()
    {
        
    }

    void Update()
    {
        Validate();
    }

    void Validate()
    {
        if (Compound != null && Compound.Equals(compound_copy))
            return;

        if (Compound == null)
        {
            molecule_component.SetMolecule(null);
            compound_copy = null;
        }
        else
        {
            molecule_component.SetMolecule(Compound.Molecule);
            compound_copy = Compound.Copy();
        }
    }

    public CompoundComponent SetCompound(Compound compound)
    {
        Compound = compound;
        Validate();

        return this;
    }
}