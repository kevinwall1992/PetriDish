using UnityEngine;
using System.Collections;

public class MicroPrefabs : MonoBehaviour
{
    [SerializeField]
    CompoundGridPanel compound_grid_panel;
    public CompoundGridPanel CompoundGridPanel { get { return compound_grid_panel; } }

    [SerializeField]
    CompoundTile compound_tile;
    public CompoundTile CompoundTile { get { return compound_tile; } }

    [SerializeField]
    DNAPanel dna_panel;
    public DNAPanel DNAPanel { get { return dna_panel; } }
}
