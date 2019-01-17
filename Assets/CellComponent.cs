using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CellComponent : MonoBehaviour, IPointerClickHandler
{
    Cell cell;

    List<SlotComponent> slot_components= new List<SlotComponent>();

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
    }

    void Start()
    {
        
    }

    void Update()
    {
        ValidateSlots();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameObject.layer == LayerMask.NameToLayer("Example"))
            return;

        Vector2 displacement = transform.InverseTransformPoint(Scene.Micro.Camera.ScreenToWorldPoint(Input.mousePosition)) - transform.position;

        if (displacement.magnitude < 0.5)
            OrganismComponent.CytozolDetailPanel.Open();
        else
        {
            float clock_radians = (Mathf.PI * 2 - MathUtility.GetRotation(displacement)) + Mathf.PI / 2;

            int index = (int)(6 * (clock_radians + Mathf.PI / 6) / (2 * Mathf.PI));
            SlotComponent slot_component = GetSlotComponent(index);

            if (slot_component.DetailPanel != null)
                slot_component.DetailPanel.Open();
        }
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
