using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class DNAPanelElement : GoodBehavior, IDragHandler
{
    [SerializeField]
    public Text description;

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

    public DNAPanel DNAPanel
    {
        get { return GetComponentInParent<DNAPanel>(); }
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
