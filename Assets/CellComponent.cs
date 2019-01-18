using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CellComponent : GoodBehavior, IPointerClickHandler
{
    Cell cell;

    List<SlotComponent> slot_components= new List<SlotComponent>();

    SpriteRenderer highlight;
    Part current_highlighted_part = Part.None;

    int last_slot0_index = 0;

    public OrganismComponent OrganismComponent
    {
        get { return GetComponentInParent<OrganismComponent>(); }
    }

    public Cell Cell
    {
        get { return cell; }
    }

    private void Awake()
    {
        gameObject.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("cell");

        gameObject.AddComponent<CircleCollider2D>().radius = 1.9f;

        highlight = new GameObject("highlight").AddComponent<SpriteRenderer>();
        highlight.gameObject.SetActive(false);
    }

    void Start()
    {
        
    }

    void Update()
    {
        ValidateSlots();


        Part hovered_part = GetHoveredPart();
        if (current_highlighted_part != hovered_part)
        {
            if (hovered_part != Part.None)
            {
                highlight.gameObject.SetActive(true);
                highlight.transform.parent = null;
                highlight.transform.rotation = Quaternion.identity;

                if (hovered_part == Part.Cytozol)
                {
                    highlight.sprite = Resources.Load<Sprite>("cytozol_highlight");
                    highlight.transform.parent = transform;
                }
                else
                {
                    highlight.sprite = Resources.Load<Sprite>("slot_highlight");
                    highlight.transform.SetParent(slot_components[(int)hovered_part].transform, false);
                }
            }
            else
                highlight.gameObject.SetActive(false);

            current_highlighted_part = hovered_part;
        }

    }

    enum Part { Slot0, Slot1, Slot2, Slot3, Slot4, Slot5, Cytozol, None }
    Part GetHoveredPart()
    {
        if (!IsTouched)
            return Part.None;

        Vector2 displacement = transform.InverseTransformPoint(Scene.Micro.Camera.ScreenToWorldPoint(Input.mousePosition)) - transform.position;

        if (displacement.magnitude < 0.6f)
            return Part.Cytozol;
        else
        {
            float clock_radians = (Mathf.PI * 2 - MathUtility.GetRotation(displacement)) + Mathf.PI / 2;

            return (Part)(((int)(6 * (clock_radians + Mathf.PI / 6) / (2 * Mathf.PI)) - slot_components[0].Slot.Index) % 6);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameObject.layer == LayerMask.NameToLayer("Example"))
            return;

        Part hovered_part = GetHoveredPart();

        if (hovered_part == Part.Cytozol)
            OrganismComponent.CytozolDetailPanel.Open();
        else if(slot_components[(int)hovered_part].DetailPanel != null)
            slot_components[(int)hovered_part].DetailPanel.Open();
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
}
