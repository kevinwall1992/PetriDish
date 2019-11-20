using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

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

    float scroll_value = 0;

    public CommandNode CommandNode { get { return GetComponentInParent<CommandNode>(); } }

    public Program.Token Token { get; private set; }

    public List<Program.Token> Tokens
    {
        get
        {
            List<Program.Token> tokens = new List<Program.Token>();
            tokens.Add(Token);

            foreach (Transform child_transform in operand_container.transform)
                tokens.AddRange(child_transform.GetComponent<CommandNodeElement>().Tokens);

            return tokens;
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
                List<Program.Token> new_tokens = new List<Program.Token>();

                if (Token is Program.CommandToken)
                {
                    Program.CommandToken command_token = Token as Program.CommandToken;
                    List<Program.CommandType> command_types = new List<Program.CommandType>(
                        (Program.CommandType[])System.Enum.GetValues(typeof(Program.CommandType)));

                    int rotated_index = MathUtility.Mod(command_types.IndexOf(command_token.Type) + (int)scroll_value, command_types.Count);
                    command_token.Type = command_types[rotated_index];

                    new_tokens.Add(command_token);


                    switch (command_token.Type)
                    {


                        case Program.CommandType.Move:
                        case Program.CommandType.Spin:
                        case Program.CommandType.Wait:
                            new_tokens.Add(new Program.ValueToken(0));
                            break;

                        case Program.CommandType.Take:
                            new_tokens.Add(new Program.ValueToken(0));
                            new_tokens.Add(new Program.ValueToken(1));
                            break;

                        case Program.CommandType.Copy:
                            new_tokens.Add(new Program.LocusToken(0));
                            new_tokens.Add(new Program.LocusToken(0));
                            break;

                        case Program.CommandType.If:
                            new_tokens.Add(new Program.LocusToken(0));
                            new_tokens.Add(new Program.ValueToken(0));
                            break;

                        case Program.CommandType.Try:
                            new_tokens.Add(new Program.LocusToken(0));
                            new_tokens.Add(new Program.CommandToken(Program.CommandType.Pass));
                            break;

                        case Program.CommandType.Grab:
                        case Program.CommandType.Release:
                        case Program.CommandType.Pass:
                        default:
                            break;
                    }
                }
                else if (Token is Program.FunctionToken)
                {
                    Program.FunctionToken function_token = Token as Program.FunctionToken;
                    List<Program.FunctionType> function_types = new List<Program.FunctionType>(
                        (Program.FunctionType[])System.Enum.GetValues(typeof(Program.FunctionType)));

                    int rotated_index = MathUtility.Mod(function_types.IndexOf(function_token.Type) + (int)scroll_value, function_types.Count);
                    function_token.Type = function_types[rotated_index];

                    new_tokens.Add(function_token);


                    switch (function_token.Type)
                    {
                        case Program.FunctionType.Measure:
                            break;

                        case Program.FunctionType.Add:
                        case Program.FunctionType.Subtract:
                        case Program.FunctionType.LessThan:
                        case Program.FunctionType.GreaterThan:
                        case Program.FunctionType.EqualTo:
                            new_tokens.Add(new Program.ValueToken(0));
                            new_tokens.Add(new Program.ValueToken(0));
                            break;

                    }
                }
                else if (Token is Program.LocusToken)
                {
                    Program.LocusToken locus_token = Token as Program.LocusToken;

                    locus_token.Location = locus_token.Location + (int)scroll_value;

                    new_tokens.Add(locus_token);
                }
                else
                {
                    Program.ValueToken value_token = Token as Program.ValueToken;

                    value_token.Value = value_token.Value + (int)scroll_value;

                    new_tokens.Add(value_token);
                }

                Program.Code reference_code = CommandNode.SectorNode.Sector.GetNextCode(Tokens.Last());
                CommandNode.SectorNode.Sector.Remove(Tokens.First(), Tokens.Last());
                CommandNode.SectorNode.Sector.InsertBefore(reference_code, Program.TokensToCodes(new_tokens));

                Initialize(new_tokens.First(), new_tokens);
                CommandNode.UpdateCommandIcon();

                scroll_value -= (int)scroll_value;
            }
        }
    }

    Program.Token GetPreviousToken()
    {
        CommandNodeElement parent = transform.parent.GetComponentInParent<CommandNodeElement>();
        if (parent == null)
            return null;

        List<Program.Token> tokens = parent.Tokens;

        return tokens[tokens.IndexOf(Token) - 1];
    }

    public void Initialize(Program.Token token, List<Program.Token> tokens)
    {
        Token = token;
        int token_index = tokens.IndexOf(Token);

        string codon = Token.Codon;


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

        
        if(Token is Program.CommandToken)
        {
            codon_text.text = (Token as Program.CommandToken).Type.ToString();
        }
        else if (Token is Program.FunctionToken)
        {
            codon_text.text = (Token as Program.FunctionToken).Type.ToString();
        }
        else if (Token is Program.ValueToken)
        {
            codon_text.text = Interpretase.CodonToValue(codon).ToString();

            Program.Token previous_token = GetPreviousToken();
            if (previous_token is Program.CommandToken)
                switch ((previous_token as Program.CommandToken).Type)
                {
                    case Program.CommandType.Move:
                    case Program.CommandType.Take:
                        switch (Interpretase.CodonToDirection(codon))
                        {
                            case Cell.Slot.Relation.Right: codon_text.text = "Right"; break;
                            case Cell.Slot.Relation.Left: codon_text.text = "Left"; break;
                            case Cell.Slot.Relation.Across: codon_text.text = "Across"; break;
                        }
                        break;

                    case Program.CommandType.Spin:
                        switch (Interpretase.SpinCommand.CodonToDirection(codon))
                        {
                            case Interpretase.SpinCommand.Direction.Right: codon_text.text = "Right"; break;
                            case Interpretase.SpinCommand.Direction.Left: codon_text.text = "Left"; break;
                        }
                        break;
                }
        }
        else
        {
            codon_text.text = "Locus " + (Interpretase.CodonToValue(codon) -
                                          Interpretase.CodonToValue("LVV")).ToString();
        }


        foreach (CommandNodeElement element in GetComponentsInChildren<CommandNodeElement>())
            if (element != this)
            {
                element.transform.SetParent(null);
                Destroy(element.gameObject);
            }
 
        int operand_count = Interpretase.GetOperandCount(new DNA(Program.TokensToDNASequence(tokens)), token_index);

        for (int i = 0; i < operand_count;)
        {
            CommandNodeElement element = Instantiate(CommandNode.CommandNodeElementPrefab);
            element.transform.SetParent(operand_container.transform);
            element.transform.position = codon_background.transform.position;

            RectTransform element_rect_transform = element.transform as RectTransform;
            element_rect_transform.sizeDelta = new Vector2((transform as RectTransform).rect.width, element_rect_transform.sizeDelta.y);

            element.Initialize(tokens[token_index + i + 1], tokens);
            i += element.Tokens.Count;
        }

        UpdateLayout();
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
}
