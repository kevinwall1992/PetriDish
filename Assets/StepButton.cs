using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class StepButton : ScalingButton
{
    [SerializeField]
    Color step_color;

    bool is_stepping = false;

    public bool IsStepping
    {
        get { return is_stepping; }

        set
        {
            if (is_stepping == value)
                return;

            is_stepping = value;

            Color = is_stepping ? step_color : Color.white;
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        if (IsStepping)
            Scene.Micro.Visualization.TakeOneStep();
        else
            Scene.Micro.Visualization.IsPaused = true;

        IsStepping = true;
    }
}
