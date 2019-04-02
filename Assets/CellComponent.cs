using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CellComponent : GoodBehavior, IPointerClickHandler
{
    [SerializeField]
    Collider2D cytosol_collider;

    [SerializeField]
    SpriteRenderer cytosol_highlight;

    int last_slot0_index = 0;

    IEnumerable<SlotComponent> SlotComponents { get { return GetComponentsInChildren<SlotComponent>(); } }

    public OrganismComponent OrganismComponent
    {
        get { return GetComponentInParent<OrganismComponent>(); }
    }

    public Cell Cell { get; private set; }

    public SlotComponent SlotComponentPointedAt
    {
        get
        {
            foreach (SlotComponent slot_component in SlotComponents)
                if (slot_component.IsPointedAt)
                    return slot_component;

            return null;
        }
    }

    public SlotComponent SlotComponentTouched
    {
        get
        {
            if (SlotComponentPointedAt.IsTouched)
                return SlotComponentPointedAt;

            return null;
        }
    }

    public bool IsCytosolPointedAt
    {
        get { return cytosol_collider.GetComponent<GoodBehavior>().IsPointedAt; }
    }

    public bool IsCytosolTouched
    {
        get { return cytosol_collider.GetComponent<GoodBehavior>().IsTouched; }
    }

    private void Awake()
    {

    }

    void Start()
    {
        
    }

    void Update()
    {
        cytosol_highlight.gameObject.SetActive(IsCytosolTouched);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameObject.layer == LayerMask.NameToLayer("Example"))
            return;

        if (IsCytosolTouched)
            OrganismComponent.CytosolDetailPanel.Open();
        else if(SlotComponentPointedAt.DetailPanel != null)
            SlotComponentPointedAt.DetailPanel.Open();
       
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
    }

    public CellComponent SetCell(Cell cell)
    {
        Cell = cell;

        int i = 0;
        foreach (SlotComponent slot_component in SlotComponents)
            slot_component.SetSlot(Cell.Slots[i++]);

        return this;
    }

    public SlotComponent GetSlotComponent(int index)
    {
        return GetSlotComponent(Cell.Slots[index]);
    }

    public SlotComponent GetSlotComponent(Cell.Slot slot)
    {
        foreach (SlotComponent slot_component in SlotComponents)
            if (slot_component.Slot == slot)
                return slot_component;

        return null;
    }
}
