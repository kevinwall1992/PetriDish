using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;


public class GoodBehavior : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    bool is_being_dragged = false;

    public bool IsBeingDragged
    {
        get { return is_being_dragged; }
    }

    //This returns true if the mouse pointer is over the
    //screen space bounds of the object.
    public virtual bool IsPointedAt
    {
        get
        {
            if (transform is RectTransform)
                return RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, Input.mousePosition);

            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
                return collider.OverlapPoint(Scene.Micro.Camera.ScreenToWorldPoint(Input.mousePosition));

            throw new System.NotImplementedException();
        }
    }

    //Touched essentially means IsPointedAt && nothing is in the way
    public bool IsTouched { get; private set; }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        is_being_dragged = true;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        is_being_dragged = true;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        is_being_dragged = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        IsTouched = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsTouched = false;
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

    public bool HasComponent<T>() where T : MonoBehaviour
    {
        return GetComponent<T>() != null;
    }
}
