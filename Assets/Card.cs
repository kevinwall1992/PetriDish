using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Card : GoodBehavior, IPointerClickHandler
{
    static List<Card> cards = new List<Card>();

    public float CollapsedSize { get; set; }
    public float RestScale { get { return CollapsedSize / (transform as RectTransform).rect.width; } }
    public Vector2 RestPosition { get; set; }

    float target_scale;
    Vector2 target_position;

    int original_font_size = -1;

    bool is_hovered = false;

    enum ZoomTarget { None, Card, Image, Data }
    ZoomTarget current_zoom_target = ZoomTarget.None;
    ZoomTarget CurrentZoomTarget
    {
        get { return current_zoom_target; }

        set
        {
            if (current_zoom_target == value)
                return;

            current_zoom_target = value;

            if(current_zoom_target != ZoomTarget.Image)
                Scene.ExampleComponent.Example = null;//really necessary?

            if (current_zoom_target == ZoomTarget.None)
                GUI.depth = 0;

            zoom_progress = 0;
        }
    }

    float zoom_length = 2.5f;
    float zoom_progress = 0;

    Catalyst catalyst;
    public Catalyst Catalyst
    {
        get { return catalyst; }

        set
        {
            catalyst = value;

            name_text.text = catalyst.Name;
            description_small.text = description.text = catalyst.Description;
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
        if(CurrentZoomTarget == ZoomTarget.None)
        {
            target_scale = RestScale;
            target_position = RestPosition;
        }

        zoom_progress = Mathf.Min(zoom_progress + Time.deltaTime / zoom_length, 1);

        transform.localScale = new Vector3(Mathf.Lerp(transform.localScale.x, target_scale, zoom_progress),
                                           Mathf.Lerp(transform.localScale.y, target_scale, zoom_progress));

        if(transform.localScale.x < 0.25f)
        {
            description_small.gameObject.SetActive(true);
            description.gameObject.SetActive(false);
        }
        else
        {
            description_small.gameObject.SetActive(false);
            description.gameObject.SetActive(true);
        }

        transform.position = new Vector3(Mathf.Lerp(transform.position.x, target_position.x, zoom_progress), 
                                         Mathf.Lerp(transform.position.y, target_position.y, zoom_progress));

        if(CurrentZoomTarget == ZoomTarget.Image)
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

    private void OnGUI()
    {
        if (CurrentZoomTarget != ZoomTarget.None)
        {
            GUI.depth = -1;

            if (Utility.ConsumeIsKeyUp(KeyCode.Escape))
                CurrentZoomTarget = ZoomTarget.None;
        }
        else
            GUI.depth = 0;
    }

    private void OnEnable()
    {
        cards.Add(this);
    }

    private void OnDisable()
    {
        cards.Remove(this);
    }

    void CalculateTargets()
    {
        RectTransform zoomed_rect_transform;

        RectTransform canvas_transform = FindObjectOfType<Canvas>().transform as RectTransform;

        if (RectTransformUtility.RectangleContainsScreenPoint(data_panel.transform as RectTransform, Input.mousePosition))
        {
            zoomed_rect_transform = data_panel.transform as RectTransform;
            CurrentZoomTarget = ZoomTarget.Data;
        }
        else if (RectTransformUtility.RectangleContainsScreenPoint(image_panel, Input.mousePosition))
        {
            zoomed_rect_transform = image_panel.transform as RectTransform;
            CurrentZoomTarget = ZoomTarget.Image;

            if (Catalyst.Example != null)
                Scene.ExampleComponent.Example = Catalyst.Example;
        }
        else
        {
            if(CurrentZoomTarget == ZoomTarget.Card)
            {
                CurrentZoomTarget = ZoomTarget.None;
                return;
            }

            zoomed_rect_transform = card_zoom_transform as RectTransform;
            CurrentZoomTarget = ZoomTarget.Card;
        }

        target_scale = transform.localScale.x * 0.9f * canvas_transform.rect.height / (zoomed_rect_transform.rect.width * transform.localScale.x);

        Vector2 prior_scale = transform.localScale;
        transform.localScale = new Vector3(target_scale, target_scale);
        target_position = canvas_transform.position + transform.position - zoomed_rect_transform.position;
        transform.localScale = prior_scale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.SetAsLastSibling();

        if (Catalyst != null)
        {
            CalculateTargets();

            foreach (Card card in cards)
                if (card != this)
                    card.CurrentZoomTarget = ZoomTarget.None;
        }
    }


    [SerializeField]
    CardDataPanel data_panel;

    [SerializeField]
    RectTransform image_panel;

    [SerializeField]
    RectTransform card_zoom_transform;

    [SerializeField]
    RawImage card_scene_image;

    [SerializeField]
    Image compound_image;

    [SerializeField]
    Image blur;

    [SerializeField]
    Text name_text, price_text, description, description_small, code_text;
}
