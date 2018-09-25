using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;


public class DNASequenceElementLayout : FloatingLayout
{
    public override void AddElement(GameObject element, int index = -1)
    {
        Debug.Assert(element.GetComponent<CodonElement>() != null);
        if (element.GetComponent<CodonElement>() == null)
            return;

        base.AddElement(element, index);
    }

    public void AddDNASequenceElement(DNASequenceElement dna_sequence_element, int index = -1)
    {
        base.AddElement(dna_sequence_element.gameObject, index);
    }

    public DNASequenceElement GetDNASequenceElement(int index)
    {
        return base.GetElement(index).GetComponent<DNASequenceElement>();
    }

    public DNASequenceElement RemoveDNASequenceElement(int index)
    {
        return base.RemoveElement(index).GetComponent<DNASequenceElement>();
    }

    public DNASequenceElement RemoveDNASequenceElement(DNASequenceElement dna_sequence_element)
    {
        return base.RemoveElement(dna_sequence_element.gameObject).GetComponent<DNASequenceElement>();
    }

    public DNASequenceElement ReplaceDNASequenceElement(int index, DNASequenceElement element)
    {
        return ReplaceElement(index, element.gameObject).GetComponent<DNASequenceElement>();
    }

    public DNASequenceElement ReplaceDNASequenceElement(DNASequenceElement original_element, DNASequenceElement new_element)
    {
        return ReplaceElement(original_element.gameObject, new_element.gameObject).GetComponent<DNASequenceElement>();
    }

    public bool Contains(DNASequenceElement element)
    {
        return Contains(element.gameObject);
    }

    public void SetDNASequenceElementOffset(int index, Vector2 offset)
    {
        SetElementOffset(index, offset);
    }

    public void SetDNASequenceElementOffset(DNASequenceElement dna_sequence_element, Vector2 offset)
    {
        SetElementOffset(dna_sequence_element.gameObject, offset);
    }
}
