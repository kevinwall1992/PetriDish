using UnityEngine;
using System.Collections;

//Hopefully one day Unity supports generic MonoBehaviours
public class CodonElementLayout : FloatingLayout
{
    public override void AddElement(GameObject element, int index = -1)
    {
        Debug.Assert(element.GetComponent<CodonElement>()!= null);
        if (element.GetComponent<CodonElement>() == null)
            return;

        base.AddElement(element, index);
    }
    
    public void AddCodonElement(CodonElement codon_element, int index = -1)
    {
        base.AddElement(codon_element.gameObject, index);
    }

    public CodonElement GetCodonElement(int index)
    {
        return base.GetElement(index).GetComponent<CodonElement>();
    }

    public CodonElement RemoveCodonElement(int index)
    {
        return base.RemoveElement(index).GetComponent<CodonElement>();
    }

    public CodonElement RemoveCodonElement(CodonElement codon_element)
    {
        return base.RemoveElement(codon_element.gameObject).GetComponent<CodonElement>();
    }

    public CodonElement ReplaceCodonElement(int index, CodonElement element)
    {
        return ReplaceElement(index, element.gameObject).GetComponent<CodonElement>();
    }

    public CodonElement ReplaceCodonElement(CodonElement original_element, CodonElement new_element)
    {
        return ReplaceElement(original_element.gameObject, new_element.gameObject).GetComponent<CodonElement>();
    }

    public bool Contains(CodonElement element)
    {
        return Contains(element.gameObject);
    }

    public void SetCodonElementOffset(int index, Vector2 offset)
    {
        SetElementOffset(index, offset);
    }

    public void SetCodonElementOffset(CodonElement codon_element, Vector2 offset)
    {
        SetElementOffset(codon_element.gameObject, offset);
    }
}
