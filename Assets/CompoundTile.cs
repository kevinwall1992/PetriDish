using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CompoundTile : GoodBehavior
{
    [SerializeField]
    Text name_text, quantity_text;

    [SerializeField]
    Image image;

    [SerializeField]
    CanvasGroup canvas_group;

    Compound compound;

    int placeholder_index = -1;

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

                quantity_text.text = Measures.GetVisualQuantity(compound).ToString("n1");
                image.sprite = MoleculeComponent.GetSprite(compound.Molecule);
            }
        }
    }

    public float Size
    {
        get { return (transform as RectTransform).sizeDelta.x; }
        set { (transform as RectTransform).sizeDelta = new Vector2(value, 0); }
    }

    public CompoundGridPanel CompoundGridPanel
    {
        get { return GetComponentInParent<CompoundGridPanel>(); }
    }

    public TrashcanPanel TrashcanPanel
    {
        get { return GetComponentInParent<TrashcanPanel>(); }
    }

    public Hole Hole
    {
        get { return GetComponentInParent<Hole>(); }
    }


    private void Awake()
    {
        Compound = null;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

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
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        canvas_group.blocksRaycasts = false;

        if (IsPointedAt)
            transform.position = transform.position + (Vector3)eventData.delta;
        else
            transform.position = Input.mousePosition;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        canvas_group.blocksRaycasts = true;

        if (Hole != null)
            return;
        else if (DetailPanel.Left != null && DetailPanel.Left.IsPointedAt)
        {
            if (DetailPanel.Left is CompoundGridPanel)
                (DetailPanel.Left as CompoundGridPanel).AddCompound(Compound);
            else if (DetailPanel.Left is DNAPanel && Compound.Molecule is Catalyst)
            {
                DNAPanel dna_panel = DetailPanel.Left as DNAPanel;

                if (dna_panel.IsPointedAt && Compound.Molecule is Catalyst)
                {
                    SectorNode sector_node = dna_panel.SectorNode.GetDeepestVisibleSectorNode();

                    string dna_sequence;
                    if (compound.Molecule is Ribozyme)
                        dna_sequence = (compound.Molecule as Ribozyme).Sequence;
                    else
                        dna_sequence = (compound.Molecule as Protein).DNASequence;

                    sector_node.InsertCodes(Program.DNASequenceToTokens(dna_sequence).ConvertAll((token) => ((Program.Code)token)));//****Replace these things with Utility function
                }
            }
        }
        else if (DetailPanel.Right != null && DetailPanel.Right.IsTouched)
        {
            if (DetailPanel.Right is TrashcanPanel)
                Scene.Micro.TrashcanButton.Trashcan.ThrowAway(compound);
        }
        else if (Scene.Micro.TrashcanButton.IsPointedAt)
            Scene.Micro.TrashcanButton.Trashcan.ThrowAway(compound);
        else if (Scene.Micro.Visualization.OrganismComponent.IsPointedAt)
        {
            OrganismComponent organism_component = Scene.Micro.Visualization.OrganismComponent;

            CellComponent cell_component = organism_component.CellComponentPointedAt;

            if (cell_component.IsCytosolTouched)
                organism_component.Organism.Cytosol.AddCompound(Compound);
            else if(cell_component.SlotComponentTouched != null)
            {
                Cell.Slot slot = cell_component.SlotComponentTouched.Slot;

                if (slot.Compound == null)
                    slot.AddCompound(Compound);
            }
        }

        Scene.Micro.Editor.Do();

        Destroy(gameObject);
    }
}
