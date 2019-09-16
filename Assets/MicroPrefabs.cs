using UnityEngine;
using System.Collections;

public class MicroPrefabs : MonoBehaviour
{
    //UI
    [SerializeField]
    CompoundGridPanel compound_grid_panel;
    public CompoundGridPanel CompoundGridPanel { get { return compound_grid_panel; } }

    [SerializeField]
    CompoundTile compound_tile;
    public CompoundTile CompoundTile { get { return compound_tile; } }

    [SerializeField]
    DNAPanel dna_panel;
    public DNAPanel DNAPanel { get { return dna_panel; } }

    [SerializeField]
    CatalystPanel catalyst_panel;
    public CatalystPanel CatalystPanel { get { return catalyst_panel; } }

    [SerializeField]
    DeckPanel deck_panel;
    public DeckPanel DeckPanel { get { return deck_panel; } }

    [SerializeField]
    Card card;
    public Card Card { get { return card; } }

    [SerializeField]
    TrashcanPanel trashcan_panel;
    public TrashcanPanel TrashcanPanel { get { return trashcan_panel; } }


    //Game
    [SerializeField]
    OrganismComponent organism_component;
    public OrganismComponent OrganismComponent { get { return organism_component; } }

    [SerializeField]
    CellComponent cell_component;
    public CellComponent CellComponent { get { return cell_component; } }

    [SerializeField]
    CompoundComponent compound_component;
    public CompoundComponent CompoundComponent { get { return compound_component; } }

    [SerializeField]
    Animator spore_animator;
    public Animator SporeAnimator { get { return spore_animator; } }

    [SerializeField]
    Animator construction_animator;
    public Animator ConstructionAnimator { get { return construction_animator; } }

    [SerializeField]
    AttachmentComponent input_attachment_component;
    public AttachmentComponent InputAttachmentComponent { get { return input_attachment_component; } }

    [SerializeField]
    AttachmentComponent output_attachment_component;
    public AttachmentComponent OutputAttachmentComponent { get { return output_attachment_component; } }

    [SerializeField]
    GrabberComponent grabber_component;
    public GrabberComponent GrabberComponent { get { return grabber_component; } }

    [SerializeField]
    AttachmentComponent extruder_component;
    public AttachmentComponent ExtruderComponent { get { return extruder_component; } }

    [SerializeField]
    AttachmentComponent separator_component;
    public AttachmentComponent SeparatorComponent { get { return separator_component; } }

    [SerializeField]
    SectorNode sector_node;
    public SectorNode SectorNode { get { return sector_node; } }

    [SerializeField]
    CommandNode command_node;
    public CommandNode CommandNode { get { return command_node; } }

    [SerializeField]
    LocusNode locus_node;
    public LocusNode LocusNode { get { return locus_node; } }

    [SerializeField]
    CatalystNode catalyst_node;
    public CatalystNode CatalystNode { get { return catalyst_node; } }
}
