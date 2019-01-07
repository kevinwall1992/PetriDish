using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CodonElement : DNAPanelElement
{
    [SerializeField]
    CodonSelector codon_selector;

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
        List<string> command_codons = new List<string> { "CAA", "CCC", "CGG", "CTT", "CAC", "CAG", "CAT", "CCA", "CCG" };
        List<string> marker_codons = new List<string> { "TCT", "TGA", "TGC", "TGT", "TTA", "TTC", "TTG" };
        List<string> location_codons = new List<string> { "TAA", "TAC", "TAG", "TAT", "TCA", "TCC", "TCG" };
        List<string> value_codons = new List<string> { "AAA", "AAC", "AAG", "AAT", "ACA", "ACC", "ACG", "ACT", "AGA", "AGC", "AGG", "AGT", "ATA", "ATC", "ATG", "ATT" };
        List<string> function_codons = new List<string> { "GAA", "GAC", "GAG", "GAT" };
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
                int value = Interpretase.CodonToValue(codon);

                if (value <= 54)
                    codon_options = location_codons;
                else if (value < 63)
                    codon_options = marker_codons;
                else
                    codon_options = end_codon;

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

                if (Interpretase.CodonToValue(Codon) <= 54)
                    tint = Color.Lerp(Color.Lerp(tint, Color.green, 0.2f), Color.white, 0.6f);

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
                int value = Interpretase.CodonToValue(codon);

                if (value < 6)
                    Description = "Slot " + value.ToString();
                else if (value == 6)
                    Description = "Across Slot";
                else if (value == 7)
                    Description = "Cytozol";
                else if (value == 7)
                    Description = "";

                break;

            case 'C':
                switch (codon)
                {
                    case "CAA": Description = "Move Single Unit"; break;
                    case "CCC": Description = "Move Half Stack"; break;
                    case "CGG": Description = "Move Full Stack"; break;
                    case "CTT": Description = "Move Max"; break;
                    case "CAC": Description = "Activate"; break;
                    case "CAG": Description = "Goto"; break;
                    case "CCG": Description = "Try"; break;
                    case "CAT": Description = "Cut"; break;
                    case "CCA": Description = "Swap"; break;
                    default: Description = ""; break;
                }
                break;

            case 'G':
                switch (codon)
                {
                    case "GAA": Description = "Get Size"; break;
                    case "GAC": Description = "Greater Than"; break;
                    case "GAG": Description = "Equal To"; break;
                    case "GAT": Description = "Less Than"; break;

                    default:
                        Description = "";
                        break;
                }
                break;

            case 'T':
                if(codon == "TTT")
                    Description = "End Marker";
                else
                    Description = "Marker " + (Interpretase.CodonToValue(codon) - Interpretase.CodonToValue("TCT")).ToString();

                break;
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        if (DNAPanel.CodonLayout.IsHovered())
            DNAPanel.CodonLayout.AddCodonElement(this, DNAPanel.CodonLayout.GetHoveredInsertionIndex());
        else if (DNAPanel.CodonLayout.Contains(this))
            DNAPanel.CodonLayout.RemoveCodonElement(this);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        if (!DNAPanel.CodonLayout.IsHovered())
            GameObject.Destroy(this.gameObject);
    }
}
