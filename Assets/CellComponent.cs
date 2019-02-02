using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CellComponent : GoodBehavior, IPointerClickHandler, Spawner
{
    Cell cell;

    List<SlotComponent> slot_components= new List<SlotComponent>();

    int last_slot0_index = 0;

    SpriteRenderer cytozol_highlight;

    public OrganismComponent OrganismComponent
    {
        get { return GetComponentInParent<OrganismComponent>(); }
    }

    public Cell Cell
    {
        get { return cell; }
    }

    public enum Part { Slot0, Slot1, Slot2, Slot3, Slot4, Slot5, Cytozol, None }

    public Part PartPointedAt
    {
        get
        {
            if (!IsPointedAt)
                return Part.None;

            Vector2 displacement = transform.InverseTransformPoint(Scene.Micro.Camera.ScreenToWorldPoint(Input.mousePosition));

            if (displacement.magnitude < 0.6f)
                return Part.Cytozol;
            else
            {
                float clock_radians = (Mathf.PI * 2 - MathUtility.GetRotation(displacement)) + Mathf.PI / 2;

                return (Part)(((int)(6 * (clock_radians + Mathf.PI / 6) / (2 * Mathf.PI))) % 6);
            }
        }
    }

    public Part TouchedPart
    {
        get
        {
            if (!IsTouched)
                return Part.None;

            return PartPointedAt;
        }
    }

    private void Awake()
    {
        gameObject.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("cell");

        gameObject.AddComponent<CircleCollider2D>().radius = 1.9f;

        gameObject.AddComponent<SpawnOnDragBehavior>();

        cytozol_highlight = new GameObject("highlight").AddComponent<SpriteRenderer>();
        cytozol_highlight.sprite = Resources.Load<Sprite>("cytozol_highlight");
        cytozol_highlight.transform.SetParent(transform, false);
    }

    void Start()
    {
        
    }

    void Update()
    {
        ValidateSlots();

        cytozol_highlight.gameObject.SetActive(false);
        foreach (SlotComponent slot_component in slot_components)
            slot_component.IsHighlighted = false;

        Part part_pointed_at = PartPointedAt;
        if (part_pointed_at == Part.None) ;
        else if (part_pointed_at == Part.Cytozol)
            cytozol_highlight.gameObject.SetActive(true);
        else
            GetSlotComponent((int)part_pointed_at).IsHighlighted = true;

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameObject.layer == LayerMask.NameToLayer("Example"))
            return;

        Part touched_part = TouchedPart;

        if (touched_part == Part.Cytozol)
            OrganismComponent.CytozolDetailPanel.Open();
        else if(GetSlotComponent(Cell.Slots[(int)touched_part]).DetailPanel != null)
            GetSlotComponent(Cell.Slots[(int)touched_part]).DetailPanel.Open();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
    }

    void SetSlotTransformations()
    {
        for (int i = 0; i < 6; i++)
        {
            slot_components[i].transform.localRotation = Quaternion.identity;
            slot_components[i].gameObject.transform.Rotate(new Vector3(0, 0, slot_components[i].Slot.Index * -60));
        }
    }

    void ValidateSlots()
    {
        if (last_slot0_index == slot_components[0].Slot.Index)
            return;

        SetSlotTransformations();
        last_slot0_index = slot_components[0].Slot.Index;

        transform.rotation = Quaternion.identity;
    }

    public CellComponent SetCell(Cell cell_)
    {
        cell = cell_;

        for (int i = 0; i < 6; i++)
        {
            slot_components.Add(new GameObject("slot").AddComponent<SlotComponent>().SetSlot(cell.Slots[i]));
            slot_components[i].transform.parent = transform;
        }

        SetSlotTransformations();

        return this;
    }

    public SlotComponent GetSlotComponent(int index)
    {
        return GetSlotComponent(Cell.Slots[index]);
    }

    public SlotComponent GetSlotComponent(Cell.Slot slot)
    {
        foreach (SlotComponent slot_component in slot_components)
            if (slot_component.Slot == slot)
                return slot_component;

        return null;
    }

    public GameObject Spawn()
    {
        if (PartPointedAt == Part.Cytozol)
            return null;

        Cell.Slot slot = Cell.Slots[(int)PartPointedAt];
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
