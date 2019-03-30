using UnityEngine;
using System.Collections;

public class GrabberComponent : AttachmentComponent
{
    bool is_grabbing_visually = false;

    [SerializeField]
    Animator animator;

    Grabber Grabber { get { return Attachment as Grabber; } }

    protected override void Update()
    {
        base.Update();

        if (Grabber != null && is_grabbing_visually != Grabber.IsGrabbing)
        {
            animator.SetFloat("moment", Grabber.IsGrabbing ? 1 : 0);
            is_grabbing_visually = Grabber.IsGrabbing;
        }
    }

    public override AttachmentComponent SetAttachment(Attachment attachment, Color color)
    {
        Debug.Assert(attachment is Grabber);

        return base.SetAttachment(attachment, color);
    }
}
