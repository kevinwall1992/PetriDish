using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CardDataPanel : MonoBehaviour
{
    [SerializeField]
    VerticalLayoutGroup vertical_layout_group;

    float element_scale = 1;

    public float ElementScale
    {
        get { return element_scale; }

        set
        {
            float ratio = value / element_scale;
            element_scale = value;

            foreach (Transform child_transform in vertical_layout_group.transform)
            {
                RectTransform rect_transform = child_transform as RectTransform;
                rect_transform.sizeDelta = rect_transform.sizeDelta * ratio;
            }
        }
    }

    void Start()
    {

    }

    void Update()
    {

    }
}
