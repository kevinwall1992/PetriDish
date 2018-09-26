using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotComponent : MonoBehaviour
{
    Cell.Slot slot;

    CompoundComponent compound_component;
    CompoundComponent catalyst_compound_component;

    public Cell.Slot Slot
    {
        get { return slot; }
    }

    public CompoundComponent CompoundComponent
    {
        get { return compound_component; }
    }

    public CompoundComponent CatalystCompoundComponent
    {
        get { return catalyst_compound_component; }
    }

    public CellComponent CellComponent
    {
        get { return GetComponentInParent<CellComponent>(); }
    }

    public Vector2 Center
    {
        get { return compound_component.transform.position; }
    }

    private void Awake()
    {
        gameObject.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("slot");
    }

    void Start()
    {
        
    }

    void Update()
    {
        compound_component.SetCompound(Slot.Compound);
        catalyst_compound_component.SetCompound(Slot.CatalystCompound);
    }

    public SlotComponent SetSlot(Cell.Slot slot_)
    {
        slot = slot_;

        if(compound_component== null)
        {
            compound_component = new GameObject("compound").AddComponent<CompoundComponent>();
            compound_component.transform.parent = this.transform;
            compound_component.transform.localPosition = new Vector3(0, 1.5f, 0);

            catalyst_compound_component = new GameObject("catalyst").AddComponent<CompoundComponent>();
            catalyst_compound_component.transform.parent = this.transform;
            catalyst_compound_component.transform.localPosition = new Vector3(0, 1.5f, 0);
        }

        return this;
    }
}
