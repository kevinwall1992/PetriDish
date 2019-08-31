using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CatalystNode : DNAPanelNode
{
    [SerializeField]
    Image image;

    [SerializeField]
    Card card;

    [SerializeField]
    RectTransform card_container;

    public override bool IsCollapsed
    {
        get
        {
            return base.IsCollapsed;
        }

        set
        {
            base.IsCollapsed = value;

            if (!IsCollapsed)
            {
                card.Catalyst = Catalyst;
                card.gameObject.SetActive(true);
            }
            else
                card.gameObject.SetActive(false);
        }
    }

    public override int CodonLength
    {
        get
        {
            if (Catalyst is Ribozyme)
                return (Catalyst as Ribozyme).CodonCount;
            else
                return (Catalyst as Enzyme).DNASequence.Length / 3;
        }
    }

    public Catalyst Catalyst { get; private set; }

    protected override void Start()
    {
        base.Start();

        card.transform.SetParent(Scene.Micro.Canvas.transform);
    }

    protected override void Update()
    {
        base.Update();

        card.CollapsedSize = card_container.rect.width;
        card.RestPosition = card_container.position;
    }

    public static CatalystNode CreateInstance(Catalyst catalyst)
    {
        CatalystNode catalyst_node = Instantiate(Scene.Micro.Prefabs.CatalystNode);
        catalyst_node.Catalyst = catalyst;
        catalyst_node.image.sprite = Resources.Load<Sprite>(catalyst is Ribozyme ? "ribozyme" : "enzyme");

        return catalyst_node;
    }
}
