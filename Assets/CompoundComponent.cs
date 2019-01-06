﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompoundComponent : MonoBehaviour
{
    Compound compound;

    string current_molecule_name= "";

    public Compound Compound
    {
        get { return compound; }
    }

    public SlotComponent SlotComponent
    {
        get { return GetComponentInParent<SlotComponent>(); }
    }

    private void Awake()
    {
        gameObject.AddComponent<SpriteRenderer>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        ValidateCompound();
    }

    void ValidateCompound()
    {
        if (compound == null)
        {
            if (current_molecule_name != "")
            {
                gameObject.GetComponent<SpriteRenderer>().sprite = null;
                current_molecule_name = "";
            }

            return;
        }
        else if(current_molecule_name == compound.Molecule.Name)
            return;


        gameObject.GetComponent<SpriteRenderer>().sprite = GetSprite(compound.Molecule);
        current_molecule_name = Compound.Molecule.Name;
    }

    public CompoundComponent SetCompound(Compound compound_)
    {
        compound = compound_;

        return this;
    }

    //Move this somewhere else
    public static Sprite GetSprite(Molecule molecule)
    {
        string resource_name = null;
        switch (molecule.Name)
        {
            case "Water": resource_name = "water"; break;
            case "Oxygen": resource_name = "oxygen"; break;
            case "Nitrogen": resource_name = "nitrogen_gas"; break;
            case "Hydrogen": resource_name = "hydrogen_gas"; break;
            case "Carbon Monoxide": resource_name = "carbon_monoxide"; break;
            case "Carbon Dioxide": resource_name = "carbon_dioxide"; break;
            case "Hydrogen Sulfide": resource_name = "hydrogen_sulfide"; break;
            case "Purine": resource_name = "purine"; break;
            case "Pyrimidine": resource_name = "pyrimidine"; break;
            case "Imidazole": resource_name = "imidazole"; break;
            case "ATP": resource_name = "atp"; break;
            case "ADP": resource_name = "adp"; break;
            case "Phospholipid": resource_name = "lipid"; break;
            case "Glucose": resource_name = "sugar"; break;
            case "Phosphate": resource_name = "phosphate"; break;
            case "AMP": resource_name = "amp"; break;
            case "CMP": resource_name = "cmp"; break;
            case "GMP": resource_name = "gmp"; break;
            case "TMP": resource_name = "tmp"; break;
            case "Methane": resource_name = "methane"; break;
            case "Sulfur": resource_name = "sulfur"; break;
            case "Ammonia": resource_name = "ammonia"; break;
            case "Vinegar": resource_name = "vinegar"; break;
            case "Pyruvate": resource_name = "pyruvate"; break;
            default: break;
        }
        if (resource_name == null)
        {
            if (molecule is Catalyst)
                resource_name = "catalyst";
            else if (molecule is AminoAcid)
                resource_name = "amino_acid";
            else if (molecule is DNA)
                resource_name = "dna";
        }
        if (resource_name == null)
            resource_name = "generic_molecule";

        return Resources.Load<Sprite>(resource_name);
    }
}