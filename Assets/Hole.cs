using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Hole : MonoBehaviour
{
    [SerializeField]
    RectMask2D mask;

    GameObject object_in_hole;

    Vector2 target_position;

    //distance between object and target_position
    //required to transition to second part of animation
    float transition_distance = 5.0f;

    float ObjectHeight
    {
        get { return (Object.transform as RectTransform).rect.height; }
    }

    bool ObjectIsBeingDragged
    {
        get
        {
            GoodBehavior good_behavior = Object.GetComponent<GoodBehavior>();

            return good_behavior == null || good_behavior.IsBeingDragged;
        }
    }

    public GameObject Object
    {
        get { return object_in_hole; }

        set
        {
            if (object_in_hole == value)
                return;

            object_in_hole = value;

            if (object_in_hole != null)
                target_position = new Vector2(transform.position.x,
                                              transform.position.y + ObjectHeight / 2 + transition_distance);
        }
    }

    public bool ObjectIsInsideHole
    {
        get
        {
            return Object.transform.parent == mask.transform &&
                   (Object.transform.position.y - ObjectHeight / 2) < transform.position.y;
        }
    }

    public bool ObjectHasSunk
    {
        get
        {
            return Object.transform.parent == mask.transform && 
                   (Object.transform.position.y + ObjectHeight / 2) < transform.position.y;
        }
    }

    private void Update()
    {
        if (Object == null)
            return;

        if (ObjectIsBeingDragged)
        {
            if (ObjectHasSunk)
                Sink();
            else if (ObjectIsInsideHole)
                Float();
        }

        float lerp_speed = Object.transform.parent == mask.transform ? 1.0f : 3.0f;

        if (!ObjectIsBeingDragged)
            Object.transform.position =
                new Vector3(Mathf.Lerp(Object.transform.position.x, target_position.x, Time.deltaTime * lerp_speed),
                            Mathf.Lerp(Object.transform.position.y, target_position.y, Time.deltaTime * lerp_speed));

        if (!ObjectIsInsideHole && 
            ((Vector2)Object.transform.position - target_position).magnitude < transition_distance)
        {
            Object.transform.SetParent(mask.transform);
            Float();
        }

        if (ObjectIsInsideHole)
            Object.transform.position = new Vector3(transform.position.x, Object.transform.position.y);
    }

    public GameObject RemoveObject()
    {
        GameObject removed_object = Object;
        Object = null;

        return removed_object;
    }

    public void Float()
    {
        target_position = transform.position;
    }

    public void Sink()
    {
        if (Object == null)
            return;

        target_position = transform.position - new Vector3(0, ObjectHeight / 2 + 5);
    }

    public void BubbleUp()
    {
        if (Object == null)
            return;

        Object.transform.SetParent(mask.transform);
        Object.transform.position = new Vector2(transform.position.x,
                                                      transform.position.y - ObjectHeight / 2);

        Float();
    }
}
