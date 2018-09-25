using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellComponent : MonoBehaviour
{
    OrganismComponent organism_component;

    Cell cell;

    List<SlotComponent> slot_components= new List<SlotComponent>();

    public OrganismComponent OrganismComponent
    {
        get { return organism_component; }
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
            slot_components[i].gameObject.transform.Rotate(new Vector3(0, 0, i * -60));
        }
    }

    void ValidateSlots()
    {
        if (slot_components[0].Slot == Cell.GetSlot(0))
            return;

        for (int i = 0; i < 6; i++)
            slot_components[i].SetSlot(Cell.GetSlot(i));

        SetSlotTransformations();

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
