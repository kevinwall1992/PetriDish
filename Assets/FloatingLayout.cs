using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Floater : GoodBehavior
{
    GameObject element;

    Vector3 last_local_position;
    bool lerping = true;

    public Vector2 Offset
    {
        get;
        set;
    }

    public GameObject Element
    {
        get { return element; }
        set
        {
            element = value;
            element.transform.parent = FloatingLayout.FloatingElementsTransform;

            (transform as RectTransform).sizeDelta = (element.transform as RectTransform).sizeDelta;
        }
    }

    public FloatingLayout FloatingLayout
    {
        get { return GetComponentInParent<FloatingLayout>(); }
    }

    private void Awake()
    {
        gameObject.AddComponent<RectTransform>();
    }

    private void Update()
    {
        GoodBehavior good_behavior = Element.GetComponent<GoodBehavior>();
        if (good_behavior!= null && good_behavior.IsBeingDragged)
            return;

        if (last_local_position != transform.localPosition)
            lerping = true;
        last_local_position = transform.localPosition;

        Vector3 target_position = transform.position + new Vector3(Offset.x, Offset.y);

        if (lerping)
        {
            Element.transform.position = Vector3.Lerp(Element.transform.position, target_position, Time.deltaTime * 6);

            if (Element.transform.position.x < (target_position.x + 0.1f) && Element.transform.position.x > (target_position.x - 0.1f) &&
               Element.transform.position.y < (target_position.y + 0.1f) && Element.transform.position.y > (target_position.y - 0.1f))
                lerping = false;
        }
        else
            Element.transform.position = target_position;
    }

    private void OnDestroy()
    {
        
    }
}

public class FloatingLayout : GoodBehavior
{
    LayoutGroup layout_group;

    bool elements_changed = true;

    public Transform FloatingElementsTransform
    {
        get { return this.FindDescendent("floating_elements").transform; }
    }

    public int ElementCount
    {
        get { return layout_group.transform.childCount; }
    }

    private void Awake()
    {
        layout_group = GetComponentInChildren<LayoutGroup>();
    }

    Floater GetFloater(int index)
    {
        return layout_group.transform.GetChild(index).GetComponent<Floater>();
    }

    Floater GetFloater(GameObject element)
    {
        return GetFloater(GetElementIndex(element));
    }

    int GetElementIndex(GameObject element)
    {
        foreach (Floater floater in GetComponentsInChildren<Floater>())
            if (floater.Element == element)
                return floater.transform.GetSiblingIndex();

        return -1;
    }

    public virtual void AddElement(GameObject element, int index = -1)
    {
        if (Contains(element))
        {
            if (index >= 0)
            {
                int current_index = GetElementIndex(element);

                if (current_index == index)
                    return;
            }

            RemoveElement(element);
        }

        element.gameObject.SetActive(true);

        Floater floater = new GameObject("floating element").AddComponent<Floater>();
        floater.transform.parent = layout_group.transform;
        if (index >= 0)
            floater.transform.SetSiblingIndex(index);

        floater.Element= element;

        elements_changed = true;

    }

    public GameObject GetElement(int index)
    {
        return GetFloater(index).Element;
    }

    public GameObject RemoveElement(int index)
    {
        Floater floater = GetFloater(index);
        GameObject element = floater.Element;

        floater.transform.parent = null;
        GameObject.Destroy(floater.gameObject);

        elements_changed = true;
        
        return element;
    }

    public GameObject RemoveElement(GameObject element)
    {
        return RemoveElement(GetElementIndex(element));
    }

    public GameObject ReplaceElement(int index, GameObject element)
    {
        Floater floater = GetFloater(index);
        GameObject original_element = floater.Element;
        floater.Element = element;

        elements_changed = true;

        return original_element;
    }

    public GameObject ReplaceElement(GameObject original_element, GameObject new_element)
    {
        return ReplaceElement(GetElementIndex(original_element), new_element);
    }

    public void Clear()
    {
        foreach (Floater floater in GetComponentsInChildren<Floater>())
            GameObject.Destroy(floater.gameObject);

        elements_changed = true;
    }

    public bool Contains(GameObject element)
    {
        return GetElementIndex(element) >= 0;
    }

    public int GetHoveredInsertionIndex()
    {
        for (int index = 0; index < layout_group.transform.childCount; index++)
        {
            Floater floater = GetFloater(index);
            
            if (index == 0 && Input.mousePosition.y > (floater.transform as RectTransform).position.y)
                return index;

            if (index == (layout_group.transform.childCount - 1) && Input.mousePosition.y < (floater.transform as RectTransform).position.y)
                return index + 1;

            Floater next_floater = GetFloater(index + 1);

            if (Mathf.Abs(Input.mousePosition.y - (floater.transform as RectTransform).position.y) <
                    Mathf.Abs(Input.mousePosition.y - (next_floater.transform as RectTransform).position.y))
                return index;
        }

        return -1;
    }

    public void SetElementOffset(int index, Vector2 offset)
    {
        GetFloater(index).Offset = offset;
    }

    public void SetElementOffset(GameObject element, Vector2 offset)
    {
        SetElementOffset(GetElementIndex(element), offset);
    }

    public bool Validate()
    {
        if(elements_changed)
        {
            elements_changed = false;
            return false;
        }

        return true;
    }
}