using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayPauseButton : ScalingButton
{
    [SerializeField]
    StepButton step_button;

    bool is_playing = true;

    public bool IsPlaying
    {
        get { return is_playing; }

        set
        {
            if (is_playing == value)
                return;

            is_playing = value;

            if (is_playing == true)
                step_button.IsStepping = false;

            Image.sprite = Resources.Load<Sprite>(is_playing ? "pause_icon" : "play_icon");

            Scene.Micro.Visualization.IsPaused = IsPaused;
        }
    }

    public bool IsPaused
    {
        get { return !IsPlaying; }
        set { IsPlaying = !value; }
    }

    protected override void Update()
    {
        base.Update();

        IsPaused = Scene.Micro.Visualization.IsPaused;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        IsPlaying = !IsPlaying;
    }
}
