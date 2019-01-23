using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CompoundTile : GoodBehavior
{
    [SerializeField]
    Text name_text, quantity_text;

    [SerializeField]
    Image image;

    Compound compound;

    int placeholder_index= -1;

    public Compound Compound
    {
        get { return compound; }

        set
        {
            compound = value;

            if (compound == null)
            {
                name_text.text = "";
                quantity_text.text = "";
                image.sprite = null;
            }
            else
            {
                if (compound.Molecule.Name == "Unnamed")
                    name_text.text = compound.Molecule.GetType().Name;
                else
                    name_text.text = compound.Molecule.Name;

                quantity_text.text = compound.Quantity.ToString("n1");
                image.sprite = CompoundComponent.GetSprite(compound.Molecule);
            }
        }
    }

    public float Size
    {
        get { return (transform as RectTransform).sizeDelta.x; }
        set { (transform as RectTransform).sizeDelta = new Vector2(value, 0); }
    }

    public CompoundGridPanel CompoundGridPanel { get; set; }


    private void Awake()
    {
        Compound = null;
    }

    void Start()
    {
        
    }

    void Update()
    {
        name_text.resizeTextMaxSize = (int)(22 * Size / 107.0f);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        if (CompoundGridPanel != null)
        {
            float quantity = Input.GetKey(KeyCode.LeftControl) ? compound.Quantity / 2 : 
                                                                 compound.Quantity;

            Compound = CompoundGridPanel.RemoveCompound(Compound.Molecule, quantity);

            transform.SetParent(Scene.Micro.Canvas.transform);
            CompoundGridPanel = null;
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        transform.position = Input.mousePosition;

        if (DetailPanel.Left != null && DetailPanel.Left is DNAPanel && Compound.Molecule is Catalyst)
        {
            DNAPanel dna_panel = DetailPanel.Left as DNAPanel;

            if (dna_panel.CodonLayout.IsPointedAt)
            {
                int insertion_index = dna_panel.CodonLayout.GetInsertionIndex();
                if (insertion_index < 0)
                    return;

                if (placeholder_index < 0)
                {
                    dna_panel.AddDNASequence("AAA", insertion_index);
                    dna_panel.CodonLayout.GetCodonElement(insertion_index).GetComponent<CanvasGroup>().alpha = 0;
                }
                else
                    dna_panel.CodonLayout.AddCodonElement(dna_panel.CodonLayout.RemoveCodonElement(placeholder_index), insertion_index);

                placeholder_index = insertion_index;
            }
            else if (placeholder_index >= 0)
            {
                Destroy(dna_panel.CodonLayout.RemoveCodonElement(placeholder_index).gameObject);
                placeholder_index = -1;
            }
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        if (DetailPanel.Left != null && DetailPanel.Left.IsPointedAt)
        {
            if (DetailPanel.Left is CompoundGridPanel)
                (DetailPanel.Left as CompoundGridPanel).AddCompound(Compound);
            else if (DetailPanel.Left is DNAPanel && Compound.Molecule is Catalyst)
            {
                DNAPanel dna_panel = DetailPanel.Left as DNAPanel;

                if (dna_panel.CodonLayout.IsPointedAt)
                {
                    string dna_sequence;
                    if (Compound.Molecule is Ribozyme)
                        dna_sequence = (Compound.Molecule as Ribozyme).Sequence;
                    else
                        dna_sequence = (Compound.Molecule as Enzyme).DNASequence;

                    Destroy(dna_panel.CodonLayout.RemoveCodonElement(placeholder_index).gameObject);

                    dna_panel.AddDNASequence("TAA" + dna_sequence + "TTT", placeholder_index);

                    placeholder_index = -1;
                }
            }
        }
        else if (DetailPanel.Right != null && DetailPanel.Right.IsPointedAt) ;
        else
        {
            OrganismComponent organism_component = Scene.Micro.Visualization.OrganismComponents[0];

            if (organism_component.IsPointedAt)
            {
                CellComponent cell_component = organism_component.CellComponentPointedAt;

                if (cell_component.PartPointedAt == CellComponent.Part.Cytozol)
                    organism_component.Organism.Cytozol.AddCompound(Compound);
                else
                {
                    Cell.Slot slot = cell_component.Cell.Slots[(int)cell_component.PartPointedAt];

                    if (slot.Compound == null)
                        slot.AddCompound(Compound);
                }
            }
        }

        Destroy(gameObject);
    }
}
