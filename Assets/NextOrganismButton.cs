using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class NextOrganismButton : ScalingButton
{
    [SerializeField]
    Color touch_color;

    [SerializeField]
    bool previous_organism_instead = false;

    protected override void Update()
    {
        if (IsTouched)
            Color = touch_color;
        else
            Color = new Color(1, 1, 1, 0.5f);

        base.Update();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        if (previous_organism_instead)
            Scene.Micro.Visualization.PreviousOrganism();
        else
            Scene.Micro.Visualization.NextOrganism();
    }
}
