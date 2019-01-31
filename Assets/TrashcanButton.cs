using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class TrashcanButton : ScalingButton
{
    TrashcanPanel trashcan_panel;
    TrashcanPanel TrashcanPanel
    {
        get
        {
            if (trashcan_panel == null)
                trashcan_panel = TrashcanPanel.Create(Trashcan);

            return trashcan_panel;
        }
    }

    public Trashcan Trashcan { get; private set; }

    protected override void Start()
    {
        base.Start();

        Trashcan = new Trashcan();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        if (TrashcanPanel.IsOpen)
            TrashcanPanel.Close();
        else
            TrashcanPanel.Open();
    }
}
