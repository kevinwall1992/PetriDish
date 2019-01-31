using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayPauseButton : ScalingButton
{
    [SerializeField]
    StepButton step_button;

    [SerializeField]
    Color play_color;

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

            Image.sprite = Resources.Load<Sprite>(is_playing ? "play_icon" : "pause_icon");
            Color = is_playing ? play_color : Color.white;

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

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (step_button.IsStepping)
                Scene.Micro.Visualization.TakeOneStep();
            else
                IsPlaying = !IsPlaying;
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        IsPlaying = !IsPlaying;
    }
}
