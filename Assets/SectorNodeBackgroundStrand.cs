using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SectorNodeBackgroundStrand : MonoBehaviour
{
    [SerializeField]
    RectMask2D length_mask;

    [SerializeField]
    Image background_strand_image, return_strand_image;

    [SerializeField]
    Transform return_strand_zero_position;

    public float Length
    {
        get { return length_mask.rectTransform.sizeDelta.x; }

        set
        {
            length_mask.rectTransform.sizeDelta = new Vector2(value, length_mask.rectTransform.sizeDelta.y);
        }
    }

    float horizontal_offset;
    public float HorizontalOffset
    {
        get { return horizontal_offset; }

        set
        {
            horizontal_offset = value;

            float segment_width = (background_strand_image.transform as RectTransform).rect.width / 7;
            int segment_count = (int)(Mathf.Abs(horizontal_offset) / segment_width);

            float final_offset = horizontal_offset + segment_count * (horizontal_offset < 0 ? segment_width : -segment_width);
            background_strand_image.transform.position = transform.position + new Vector3(final_offset, 0);


            float return_horizontal_offset = horizontal_offset * 2;
            segment_count = (int)(Mathf.Abs(return_horizontal_offset) / segment_width);

            final_offset = return_horizontal_offset + segment_count * (return_horizontal_offset < 0 ? segment_width : -segment_width);
            return_strand_image.transform.localPosition = new Vector3(-final_offset, 0);
        }
    }

    void Start()
    {
        float strand_length = (transform.parent as RectTransform).rect.width * 7 / 5.0f;

        (background_strand_image.transform as RectTransform).sizeDelta = new Vector2(strand_length, 0);
        (return_strand_image.transform as RectTransform).sizeDelta = new Vector2(strand_length, 0);
    }

    void Update()
    {

    }

    public void ShowReturnStrand()
    {
        return_strand_image.gameObject.SetActive(true);
    }

    public void HideReturnStrand()
    {
        return_strand_image.gameObject.SetActive(false);
    }
}
