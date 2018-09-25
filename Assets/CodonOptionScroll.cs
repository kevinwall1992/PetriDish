using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CodonOptionScroll : ScrollRect, IBeginDragHandler, IEndDragHandler
{
    CodonSelector codon_selector;

    protected override void Awake()
    {
        codon_selector = GetComponentInParent<CodonSelector>();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        codon_selector.IsBackgroundVisible = true;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        codon_selector.IsBackgroundVisible = false;
    }
}
