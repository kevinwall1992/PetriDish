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
    Animator spore;
    public Animator Spore { get { return spore; } }
}
