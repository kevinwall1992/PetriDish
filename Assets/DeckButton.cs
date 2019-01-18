using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class DeckButton : GoodBehavior, IPointerClickHandler
{
    void Update()
    {
        float scale = IsTouched ? 1.1f : 1;
        float speed = 15;

        transform.localScale = MathUtility.MakeUniformScale(Mathf.Lerp(transform.localScale.x, scale, speed * Time.deltaTime));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        DetailPanel deck_panel = Scene.Micro.Visualization.OrganismComponents[0].DeckDetailPanel;

        if (deck_panel.IsOpen)
            deck_panel.Close();
        else
            deck_panel.Open();
    }
}
