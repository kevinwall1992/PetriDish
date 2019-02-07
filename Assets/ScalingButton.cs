using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScalingButton : GoodBehavior, IPointerClickHandler
{
    [SerializeField]
    Color tint = Color.white;

    public bool IsInteractable { get; protected set; }

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

        IsInteractable = true;
    }

    protected virtual void Update()
    {
        if (!IsInteractable)
        {
            Image.color = new Color(Color.r, Color.g, Color.b, Color.a / 2);
            return;
        }

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
