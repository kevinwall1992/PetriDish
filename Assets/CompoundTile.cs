using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CompoundTile : GoodBehavior
{
    Text NameText { get { return FindDescendent<Text>("name_text"); } }
    Text QuantityText { get { return FindDescendent<Text>("quantity_text"); } }
    Image Image { get { return FindDescendent<Image>("image"); } }

    Compound compound;

    public Compound Compound
    {
        get { return compound; }

        set
        {
            compound = value;

            if (compound == null)
            {
                NameText.text = "";
                QuantityText.text = "";
                Image.sprite = null;
            }
            else
            {
                NameText.text = compound.Molecule.Name;
                QuantityText.text = compound.Quantity.ToString("n1");
                Image.sprite = CompoundComponent.GetSprite(compound.Molecule);
            }
        }
    }

    private void Awake()
    {
        Compound = null;
    }

    void Start()
    {
        
    }

    void Update()
    {

    }
}
