using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotComponent : GoodBehavior, Spawner
{
    public Cell.Slot Slot { get; private set; }

    [SerializeField]
    Transform left_corner, right_corner, bottom_corner, outside;
    public Transform LeftCorner { get { return left_corner; } }
    public Transform RightCorner { get { return right_corner; } }
    public Transform BottomCorner { get { return bottom_corner; } }
    public Transform Outside { get { return outside; } }

    [SerializeField]
    CompoundComponent compound_component;
    public CompoundComponent CompoundComponent { get { return compound_component; } }

    [SerializeField]
    SpriteRenderer highlight;

    public CellComponent CellComponent
    {
        get { return GetComponentInParent<CellComponent>(); }
    }

    public Vector2 Center
    {
        get { return CompoundComponent.transform.position; }
    }

    DetailPanel detail_panel = null;
    public DetailPanel DetailPanel
    {
        get
        {
            if (CompoundComponent.Compound == null)
                return null;

            Molecule molecule = CompoundComponent.Compound.Molecule;

            if (detail_panel is DNAPanel && molecule is DNA && ReferenceEquals(molecule as DNA, (detail_panel as DNAPanel).DNA))
                return detail_panel;

            if (detail_panel != null)
            {
                Destroy(detail_panel);
                detail_panel = null;
            }

            if (molecule is Catalyst)
            {
                if (Interpretase.GetGeneticCofactor(molecule as Catalyst) != null)
                    detail_panel = DNAPanel.Create(Slot);
                else
                    detail_panel = CatalystPanel.Create(CompoundComponent.Compound.Molecule as Catalyst);
            }
            else if (molecule is DNA)
                detail_panel = DNAPanel.Create(Slot);
            else
                detail_panel = null;

            return detail_panel;
        }
    }

    private void Awake()
    {
        
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (CompoundComponent.Compound != Slot.Compound)
        {
            CompoundComponent.SetCompound(Slot.Compound);
            detail_panel = null;
        }

        highlight.gameObject.SetActive(IsTouched);
    }

    public SlotComponent SetSlot(Cell.Slot slot)
    {
        Slot = slot;

        return this;
    }

    public GameObject Spawn()
    {
        Cell.Slot slot = Slot;
        if (slot.Compound == null)
            return null;

        CompoundTile compound_tile = Instantiate(Scene.Micro.Prefabs.CompoundTile);
        compound_tile.transform.parent = Scene.Micro.Canvas.transform;

        if (Input.GetKey(KeyCode.LeftControl))
            compound_tile.Compound = slot.Compound.Split(slot.Compound.Quantity / 2);
        else
            compound_tile.Compound = slot.RemoveCompound();

        return compound_tile.gameObject;
    }
}
