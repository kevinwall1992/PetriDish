using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class CatalystNode : DNAPanelNode
{
    [SerializeField]
    Image image;

    [SerializeField]
    Card card;

    [SerializeField]
    RectTransform card_container;

    List<Program.Token> tokens;

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

    public Catalyst Catalyst { get; private set; }

    public override IEnumerable<Program.Code> Codes
    {
        get { return tokens.ConvertAll((token) => ((Program.Code)token)); }
    }

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

    public static CatalystNode CreateInstance(IEnumerable<Program.Token> tokens)
    {
        CatalystNode catalyst_node = Instantiate(Scene.Micro.Prefabs.CatalystNode);
        catalyst_node.tokens = new List<Program.Token>(tokens);
        catalyst_node.Catalyst = Interpretase.GetCatalyst(new DNA(Program.TokensToDNASequence(tokens)), 0);
        catalyst_node.image.sprite = Resources.Load<Sprite>(catalyst_node.Catalyst is Ribozyme ? "ribozyme" : "protein");

        return catalyst_node;
    }
}
