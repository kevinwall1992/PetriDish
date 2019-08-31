using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TextOption : GoodBehavior, Option<string>
{
    [SerializeField]
    Text text;

    [SerializeField]
    MonoBehaviour choice_behavior;

    public Choice<string> Choice { get { return choice_behavior as Choice<string>; } }

    public string Value
    {
        get { return text.text; }
        set { text.text = value; }
    }

    protected override void Update()
    {
        base.Update();

        if(Input.GetMouseButtonUp(0) && IsPointedAt)
            Choice.Selection = this;
    }
}
