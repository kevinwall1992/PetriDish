﻿using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CompoundGridPanel : GridPanel
{
    IMutableContainer<Compound> CompoundContainer
    {
        get { return Data as IMutableContainer<Compound>; }
    }

    protected override void Start()
    {
        base.Start();

        RowLength = 4;
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

        foreach(Compound compound in CompoundContainer.Items)
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
        CompoundContainer.AddItem(compound);
    }

    public Compound RemoveCompound(Molecule molecule, float quantity)
    {
        Compound compound = new Compound(molecule, 0);

        foreach (Compound other_compound in CompoundContainer.Items)
            if (other_compound.Molecule.Equals(compound.Molecule))
            {
                Compound removed_compound = CompoundContainer.RemoveItem(other_compound);

                compound.Quantity += removed_compound.Split(quantity - compound.Quantity).Quantity;

                CompoundContainer.AddItem(removed_compound);
            }

        return compound;
    }

    public static CompoundGridPanel Create(IMutableContainer<Compound> compound_container)
    {
        CompoundGridPanel compound_grid = Instantiate(Scene.Micro.Prefabs.CompoundGridPanel);
        compound_grid.transform.SetParent(Scene.Micro.DetailPanelContainer, false);

        compound_grid.Data = compound_container;

        return compound_grid;
    }
}