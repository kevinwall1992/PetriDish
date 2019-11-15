using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CatalystProgressIcon : GoodBehavior
{
    [SerializeField]
    Animator animator;

    public float Moment
    {
        get { return animator.GetFloat("moment"); }
        set { animator.SetFloat("moment", value); }
    }

    protected override void Update()
    {
        base.Update();
    }
}
