using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class DNASequenceElement : DNAPanelElement
{
    public string dna_sequence;

    int last_hover_index = -1;

    public string DNASequence
    {
        get { return dna_sequence; }
        set { dna_sequence = value; }
    }

    public int CodonCount
    {
        get { return dna_sequence.Length / 3; }
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        DNASequenceElement copy = Instantiate(this);
        copy.DNASequence = DNASequence;

        DNAPanel.SequenceLayout.ReplaceDNASequenceElement(this, copy);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
    
        if(DNAPanel.CodonLayout.IsPointedAt)
        {
            int current_hover_index= DNAPanel.CodonLayout.GetInsertionIndex();

            if (last_hover_index < 0)
            {
                DNAPanel.SpawnPosition = transform.position;
                DNAPanel.AddDNASequence(DNASequence, current_hover_index);

                //If we implement easing from spawning position
                //(as opposed to easing to target position only)
                //We should make this element fully transparent at this point
                transform.SetSiblingIndex(transform.parent.childCount);
                GetComponent<CanvasGroup>().alpha = 0.5f;
            }
            else if(current_hover_index!= last_hover_index)
            {
                for (int i = 0; i < CodonCount; i++)
                {
                    int old_index, new_index;

                    if (last_hover_index < current_hover_index)
                    {
                        old_index = last_hover_index;
                        new_index = current_hover_index + CodonCount - 1;
                    }
                    else
                    {
                        old_index = last_hover_index+ i;
                        new_index = current_hover_index+ i;
                    }

                    DNAPanel.CodonLayout.AddCodonElement(DNAPanel.CodonLayout.GetCodonElement(old_index), new_index);
                }
            }

            last_hover_index = current_hover_index;
        }
        else if(last_hover_index>= 0)
        {
            for (int i = 0; i < CodonCount; i++)
                GameObject.Destroy(DNAPanel.CodonLayout.RemoveCodonElement(last_hover_index).gameObject);

            GetComponent<CanvasGroup>().alpha = 1.0f;

            last_hover_index = -1;
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        GameObject.Destroy(this.gameObject);
    }
}
