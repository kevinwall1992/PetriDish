using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;


public class GoodBehavior : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    bool is_being_dragged = false;

    public bool IsBeingDragged
    {
        get { return is_being_dragged; }
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        is_being_dragged = true;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        is_being_dragged = false;
    }

    public GameObject FindDescendent(string name)
    {
        Queue<Transform> descendents= new Queue<Transform>();
        foreach (Transform child in transform)
            descendents.Enqueue(child);

        while (descendents.Count> 0)
        {
            Transform descendent = descendents.Dequeue();

            if (descendent.name == name)
                return descendent.gameObject;

            foreach (Transform child in descendent.transform)
                descendents.Enqueue(child);
        }

        return null;
    }

    public T FindDescendent<T>(string name) where T : MonoBehaviour
    {
        return FindDescendent(name).GetComponent<T>();
    }

    public GameObject FindAncestor(string name)
    {
        Transform ancestor = transform.parent;

        while(ancestor!= null)
        {
            if (ancestor.name == name)
                return ancestor.gameObject;

            ancestor = ancestor.parent;
        }

        return null;
    }

    public T FindAncestor<T>(string name) where T : MonoBehaviour
    {
        return FindAncestor(name).GetComponent<T>();
    }

    public bool IsHovered()
    {
        if (!(transform is RectTransform))
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, Input.mousePosition);
    }

    public bool HasComponent<T>() where T : MonoBehaviour
    {
        return GetComponent<T>() != null;
    }
}
