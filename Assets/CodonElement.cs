using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CodonElement : DNAPanelElement
{
    public static CodonElement ActiveCodonElement { get; set; }

    [SerializeField]
    CodonSelector codon_selector;

    [SerializeField]
    Transform arrow_transform;

    public string Codon
    {
        get { return codon_selector.SelectedCodon; }
        set
        {
            codon_selector.CodonOptions= GetCodonOptions(value);
            codon_selector.SelectedCodon = value;
            UpdateTint();
        }
    }

    public Transform ArrowTransform { get { return arrow_transform; } }

    void Start()
    {

    }

    void Update()
    {
        if (!codon_selector.Validate())
            UpdateDescription();
    }

    List<string> GetCodonOptions(string codon)
    {
        List<string> command_codons = new List<string> { "CAA", "CAC", "CAG", "CAT", "CCC", "CGG", "CTG", "CTT" };
        List<string> marker_codons = new List<string> { "TAA", "TAC", "TAG", "TAT", "TCA", "TCC", "TCG", "TCT", "TGA", "TGC", "TGT", "TTA", "TTC", "TTG" };
        List<string> value_codons = new List<string> { "AAA", "AAC", "AAG", "AAT", "ACA", "ACC", "ACG", "ACT", "AGA", "AGC", "AGG", "AGT", "ATA", "ATC", "ATG", "ATT" };
        List<string> function_codons = new List<string> { "GAA", "GAC", "GAG", "GAT", "GCA", "GCC" };
        List<string> end_codon = new List<string> { "TTT" };

        List<string> codon_options = new List<string> { codon };

        switch (codon[0])
        {
            case 'A': codon_options = value_codons; break;

            case 'C': codon_options = command_codons; break;

            case 'G':
                switch (codon)
                {
                    case "GAA":
                    case "GAC":
                    case "GAG":
                    case "GAT": codon_options = function_codons; break;
                }

                break;

            case 'T':
                if (codon == "TTT")
                    codon_options = end_codon;
                else
                    codon_options = marker_codons;
                    
                break;
        }

        return codon_options;
    }

    void UpdateTint()
    {
        Color tint = Color.Lerp(Color.clear, Color.white, 240 / 255.0f);

        switch (Codon[0])
        {
            case 'C':
                tint = Color.Lerp(Color.Lerp(Color.yellow, Color.red, 0.15f), Color.white, 0.15f);
                break;

            case 'T':
                tint = Color.Lerp(Color.Lerp(Color.blue, Color.green, 0.2f), Color.white, 0.3f);

                break;
        }

                GetComponent<Image>().color= tint;
    }

    void UpdateDescription()
    {
        string codon = codon_selector.SelectedCodon;

        switch (codon[0])
        {
            case 'A':
                bool is_location = false;

                int this_codon_index = DNAPanel.CodonLayout.GetElementIndex(gameObject);

                string previous_codon = DNAPanel.CodonLayout.GetCodonElement(this_codon_index - 1).Codon;
                switch (previous_codon)
                {
                    case "CAA":
                    case "CAC":
                    case "CCC":
                    case "GAA":
                        is_location = true;
                        break;
                }

                if (is_location)
                {
                    if(previous_codon == "CCC")
                        switch(Interpretase.SpinCommand.CodonToDirection(codon))
                        {
                            case Interpretase.SpinCommand.Direction.Right: Description = "Right"; break;
                            case Interpretase.SpinCommand.Direction.Left: Description = "Left"; break;
                        }
                    else
                        switch (Interpretase.CodonToDirection(codon))
                        {
                            case Cell.Slot.Relation.Right: Description = "Right"; break;
                            case Cell.Slot.Relation.Left: Description = "Left"; break;
                            case Cell.Slot.Relation.Across: Description = "Across"; break;
                        }
                }
                else
                    Description = Interpretase.CodonToValue(codon).ToString();
               
                break;

            case 'C':
                switch (codon)
                {
                    case "CAA": Description = "Move"; break;
                    case "CAC": Description = "Take"; break;
                    case "CAG": Description = "Grab"; break;
                    case "CAT": Description = "Release"; break;
                    case "CCC": Description = "Spin"; break;
                    case "CGG": Description = "Excise"; break;
                    case "CTG": Description = "If"; break;
                    case "CTT": Description = "Try"; break;

                    default:
                        Description = "";
                        break;
                }
                break;

            case 'G':
                switch (codon)
                {
                    case "GAA": Description = "Get Size"; break;
                    case "GAC": Description = "Greater Than"; break;
                    case "GAG": Description = "Equal To"; break;
                    case "GAT": Description = "Less Than"; break;
                    case "GCA": Description = "Add"; break;
                    case "GCC": Description = "Subtract"; break;

                    default:
                        Description = "";
                        break;
                }
                break;

            case 'T':
                if(codon == "TTT")
                    Description = "End Marker";
                else
                    Description = "Marker " + (Interpretase.CodonToValue(codon) - Interpretase.CodonToValue("TAA")).ToString();

                break;
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        if (DNAPanel.CodonLayout.IsPointedAt)
            DNAPanel.CodonLayout.AddCodonElement(this, DNAPanel.CodonLayout.GetInsertionIndex());
        else if (DNAPanel.CodonLayout.Contains(this))
            DNAPanel.CodonLayout.RemoveCodonElement(this);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        if (!DNAPanel.CodonLayout.IsPointedAt)
            GameObject.Destroy(this.gameObject);
    }
}
