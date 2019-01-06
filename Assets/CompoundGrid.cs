using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class CompoundGrid : DetailPanel
{
    static CompoundTile compound_tile_prefab;
    GridLayoutGroup GridLayout
    {
        get { return FindDescendent<GridLayoutGroup>("grid"); }
    }

    IMutableContainer<Compound> CompoundContainer
    {
        get
        {
            return Data as IMutableContainer<Compound>;
        }
    }

    public override object Data
    {
        set
        {
            base.Data = value;
        }
    }

    public int RowLength { get; set; }

    protected void Start()
    {
        Scene.Micro.Visualization.IsPaused = true;

        RowLength = 5;
    }

    protected override void Update()
    {
        if (CompoundContainer.WasModified(this))
            UpdateTiles();

        float width = (GridLayout.transform as RectTransform).rect.width;
        float space = width * 0.02f;
        float size = (width - space * (RowLength - 1)) / RowLength;

        GridLayout.cellSize = new Vector2(size, size);
        GridLayout.spacing = new Vector2(space, space);

        base.Update();
    }

    void UpdateTiles()
    {
        InitializePrefabs();

        foreach (CompoundTile compound_tile in GridLayout.GetComponentsInChildren<CompoundTile>())
            GameObject.Destroy(compound_tile.gameObject);

        foreach(Compound compound in CompoundContainer.Elements)
        {
            if (compound.Quantity == 0)
                continue;

            CompoundTile compound_tile = GameObject.Instantiate(compound_tile_prefab.gameObject).GetComponent<CompoundTile>();
            compound_tile.transform.parent = GridLayout.transform;
            compound_tile.gameObject.SetActive(true);
            compound_tile.Compound = compound;
        }
    }

    public void AddCompound(Compound compound)
    {
        CompoundContainer.AddElement(compound);
    }

    static CompoundGrid compound_grid_prefab;
    public static CompoundGrid Create(IMutableContainer<Compound> compound_container)
    {
        InitializePrefabs();

        CompoundGrid compound_grid = GameObject.Instantiate(compound_grid_prefab.gameObject).GetComponent<CompoundGrid>();
        compound_grid.transform.SetParent(compound_grid_prefab.transform.parent, false);

        compound_grid.Data = compound_container;

        return compound_grid;
    }

    static void InitializePrefabs()
    {
        if (compound_grid_prefab != null)
            return;

        compound_grid_prefab = FindObjectsOfTypeAll(typeof(CompoundGrid))[0] as CompoundGrid;

        compound_tile_prefab = compound_grid_prefab.FindDescendent<CompoundTile>("compound_tile");
        compound_tile_prefab.transform.parent = null;
        compound_tile_prefab.gameObject.SetActive(false);
    }
}