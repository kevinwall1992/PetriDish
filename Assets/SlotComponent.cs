using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotComponent : GoodBehavior, Spawner
{
    public Cell.Slot Slot { get; private set; }

    [SerializeField]
    Transform left_corner, right_corner, outside;
    public Transform LeftCorner { get { return left_corner; } }
    public Transform RightCorner { get { return right_corner; } }
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

    DetailPanel detail_panel;
    public DetailPanel DetailPanel
    {
        get
        {
            if (detail_panel == null && CompoundComponent.Compound != null)
            {
                if (CompoundComponent.Compound.Molecule is Catalyst)
                    detail_panel = CatalystPanel.Create(CompoundComponent.Compound.Molecule as Catalyst);
                else if (CompoundComponent.Compound.Molecule is DNA)
                    detail_panel = DNAPanel.Create(CompoundComponent.Compound.Molecule as DNA);
            }

            return detail_panel;
        }
    }

    private void Awake()
    {
        
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
