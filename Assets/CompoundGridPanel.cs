using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class CompoundGridPanel : GridPanel
{
    IMutableContainer<Compound> CompoundContainer
    {
        get { return Data as IMutableContainer<Compound>; }
    }

    protected override void Update()
    {
        if (CompoundContainer.WasModified(this))
            UpdateTiles();

        base.Update();
    }

    void UpdateTiles()
    {
        foreach (CompoundTile compound_tile in GridLayoutGroup.GetComponentsInChildren<CompoundTile>())
            GameObject.Destroy(compound_tile.gameObject);

        foreach(Compound compound in CompoundContainer.Elements)
        {
            if (compound.Quantity == 0)
                continue;

            CompoundTile compound_tile = Instantiate(Scene.Micro.Prefabs.CompoundTile);
            compound_tile.transform.parent = GridLayoutGroup.transform;
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