using UnityEngine;
using System.Collections;

public class CardAttractor : GoodBehavior
{
    [SerializeField]
    Card card;

    public Card Card { get { return card; } }

    void Start()
    {
        card.transform.parent = Scene.Micro.Canvas.transform;
    }

    void Update()
    {
        card.CollapsedSize = (transform as RectTransform).rect.width;
        card.RestPosition = transform.position;
    }

    private void OnDisable()
    {
        card.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        card.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        Destroy(card.gameObject);
    }
}
