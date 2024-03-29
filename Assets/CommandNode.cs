﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

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

    List<Program.Token> tokens_given = null;
    public override IEnumerable<Program.Code> Codes
    {
        get
        {
            IEnumerable<Program.Token> tokens;

            if (command_node_element != null)
                tokens = command_node_element.Tokens;
            else
                tokens = tokens_given;

            return Program.TokensToCodes(tokens);
        }
    }

    public override bool IsCollapsed
    {
        set
        {
            base.IsCollapsed = value;

            if (!IsCollapsed)
                InitializeCommandNodeElement(tokens_given);
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

    public void UpdateCommandIcon()
    {
        string icon_filename = "";

        List<Program.Code> codes_list = new List<Program.Code>(Codes);

        switch ((codes_list[0] as Program.CommandToken).Type)
        {
            case Program.CommandType.Move:
                Program.Token first_operand = codes_list[1] as Program.Token;

                if (first_operand is Program.ValueToken)
                    switch (Interpretase.ValueToDirection((first_operand as Program.ValueToken).Value))
                    {
                        case Cell.Slot.Relation.Right: icon_filename = "move_right_icon"; break;
                        case Cell.Slot.Relation.Left: icon_filename = "move_left_icon"; break;
                        case Cell.Slot.Relation.Across: icon_filename = "move_across_icon"; break;
                        default: icon_filename = "move_function_icon"; break;
                    }
                else
                    icon_filename = "move_function_icon";
                break;

            case Program.CommandType.Take: icon_filename = "take_icon"; break;
            case Program.CommandType.Grab: icon_filename = "grab_icon"; break;
            case Program.CommandType.Release: icon_filename = "release_icon"; break;
            case Program.CommandType.Spin: icon_filename = "spin_icon"; break;
            case Program.CommandType.Copy: icon_filename = "copy_icon"; break;
            case Program.CommandType.Pass: icon_filename = "pass_icon"; break;
            case Program.CommandType.Wait: icon_filename = "wait_icon"; break;
            case Program.CommandType.If: icon_filename = "if_icon"; break;
            case Program.CommandType.Try: icon_filename = "try_icon"; break;
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

    void InitializeCommandNodeElement(List<Program.Token> tokens)
    {
        if(command_node_element != null)
        {
            command_node_element.transform.SetParent(null);
            Destroy(command_node_element);
        }

        command_node_element = Instantiate(command_node_element_prefab);
        command_node_element.transform.SetParent(strand_container);
        command_node_element.transform.SetSiblingIndex(1);
        command_node_element.transform.localPosition = Vector3.zero;

        RectTransform element_rect_transform = command_node_element.transform as RectTransform;
        element_rect_transform.sizeDelta = new Vector2((strand_container as RectTransform).rect.width, element_rect_transform.sizeDelta.y);

        command_node_element.Initialize(tokens[0], tokens);
    }


    public static CommandNode CreateInstance(IEnumerable<Program.Token> tokens)
    {
        CommandNode command_dna_node = Instantiate(Scene.Micro.Prefabs.CommandNode);
        command_dna_node.tokens_given = new List<Program.Token>(tokens);

        return command_dna_node;
    }
}