using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotComponent : MonoBehaviour
{
    Cell.Slot slot;

    CompoundComponent compound_component;
    CompoundComponent catalyst_compound_component;

    GameObject left_corner, right_corner;

    public Cell.Slot Slot
    {
        get { return slot; }
    }

    public GameObject LeftCorner
    {
        get { return left_corner; }
    }

    public GameObject RightCorner
    {
        get { return right_corner; }
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

        left_corner = new GameObject("left_corner");
        left_corner.transform.parent = transform;
        left_corner.transform.localPosition = new Vector3(-0.4f, 1.5f);

        right_corner = new GameObject("right_corner");
        right_corner.transform.parent = transform;
        right_corner.transform.localPosition = new Vector3(0.4f, 1.5f);
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
