using UnityEngine;
using System.Collections;

public class MoleculeComponent : GoodBehavior
{
    Molecule molecule;

    [SerializeField]
    SpriteRenderer molecule_base;

    [SerializeField]
    Transform across_transform,
              right_transform,
              left_transform;

    [SerializeField]
    Color ribozyme_color,
          protein_color;

    [SerializeField]
    SpriteRenderer catalyst_background;

    [SerializeField]
    SpriteRenderer molecule_layer0;
    [SerializeField]
    SpriteRenderer molecule_layer1;

    [SerializeField]
    Animator animator;

    public Molecule Molecule { get { return molecule; } }

    [SerializeField]
    CatalystProgressIcon catalyst_progress_icon;
    public CatalystProgressIcon CatalystProgressIcon { get { return catalyst_progress_icon; } }

    public AttachmentComponent AcrossAttachmentComponent
    { get { return across_transform.GetComponentInChildren<AttachmentComponent>(); } }

    public AttachmentComponent RightAttachmentComponent
    { get { return right_transform.GetComponentInChildren<AttachmentComponent>(); } }

    public AttachmentComponent LeftAttachmentComponent
    { get { return left_transform.GetComponentInChildren<AttachmentComponent>(); } }

    public float Alpha
    {
        get { return molecule_base.color.a; }

        set
        {
            molecule_base.color = Utility.ChangeAlpha(molecule_base.color, value);
            catalyst_background.color = Utility.ChangeAlpha(catalyst_background.color, value);
        }
    }

    protected override void Update()
    {
        base.Update();

        CatalystProgressIcon.transform.position =
            Scene.Micro.Camera.ScreenToWorldPoint(
            Scene.Micro.Camera.WorldToScreenPoint(
                transform.position) + new Vector3(-15, 24));

        if (molecule is Catalyst)
        {
            transform.localRotation = Quaternion.Euler(0, 0, ((int)(molecule as Catalyst).Orientation) * 120);
        }
    }

    public MoleculeComponent SetMolecule(Molecule molecule_)
    {
        if (molecule != null && molecule.Equals(molecule_))
            return this;

        molecule = molecule_;

        transform.localRotation = Quaternion.identity;
        if (molecule is Catalyst)
            transform.localRotation = Quaternion.Euler(0, 0, ((int)(molecule as Catalyst).Orientation) * 120);

        catalyst_background.gameObject.SetActive(false);
        animator.runtimeAnimatorController = null;
        molecule_layer0.sprite = null;
        molecule_layer1.sprite = null;

        if (molecule != null)
            molecule_base.sprite = GetSprite(molecule);
        else molecule_base.sprite = null;

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

                catalyst_background.gameObject.SetActive(true);
                catalyst_background.color = molecule is Ribozyme ? ribozyme_color : protein_color;

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
                    else if (attachment is Extruder)
                        attachment_component = Instantiate(Scene.Micro.Prefabs.ExtruderComponent);
                    else if (attachment is Separator)
                        attachment_component = Instantiate(Scene.Micro.Prefabs.SeparatorComponent);

                    attachment_component.transform.SetParent(attachment_transform, false);
                    attachment_component.SetAttachment(attachment, catalyst is Ribozyme ? ribozyme_color : protein_color);
                }
            }

        }

        if (molecule != null && molecule is Catalyst)
        {
            Catalyst catalyst = molecule as Catalyst;

            if (catalyst.GetFacet<Interpretase>() != null)
                animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animations/MoleculeComponent/Think");
            else if (catalyst.GetFacet<Constructase>() != null)
                animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animations/MoleculeComponent/Caulk");
            else if (catalyst.GetFacet<Reaction.ReactionCatalyst>() != null)
            {
                animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animations/MoleculeComponent/FactoryFlue");

                switch (molecule.Name)
                {
                    case "Structatogenase":
                        molecule_base.color = Color.white;
                        molecule_layer0.color = Color.Lerp(Color.grey, Color.white, 0.5f);
                        molecule_layer1.color = Color.Lerp(Color.yellow, Color.red, 0.3f);
                        break;

                    case "NRG Synthase":
                        molecule_base.color = Color.Lerp(Color.yellow, Color.white, 0.5f);
                        molecule_layer0.color = Color.blue;
                        molecule_layer1.color = Color.yellow;
                        break;
                }
            }
        }

        return this;
    }

    public static Sprite GetSprite(Molecule molecule)
    {
        string name = null;

        switch (molecule.Name)
        {
            case "Water": name = "water"; break;
            case "Structate": name = "sugar"; break;
            case "Nitrogen": name = "nitrogen_gas"; break;
            case "Hindenburgium Gas": name = "hydrogen_gas"; break;
            case "Karbon Diaeride": name = "carbon_dioxide"; break;
            case "Umamium Gas": name = "nitrogen_gas"; break;
            case "Aerogen Gas": name = "carbon_dioxide"; break;
            case "Phlorate": name = "phosphate"; break;
            case "Umomia": name = "ammonia"; break;
            case "Hindenburgium Stankide": name = "hydrogen_sulfide"; break;
            case "NRG": name = (molecule as ChargeableMolecule).IsCharged ? "battery" : "empty_battery"; break;
            case "Genes": name = "genes"; break;

            case "Interpretase": name = "brain"; break;
            case "Constructase": name = "caulk"; break;
            case "Separatase": return null;
            case "Gene Synthase": name = "genes_factory"; break;
        }

        if (name == null)
        {
            if (molecule is Catalyst && (molecule as Catalyst).GetFacet<Reaction.ReactionCatalyst>() != null)
                name = "factory";
            else if (molecule is Ribozyme)
                name = "ribozyme";
            else if (molecule is Protein)
                name = "protein";
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
