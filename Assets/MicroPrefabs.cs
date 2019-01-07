using UnityEngine;
using System.Collections;

public class MicroPrefabs : MonoBehaviour
{
    [SerializeField]
    CompoundGrid compound_grid;
    public CompoundGrid CompoundGrid { get { return compound_grid; } }

    [SerializeField]
    CompoundTile compound_tile;
    public CompoundTile CompoundTile { get { return compound_tile; } }

    [SerializeField]
    DNAPanel dna_panel;
    public DNAPanel DNAPanel { get { return dna_panel; } }
}
