using UnityEngine;
using System.Collections;

public class CatalystPanel : DetailPanel
{
    [SerializeField]
    Card card;

    SlotComponent SlotComponent { get { return Data as SlotComponent; } }

    Catalyst Catalyst
    {
        get
        {
            return Data as Catalyst;
        }
    }

    public override object Data
    {
        set
        {
            base.Data = value;

            card.Catalyst = Catalyst;
        }
    }

    void Start()
    {
        
    }

    protected override void Update()
    {
        card.CollapsedSize = (transform as RectTransform).rect.width;

        base.Update();
    }

    public static CatalystPanel Create(Catalyst catalyst)
    {
        CatalystPanel catalyst_panel = Instantiate(Scene.Micro.Prefabs.CatalystPanel);
        catalyst_panel.transform.SetParent(Scene.Micro.Canvas.transform, false);

        catalyst_panel.Data = catalyst;

        return catalyst_panel;
    }
}
