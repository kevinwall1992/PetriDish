using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class CompoundGridPanel : DetailPanel
{
    [SerializeField]
    GridLayoutGroup grid_layout;

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

        float width = (grid_layout.transform as RectTransform).rect.width;
        float space = width * 0.02f;
        float size = (width - space * (RowLength - 1)) / RowLength;

        grid_layout.cellSize = new Vector2(size, size);
        grid_layout.spacing = new Vector2(space, space);

        base.Update();
    }

    void UpdateTiles()
    {
        foreach (CompoundTile compound_tile in grid_layout.GetComponentsInChildren<CompoundTile>())
            GameObject.Destroy(compound_tile.gameObject);

        foreach(Compound compound in CompoundContainer.Elements)
        {
            if (compound.Quantity == 0)
                continue;

            CompoundTile compound_tile = Instantiate(Scene.Micro.Prefabs.CompoundTile);
            compound_tile.transform.parent = grid_layout.transform;
            compound_tile.Compound = compound;
        }
    }

    public void AddCompound(Compound compound)
    {
        CompoundContainer.AddElement(compound);
    }

    public static CompoundGridPanel Create(IMutableContainer<Compound> compound_container)
    {
        CompoundGridPanel compound_grid = Instantiate(Scene.Micro.Prefabs.CompoundGridPanel);
        compound_grid.transform.SetParent(Scene.Micro.Canvas.transform, false);

        compound_grid.Data = compound_container;

        return compound_grid;
    }
}