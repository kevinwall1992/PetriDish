using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CommandNodeElement : GoodBehavior
{
    [SerializeField]
    Image codon_background;

    [SerializeField]
    Text codon_text;

    [SerializeField]
    Image rung0, rung1, rung2;

    [SerializeField]
    Text rung0_text, rung1_text, rung2_text;

    [SerializeField]
    RectTransform primary_codon_element;

    [SerializeField]
    RectTransform operand_container;

    int codon_index_offset;

    float scroll_value = 0;

    public CommandNode CommandNode { get { return GetComponentInParent<CommandNode>(); } }

    public string Codon
    {
        get { return rung0_text.text + rung1_text.text + rung2_text.text; }

        set
        {
            if (Codon == value)
                return;

            DNA dna = CommandNode.SectorNode.Sector.DNA;
            int codon_index = CodonIndex;
            dna.RemoveSequence(codon_index, CodonLength);

            string operands = "";
            switch(value)
            {
                case "CVF": 
                case "CVL": 
                case "FVV": break;

                case "CVV":
                case "CCC": operands = "VVV"; break;

                case "CVC":
                case "FVC":
                case "FVL":
                case "FVF":
                case "FCV":
                case "FCC": operands = "VVVVVV"; break;

                case "CFF": operands = "LVVLVV"; break;

                case "CLL": operands = "LVVCVFVVV"; break;
                     
                case "CLF": operands = "LVVVVV"; break; 
            }
            dna.InsertSequence(codon_index, value + operands);
            Scene.Micro.Editor.Do();

            CommandNode.UpdateCommandIcon();

            UpdateCodon();
        }
    }

    public int CodonIndex { get { return CommandNode.CodonIndex + codon_index_offset; } }

    public int CodonLength
    {
        get
        {
            switch (Codon[0])
            {
                case 'C':
                case 'F': return operand_container.childCount + 1;

                default: return 1;
            }
        }
    }

    public string DNASequence
    {
        get
        {
            string dna_sequence = Codon;

            foreach (CommandNodeElement element in operand_container.GetComponentsInChildren<CommandNodeElement>())
                dna_sequence += element.DNASequence;

            return dna_sequence;
        }
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        UpdateLayout();

        if (Utility.IsPointedAt(codon_background))
        {
            scroll_value += Input.mouseScrollDelta.y;

            if ((int)scroll_value != 0)
            {
                string[] command_codons = { "CVV", "CVC", "CVF", "CVL", "CCC", "CFF", "CLL", "CLF" };

                string[] value_and_function_codons = { "VVV", "VVC", "VVF", "VVL",
                                                       "VCV", "VCC", "VCF", "VCL",
                                                       "VFV", "VFC", "VFF", "VFL",
                                                       "VLV", "VLC", "VLF", "VLL",
                                                       "FVV", "FVC", "FVL", "FVF", "FCV", "FCC" };

                string[] locus_codons = { "LVV", "LVC", "LVF", "LVL",
                                          "LCV", "LCC", "LCF", "LCL",
                                          "LFV", "LFC", "LFF", "LFL",
                                          "LLV", "LLC", "LLF", "LLL" };

                string[] relevant_codon_array = null;
                switch (Codon[0])
                {
                    case 'C': relevant_codon_array = command_codons; break;
                    case 'V': 
                    case 'F': relevant_codon_array = value_and_function_codons; break;
                    case 'L': relevant_codon_array = locus_codons; break;
                }

                int index = System.Array.IndexOf(relevant_codon_array, Codon);
                Codon = relevant_codon_array[MathUtility.Mod(index + (int)scroll_value, relevant_codon_array.Length)];

                scroll_value -= (int)scroll_value;
            }
        }

        //if (Codon != GetLogicalCodon())
        //    UpdateCodon();
    }

    void UpdateLayout()
    {
        float vertical_offset = 0;
        float combined_operand_height = 0;

        foreach (RectTransform operand_transform in operand_container)
        {
            Vector3 target_position = new Vector3(0, vertical_offset - (operand_transform as RectTransform).rect.height / 2);
            operand_transform.localPosition = Vector3.Lerp(operand_transform.localPosition, target_position, Time.deltaTime * 3);
            //operand_transform.localPosition = target_position;

            vertical_offset -= (operand_transform as RectTransform).rect.height;
            combined_operand_height += (operand_transform as RectTransform).rect.height;
        }

        (transform as RectTransform).sizeDelta = new Vector2((transform as RectTransform).sizeDelta.x,
                                                                primary_codon_element.rect.height + combined_operand_height);
    }

    public void Initialize(int codon_index_offset_)
    {
        codon_index_offset = codon_index_offset_;

        UpdateCodon();
    }

    void UpdateCodon()
    {
        string codon = GetLogicalCodon();

        Image[] images = { codon_background, rung0, rung1, rung2 };
        string nucleotides = codon[0].ToString() + codon;

        for (int i = 0; i < images.Length; i++)
            switch (nucleotides[i])
            {
                case 'V': images[i].color = Color.white; break;
                case 'C': images[i].color = Colors.CommandYellow; break;
                case 'F': images[i].color = Colors.FunctionBlue; break;
                case 'L': images[i].color = Colors.LocusRed; break;
            }

        rung0_text.text = codon[0].ToString();
        rung1_text.text = codon[1].ToString();
        rung2_text.text = codon[2].ToString();

        switch (codon[0])
        {
            case 'V':

                bool is_location = false;

                string previous_codon = CommandNode.SectorNode.Sector.DNA.GetCodon(CodonIndex);
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
                    if (previous_codon == "CCC")
                        switch (Interpretase.SpinCommand.CodonToDirection(codon))
                        {
                            case Interpretase.SpinCommand.Direction.Right: codon_text.text = "Right"; break;
                            case Interpretase.SpinCommand.Direction.Left: codon_text.text = "Left"; break;
                        }
                    else
                        switch (Interpretase.CodonToDirection(codon))
                        {
                            case Cell.Slot.Relation.Right: codon_text.text = "Right"; break;
                            case Cell.Slot.Relation.Left: codon_text.text = "Left"; break;
                            case Cell.Slot.Relation.Across: codon_text.text = "Across"; break;
                        }
                }
                else
                    codon_text.text = Interpretase.CodonToValue(codon).ToString();

                break;

            case 'C':
                switch (codon)
                {
                    case "CVV": codon_text.text = "Move"; break;
                    case "CVC": codon_text.text = "Take"; break;
                    case "CVF": codon_text.text = "Grab"; break;
                    case "CVL": codon_text.text = "Release"; break;
                    case "CCC": codon_text.text = "Spin"; break;
                    case "CFF": codon_text.text = "Copy"; break;
                    case "CLL": codon_text.text = "Try"; break;
                    case "CLF": codon_text.text = "If"; break;
                }
                break;

            case 'F':
                switch (codon)
                {
                    case "FVV": codon_text.text = "Measure"; break;
                    case "FVC": codon_text.text = "Greater Than"; break;
                    case "FVL": codon_text.text = "Less Than"; break;
                    case "FVF": codon_text.text = "Equal To"; break;
                    case "FCV": codon_text.text = "Add"; break;
                    case "FCC": codon_text.text = "Subtract"; break;
                }
                break;

            case 'L':
                codon_text.text = "Locus " + (Interpretase.CodonToValue(codon) -
                                              Interpretase.CodonToValue("LVV")).ToString();
                break;
        }


        foreach (CommandNodeElement element in GetComponentsInChildren<CommandNodeElement>())
            if (element != this)
                Destroy(element.gameObject);

        int operand_count = Interpretase.GetOperandCount(CommandNode.SectorNode.Sector.DNA, CodonIndex);

        for (int i = 0; i < operand_count; i++)
        {
            CommandNodeElement element = Instantiate(CommandNode.CommandNodeElementPrefab);
            element.transform.SetParent(operand_container.transform);
            element.transform.position = codon_background.transform.position;

            RectTransform element_rect_transform = element.transform as RectTransform;
            element_rect_transform.sizeDelta = new Vector2((transform as RectTransform).rect.width, element_rect_transform.sizeDelta.y);

            element.Initialize(codon_index_offset + i + 1);
        }

        UpdateLayout();
    }

    string GetLogicalCodon()
    {
        return CommandNode.SectorNode.Sector.DNA.GetCodon(CodonIndex);
    }
}
