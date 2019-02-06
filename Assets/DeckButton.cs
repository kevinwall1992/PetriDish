using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class DeckButton : ScalingButton
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        DetailPanel deck_panel = Scene.Micro.Visualization.OrganismComponent.DeckDetailPanel;

        if (deck_panel.IsOpen)
            deck_panel.Close();
        else
            deck_panel.Open();
    }
}
