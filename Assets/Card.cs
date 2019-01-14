using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : GoodBehavior, IPointerClickHandler
{
    [SerializeField]
    CardDataPanel data_panel;

    [SerializeField]
    RectTransform image_panel;

    [SerializeField]
    RawImage card_scene_image;

    [SerializeField]
    Image compound_image;

    [SerializeField]
    Image blur;

    [SerializeField]
    Text name_text, price_text, description, code_text;

    Vector2 original_scale;

    Vector2 target_scale;
    Vector2 target_position;

    int original_font_size = -1;

    bool is_hovered = false;

    enum ZoomTarget { None, Image, Data }
    ZoomTarget zoom_target = ZoomTarget.None;

    float zoom_length = 2.5f;
    float zoom_progress = 0;

    RectTransform RectTransform { get { return transform as RectTransform; } }

    float collapsed_size;
    public float CollapsedSize
    {
        get { return collapsed_size; }

        set
        {
            if (collapsed_size == value)
                return;

            collapsed_size = value;

            float scale = collapsed_size / RectTransform.rect.height;

            original_scale = transform.localScale = new Vector2(scale, scale);

            ResetTargets();
        }
    }

    Catalyst catalyst;
    public Catalyst Catalyst
    {
        get { return catalyst; }

        set
        {
            catalyst = value;

            name_text.text = catalyst.Name;
            description.text = catalyst.Description;
            price_text.text = catalyst.Price.ToString();

            Color dark_gray = Color.Lerp(Color.black, Color.gray, 0.2f);

            string code = "";
            if(catalyst is Ribozyme)
            {
                Ribozyme ribozyme = catalyst as Ribozyme;

                for (int i = 0; i < ribozyme.CodonCount; i++)
                    code += ribozyme.GetCodon(i) + " ";

                GetComponent<Image>().sprite = Resources.Load<Sprite>("card_ribozyme");
                code_text.color = Color.Lerp(Color.green, dark_gray, 0.8f);
            }
            else if(catalyst is Enzyme)
            {
                Enzyme enzyme = catalyst as Enzyme;

                foreach (Polymer.Monomer monomer in enzyme.Monomers)
                    code += (monomer as AminoAcid).Abbreviation + " ";

                GetComponent<Image>().sprite = Resources.Load<Sprite>("card_enzyme");
                code_text.color = Color.Lerp(Color.Lerp(Color.yellow, Color.red, 0.5f), dark_gray, 0.8f);
            }

            code_text.text = code.Trim(' ');
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        zoom_progress = Mathf.Min(zoom_progress + Time.deltaTime / zoom_length, 1);

        transform.localScale = new Vector3(Mathf.Lerp(transform.localScale.x, target_scale.x, zoom_progress),
                                           Mathf.Lerp(transform.localScale.y, target_scale.y, zoom_progress));

        transform.position = new Vector3(Mathf.Lerp(transform.position.x, target_position.x, zoom_progress), 
                                         Mathf.Lerp(transform.position.y, target_position.y, zoom_progress));

        if(zoom_target == ZoomTarget.Image)
        {
            compound_image.color = Color.Lerp(compound_image.color, Color.clear, zoom_progress);
            card_scene_image.color = Color.Lerp(card_scene_image.color, Color.white, zoom_progress);
        }
        else
        {
            compound_image.color = Color.Lerp(compound_image.color, Color.white, 0.1f);
            card_scene_image.color = Color.Lerp(card_scene_image.color, Color.clear, 0.1f);
        }

        (blur.gameObject.transform as RectTransform).sizeDelta = 
            new Vector2(
                Mathf.Min(name_text.rectTransform.rect.width - 
                          (name_text.transform.parent.transform as RectTransform).rect.width +
                          140, 0), 
                0.0f);
    }

    void CalculateTargets()
    {
        RectTransform zoomed_rect_transform;

        RectTransform canvas_transform = FindObjectOfType<Canvas>().transform as RectTransform;

        if (RectTransformUtility.RectangleContainsScreenPoint(data_panel.transform as RectTransform, Input.mousePosition))
        {
            zoomed_rect_transform = data_panel.transform as RectTransform;
            zoom_target = ZoomTarget.Data;
        }
        else if (RectTransformUtility.RectangleContainsScreenPoint(image_panel, Input.mousePosition))
        {
            zoomed_rect_transform = image_panel.transform as RectTransform;
            zoom_target = ZoomTarget.Image;

            if (Catalyst.Example != null)
                Scene.ExampleComponent.Example = Catalyst.Example;
        }
        else
        {
            ResetTargets();
            return;
        }

        target_scale = transform.localScale * 0.9f * canvas_transform.rect.height / (zoomed_rect_transform.rect.height * transform.localScale.y);

        Vector2 prior_scale = transform.localScale;
        transform.localScale = target_scale;
        target_position = canvas_transform.position + transform.position - zoomed_rect_transform.position;
        transform.localScale = prior_scale;

        zoom_progress = 0;
    }

    void ResetTargets()
    {
        target_scale = original_scale;

        RectTransform parent_rect_transform = transform.parent as RectTransform;
        target_position = (Vector2)parent_rect_transform.position +
                          new Vector2(parent_rect_transform.rect.min.x, parent_rect_transform.rect.max.y);

        zoom_target = ZoomTarget.None;

        zoom_progress = 0;

        Scene.ExampleComponent.Example = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Catalyst != null)
            CalculateTargets();
    }
}
