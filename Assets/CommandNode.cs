using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CommandNode : DNAPanelNode
{
    [SerializeField]
    CommandNodeElement command_node_element_prefab;
    public CommandNodeElement CommandNodeElementPrefab
    {
        get { return command_node_element_prefab; }
    }

    [SerializeField]
    Image command_icon;

    [SerializeField]
    RectTransform strand_container;

    [SerializeField]
    RectTransform background;

    CommandNodeElement command_node_element = null;

    int codon_length = -1;
    public override int CodonLength
    {
        get
        {
            if (!IsCollapsed)
                codon_length = command_node_element.CodonLength;
            else if (codon_length < 0)
                codon_length = Interpretase.GetOperandCount(SectorNode.Sector.DNA, CodonIndex) + 1;

            return codon_length;
        }
    }

    public override string DNASequence
    {
        get
        {
            if (IsCollapsed)
                return base.DNASequence;

            return command_node_element.DNASequence;
        }
    }

    public override bool IsCollapsed
    {
        set
        {
            base.IsCollapsed = value;

            if (!IsCollapsed)
            {
                if (command_node_element != null)
                {
                    command_node_element.transform.SetParent(null);
                    Destroy(command_node_element.gameObject);
                }

                string codon = SectorNode.Sector.DNA.GetCodon(CodonIndex);

                command_node_element = Instantiate(command_node_element_prefab);
                command_node_element.transform.SetParent(strand_container);
                command_node_element.transform.SetSiblingIndex(1);
                command_node_element.transform.localPosition = Vector3.zero;

                RectTransform element_rect_transform = command_node_element.transform as RectTransform;
                element_rect_transform.sizeDelta = new Vector2((strand_container as RectTransform).rect.width, element_rect_transform.sizeDelta.y);

                command_node_element.Initialize(0);
            }
        }
    }

    protected override void Start()
    {
        UpdateCommandIcon();
    }

    protected override void Update()
    {
        base.Update();

        if (IsCollapsed)
            return;

        UpdateLayout();
    }

    public void Invalidate()
    {
        codon_length = -1;
    }

    public void UpdateCommandIcon()
    {
        string icon_filename = "";

        switch (SectorNode.Sector.DNA.GetCodon(CodonIndex))
        {
            case "CVV":
                string first_operand = SectorNode.Sector.DNA.GetCodon(CodonIndex + 1);

                if (first_operand[0] == 'V')
                    switch (Interpretase.ValueToDirection(Interpretase.CodonToValue(first_operand)))
                    {
                        case Cell.Slot.Relation.Right: icon_filename = "move_right_icon"; break;
                        case Cell.Slot.Relation.Left: icon_filename = "move_left_icon"; break;
                        case Cell.Slot.Relation.Across: icon_filename = "move_across_icon"; break;
                        default: icon_filename = "move_function_icon"; break;
                    }
                else
                    icon_filename = "move_function_icon";
                break;

            case "CVC": icon_filename = "take_icon"; break;
            case "CVF": icon_filename = "grab_icon"; break;
            case "CVL": icon_filename = "release_icon"; break;
            case "CCC": icon_filename = "spin_icon"; break;
            case "CFF": icon_filename = "copy_icon"; break;
            case "CLV": icon_filename = "pass_icon"; break;
            case "CLC": icon_filename = "wait_icon"; break;
            case "CLF": icon_filename = "if_icon"; break;
            case "CLL": icon_filename = "try_icon"; break;
        }

        command_icon.sprite = Resources.Load<Sprite>(icon_filename);
    }

    void UpdateLayout()
    {
        float vertical_offset = 0;
        foreach (RectTransform element_transform in strand_container)
        {
            Vector3 target_position = new Vector3(element_transform.rect.width / 2, vertical_offset - element_transform.rect.height / 2);
            element_transform.localPosition = Vector3.Lerp(element_transform.localPosition, target_position, Time.deltaTime * 3);
            //element_transform.localPosition = target_position;

            vertical_offset -= (element_transform as RectTransform).rect.height;
        }

        background.sizeDelta = new Vector2(background.sizeDelta.x, -vertical_offset);
    }


    public static CommandNode CreateInstance(string dna_sequence)
    {
        CommandNode command_dna_node = Instantiate(Scene.Micro.Prefabs.CommandNode);

        return command_dna_node;
    }
}