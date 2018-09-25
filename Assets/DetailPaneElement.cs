using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class DetailPaneElement : GoodBehavior, IDragHandler
{
    Text description;

    public string Description
    {
        get { return description.text; }
        set { description.text = value; }
    }

    public Color Color
    {
        get { return GetComponent<Image>().color; }
        set { GetComponent<Image>().color = value; }
    }

    public DetailPane DetailPane
    {
        get { return GetComponentInParent<DetailPane>(); }
    }

    protected virtual void Awake()
    {
        description = FindDescendent("description_panel").GetComponentInChildren<Text>();
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
    }
}
