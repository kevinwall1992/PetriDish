using UnityEngine;
using System.Collections;

public class AttachmentComponent : GoodBehavior
{
    [SerializeField]
    SpriteRenderer sprite_renderer;

    public Attachment Attachment { get; private set; }

    protected virtual void Update()
    {
        
    }

    public virtual AttachmentComponent SetAttachment(Attachment attachment, Color color)
    {
        Attachment = attachment;
        sprite_renderer.color = color;

        return this;
    }
}
