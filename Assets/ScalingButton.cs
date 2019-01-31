using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScalingButton : GoodBehavior, IPointerClickHandler
{
    [SerializeField]
    Color tint = Color.white;

    protected Color Tint
    {
        get { return tint; }
        set { tint = value; }
    }

    protected Color Color { get; set; }

    protected Image Image { get { return GetComponent<Image>(); } }

    protected virtual void Start()
    {
        Color = Image.color;
    }

    protected virtual void Update()
    {
        float scale = IsPointedAt ? 1.1f : 1;
        float speed = 15;

        transform.localScale = MathUtility.MakeUniformScale(Mathf.Lerp(transform.localScale.x, scale, speed * Time.deltaTime));

        if (IsPointedAt)
            Image.color = Color.Lerp(Color, Tint, 0.5f);
        else
            Image.color = Color;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        
    }
}
