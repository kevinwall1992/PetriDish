using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DeckPanel : GridPanel
{
    [SerializeField]
    CardAttractor card_attractor_prefab;

    Deck deck;

    Organism Organism { get { return Data as Organism; } }

    protected override void Start()
    {
        base.Start();

        RowLength = 4;
    }

    protected override void Update()
    {
        base.Update();

        if (!Organism.Deck.Equals(deck))
            UpdateCards();
    }

    void UpdateCards()
    {
        foreach (Transform child_transform in GridLayoutGroup.transform)
            Destroy(child_transform.gameObject);

        deck = Organism.Deck;

        foreach(Catalyst catalyst in deck)
        {
            CardAttractor card_attractor = Instantiate(card_attractor_prefab);
            card_attractor.transform.parent = GridLayoutGroup.transform;

            card_attractor.Card.Catalyst = catalyst;
        }
    }

    public static DeckPanel Create(Organism organism)
    {
        DeckPanel deck_panel = Instantiate(Scene.Micro.Prefabs.DeckPanel);
        deck_panel.transform.SetParent(Scene.Micro.Canvas.transform, false);

        deck_panel.Data = organism;

        return deck_panel;
    }
}
