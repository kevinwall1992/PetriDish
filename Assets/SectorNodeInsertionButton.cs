using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SectorNodeInsertionButton : GoodBehavior, IPointerClickHandler, IPointerDownHandler
{
    [SerializeField]
    Image image, highlight;

    bool click_is_in_progress = false;

    public DNAPanelNode DNAPanelNode { get; set; }

    public bool IsHighlighted { get; set; }


    protected override void Update()
    {
        base.Update();

        float near_edge = DNAPanelNode.transform.position.x - (DNAPanelNode.transform as RectTransform).rect.width / 2;
        transform.position = new Vector3(near_edge - DNAPanelNode.SectorNode.Spacing / 2, DNAPanelNode.transform.position.y);

        image.gameObject.SetActive(GoodBehavior.DraggedElement == null && IsPointedAt);
        

        if (IsHighlighted)
        {
            highlight.gameObject.SetActive(true);
            (highlight.transform as RectTransform).sizeDelta = 
                new Vector2((DNAPanelNode.transform as RectTransform).rect.width + (transform as RectTransform).rect.width / 2, 0);
        }
        else
            highlight.gameObject.SetActive(false);


        if (click_is_in_progress && 
            !DNAPanelNode.SectorNode.IsSelecting && 
            Utility.GetMouseMotion().magnitude > 0)
            DNAPanelNode.SectorNode.BeginSelect(DNAPanelNode);

        if (Input.GetMouseButtonUp(0))
        {
            if (click_is_in_progress && IsPointedAt)
                DNAPanelNode.SectorNode.ShowInsertionChoice(DNAPanelNode);

            click_is_in_progress = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        click_is_in_progress = true;
    }
}
