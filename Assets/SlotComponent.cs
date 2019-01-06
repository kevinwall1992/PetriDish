﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotComponent : MonoBehaviour, HasDetailPanel
{
    public Cell.Slot Slot { get; private set; }

    public GameObject LeftCorner { get; private set; }

    public GameObject RightCorner { get; private set; }

    public GameObject Outside { get; private set; }

    public CompoundComponent CompoundComponent { get; private set; }

    public CellComponent CellComponent
    {
        get { return GetComponentInParent<CellComponent>(); }
    }

    public Vector2 Center
    {
        get { return CompoundComponent.transform.position; }
    }

    DetailPanel detail_panel;
    public DetailPanel DetailPanel
    {
        get
        {
            if (detail_panel == null &&
                CompoundComponent.Compound != null && 
                CompoundComponent.Compound.Molecule is DNA && 
                !(CompoundComponent.Compound.Molecule is Catalyst))

                detail_panel = DNAPanel.Create(CompoundComponent.Compound.Molecule as DNA);

            return detail_panel;
        }
    }

    private void Awake()
    {
        gameObject.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("slot");

        LeftCorner = new GameObject("left_corner");
        LeftCorner.transform.parent = transform;
        LeftCorner.transform.localPosition = new Vector3(-0.4f, 1.5f);

        RightCorner = new GameObject("right_corner");
        RightCorner.transform.parent = transform;
        RightCorner.transform.localPosition = new Vector3(0.4f, 1.5f);

        Outside = new GameObject("outside");
        Outside.transform.parent = transform;
        Outside.transform.localPosition = new Vector3(0.0f, 3.0f);
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (CompoundComponent.Compound != Slot.Compound)
        {
            CompoundComponent.SetCompound(Slot.Compound);
            detail_panel = null;
        }
    }

    public SlotComponent SetSlot(Cell.Slot slot)
    {
        Slot = slot;

        if(CompoundComponent == null)
        {
            CompoundComponent = new GameObject("compound").AddComponent<CompoundComponent>();
            CompoundComponent.transform.parent = this.transform;
            CompoundComponent.transform.localPosition = new Vector3(0, 1.3f, 0);
        }

        return this;
    }
}
