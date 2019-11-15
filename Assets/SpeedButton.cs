using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class SpeedButton : ScalingButton
{
    [SerializeField]
    Color slow_color, fast_color, faster_color;

    enum Speed { Slow, Normal, Fast, Faster, Count }
    Speed speed = Speed.Normal;

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        speed = (Speed)(((int)speed + 1) % (int)Speed.Count);

        switch(speed)
        {
            case Speed.Slow:
                Scene.Micro.Visualization.Speed = 0.333f;
                Image.sprite = Resources.Load<Sprite>("slow_icon");
                Color = slow_color;
                break;

            case Speed.Normal:
                Scene.Micro.Visualization.Speed = 1.0f;
                Image.sprite = Resources.Load<Sprite>("fast_icon");
                Color = Color.white;
                break;

            case Speed.Fast:
                Scene.Micro.Visualization.Speed = 2.0f;
                Image.sprite = Resources.Load<Sprite>("fast_icon");
                Color = fast_color;
                break;

            case Speed.Faster:
                Scene.Micro.Visualization.Speed = 4.0f;
                Image.sprite = Resources.Load<Sprite>("faster_icon");
                Color = faster_color;
                break;
        }
    }
}
