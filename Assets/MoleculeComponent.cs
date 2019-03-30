using UnityEngine;
using System.Collections;

public class MoleculeComponent : GoodBehavior
{
    Molecule molecule, molecule_copy;

    [SerializeField]
    SpriteRenderer sprite_renderer;

    [SerializeField]
    Transform across_transform,
              right_transform,
              left_transform;

    [SerializeField]
    Color ribozyme_color, 
          enzyme_color;

    public AttachmentComponent AcrossAttachmentComponent
    { get { return across_transform.GetComponentInChildren<AttachmentComponent>(); } }

    public AttachmentComponent RightAttachmentComponent
    { get { return right_transform.GetComponentInChildren<AttachmentComponent>(); } }

    public AttachmentComponent LeftAttachmentComponent
    { get { return left_transform.GetComponentInChildren<AttachmentComponent>(); } }

    void Update()
    {
        Validate();
    }

    void Validate()
    {
        if (molecule != null && molecule.Equals(molecule_copy))
            return;

        if (molecule == null)
        {
            sprite_renderer.sprite = null;
            molecule_copy = null;
        }
        else
        {
            molecule_copy = molecule.Copy();

            if (molecule is Catalyst)
                transform.localRotation = Quaternion.Euler(0, 0, ((int)(molecule as Catalyst).Orientation + 1) * -120);
        }
    }

    public MoleculeComponent SetMolecule(Molecule molecule_)
    {
        molecule = molecule_;

        transform.localRotation = Quaternion.identity;

        if (molecule != null)
            sprite_renderer.sprite = GetSprite(molecule);

        foreach (Cell.Slot.Relation direction in System.Enum.GetValues(typeof(Cell.Slot.Relation)))
        {
            Transform attachment_transform = null;
            switch (direction)
            {
                case Cell.Slot.Relation.Across: attachment_transform = across_transform; break;
                case Cell.Slot.Relation.Right: attachment_transform = right_transform; break;
                case Cell.Slot.Relation.Left: attachment_transform = left_transform; break;

                case Cell.Slot.Relation.None: continue;
            }

            AttachmentComponent existing_attachment_component = attachment_transform.GetComponentInChildren<AttachmentComponent>();
            if (existing_attachment_component != null)
            {
                existing_attachment_component.transform.SetParent(null);
                Destroy(existing_attachment_component.gameObject);
            }

            if (molecule != null && molecule is Catalyst)
            {
                Catalyst catalyst = molecule as Catalyst;

                if (catalyst.Attachments.ContainsKey(direction))
                {
                    Attachment attachment = catalyst.Attachments[direction];
                    AttachmentComponent attachment_component = null;

                    if (attachment is InputAttachment)
                        attachment_component = Instantiate(Scene.Micro.Prefabs.InputAttachmentComponent);
                    else if (attachment is OutputAttachment)
                        attachment_component = Instantiate(Scene.Micro.Prefabs.OutputAttachmentComponent);
                    else if (attachment is Grabber)
                        attachment_component = Instantiate(Scene.Micro.Prefabs.GrabberComponent);

                    attachment_component.transform.SetParent(attachment_transform, false);
                    attachment_component.SetAttachment(attachment, catalyst is Ribozyme ? ribozyme_color : enzyme_color);
                }
            }
        }

        Validate();

        return this;
    }

    public static Sprite GetSprite(Molecule molecule)
    {
        string name = null;

        switch (molecule.Name)
        {
            case "Water": name = "water"; break;
            case "Oxygen": name = "oxygen"; break;
            case "Nitrogen": name = "nitrogen_gas"; break;
            case "Hydrogen": name = "hydrogen_gas"; break;
            case "Carbon Monoxide": name = "carbon_monoxide"; break;
            case "Carbon Dioxide": name = "carbon_dioxide"; break;
            case "Hydrogen Sulfide": name = "hydrogen_sulfide"; break;
            case "Purine": name = "purine"; break;
            case "Pyrimidine": name = "pyrimidine"; break;
            case "Imidazole": name = "imidazole"; break;
            case "ATP": name = "atp"; break;
            case "ADP": name = "adp"; break;
            case "Phospholipid": name = "lipid"; break;
            case "Glucose": name = "sugar"; break;
            case "Phosphate": name = "phosphate"; break;
            case "AMP": name = "amp"; break;
            case "CMP": name = "cmp"; break;
            case "GMP": name = "gmp"; break;
            case "TMP": name = "tmp"; break;
            case "Methane": name = "methane"; break;
            case "Sulfur": name = "sulfur"; break;
            case "Ammonia": name = "ammonia"; break;
            case "Vinegar": name = "vinegar"; break;
            case "Pyruvate": name = "pyruvate"; break;
        }

        if (name == null)
        {
            if (molecule is Ribozyme)
                name = "ribozyme";
            else if (molecule is Enzyme)
                name = "enzyme";
            else if (molecule is AminoAcid)
                name = "amino_acid";
            else if (molecule is DNA)
                name = "dna";
            else
                name = "generic_molecule";
        }

        return Resources.Load<Sprite>(name);
    }
}
