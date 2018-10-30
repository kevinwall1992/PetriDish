using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellComponent : MonoBehaviour
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
    }

    void Start()
    {
        
    }

    void Update()
    {
        ValidateSlots();
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
            slot_components.Add(new GameObject("slot").AddComponent<SlotComponent>().SetSlot(cell.GetSlot(i)));
            slot_components[i].transform.parent = transform;
        }

        SetSlotTransformations();

        return this;
    }

    public SlotComponent GetSlotComponent(int index)
    {
        return slot_components[index];
    }

    public SlotComponent GetSlotComponent(Cell.Slot slot)
    {
        foreach (SlotComponent slot_component in slot_components)
            if (slot_component.Slot == slot)
                return slot_component;

        return null;
    }
}
