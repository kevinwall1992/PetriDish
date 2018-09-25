using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CompoundComponent : MonoBehaviour
{
    Compound compound;

    string current_molecule_name= "";

    public Compound Compound
    {
        get { return compound; }
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
        else if(current_molecule_name == compound.Molecule.GetName())
            return;


        string resource_name = null;
        switch (Compound.Molecule.GetName())
        {
            case "Water": resource_name = "water"; break;
            case "Oxygen": resource_name = "gas"; break;
            case "Purine": resource_name = "purine"; break;
            case "Pyrimidine": resource_name = "pyrimidine"; break;
            case "ATP": resource_name = "atp"; break;
            case "ADP": resource_name = "adp"; break;
            default: break;
        }
        if (resource_name == null)
        {
            if (Compound.Molecule is Catalyst)
                resource_name = "enzyme";
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
        current_molecule_name = Compound.Molecule.GetName();
    }

    public CompoundComponent SetCompound(Compound compound_)
    {
        compound = compound_;

        return this;
    }
}