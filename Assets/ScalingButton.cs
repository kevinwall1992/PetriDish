using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ScalingButton : GoodBehavior, IPointerClickHandler
{
    void Update()
    {
        float scale = IsPointedAt ? 1.1f : 1;
        float speed = 15;

        transform.localScale = MathUtility.MakeUniformScale(Mathf.Lerp(transform.localScale.x, scale, speed * Time.deltaTime));
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        
    }
}
