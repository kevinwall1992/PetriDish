using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class SectorNodeInsertionChoice : GoodBehavior, Choice<string>
{
    Option<string> selection = null;

    SectorNode SectorNode { get { return GetComponentInParent<SectorNode>(); } }

    public DNAPanelNode ReferenceNode { get; set; }

    public Option<string> Selection
    {
        get
        {
            return selection;
        }

        set
        {
            selection = value;

            List<Program.Code> codes = new List<Program.Code>();
            switch(Selection.Value)
            {
                case "Command":
                    codes.Add(new Program.CommandToken(Program.CommandType.Move));
                    codes.Add(new Program.ValueToken(0));
                    break;

                case "Locus":
                    codes.Add(new Program.LocusToken(0));
                    break;

                case "Paste":
                    JArray json_codes_array = JArray.Parse(GUIUtility.systemCopyBuffer);
                    foreach (JToken json_code_token in json_codes_array)
                        codes.Add(Program.DecodeCode(json_code_token as JObject));
                    break;
            }

            SectorNode.InsertCodesBefore(ReferenceNode, codes);

            SectorNode.HideInsertionChoice();
        }
    }
}
