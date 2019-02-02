using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotComponent : GoodBehavior
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

    SpriteRenderer highlight;
    bool is_highlighted = false;
    public bool IsHighlighted
    {
        get { return is_highlighted; }

        set
        {
            if (is_highlighted == value)
                return;

            is_highlighted = value;

            if (highlight == null)
            {
                highlight = new GameObject("highlight").AddComponent<SpriteRenderer>();
                highlight.sprite = Resources.Load<Sprite>("slot_highlight");
                highlight.transform.SetParent(transform, false);
            }

            highlight.gameObject.SetActive(is_highlighted);
        }
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
