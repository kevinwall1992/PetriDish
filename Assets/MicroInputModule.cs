using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class MicroInputModule : StandaloneInputModule
{
    GameObject drag_donor, drag_recipient;

    protected override void ProcessDrag(PointerEventData pointerEvent)
    {
        if (pointerEvent.dragging && pointerEvent.pointerDrag == drag_donor)
            pointerEvent.pointerDrag = drag_recipient;

        base.ProcessDrag(pointerEvent);
    }

    public void SwapDrag(GameObject drag_donor_, GameObject drag_recipient_)
    {
        drag_donor = drag_donor_;
        drag_recipient = drag_recipient_;
    }
}
