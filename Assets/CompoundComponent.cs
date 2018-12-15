using System.Collections;
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


        string resource_name = null;
        switch (Compound.Molecule.Name)
        {
            case "Water": resource_name = "water"; break;
            case "Oxygen": resource_name = "gas"; break;
            case "Purine": resource_name = "purine"; break;
            case "Pyrimidine": resource_name = "pyrimidine"; break;
            case "ATP": resource_name = "atp"; break;
            case "ADP": resource_name = "adp"; break;
            case "Phospholipid": resource_name = "lipid"; break;
            case "Glucose": resource_name = "sugar"; break;
            default: break;
        }
        if (resource_name == null)
        {
            if (Compound.Molecule is Catalyst)
                resource_name = "catalyst";
            else if (Compound.Molecule is AminoAcid)
                resource_name = "amino_acid";
            else if (Compound.Molecule is Nucleotide)
                resource_name = "nucleotide";
            else if (Compound.Molecule is DNA)
                resource_name = "dna";
        }
        if (resource_name == null)
            resource_name = "generic_molecule";

        gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(resource_name);
        current_molecule_name = Compound.Molecule.Name;
    }

    public CompoundComponent SetCompound(Compound compound_)
    {
        compound = compound_;

        return this;
    }
}