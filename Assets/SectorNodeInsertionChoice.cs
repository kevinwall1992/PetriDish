using UnityEngine;
using System.Collections;

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

            DNAPanelNode node = null;
            switch(Selection.Value)
            {
                case "Command":
                    node = CommandNode.CreateInstance("CVVVVV");
                    break;

                case "Locus":
                    node = LocusNode.CreateInstance("LLC");
                    break;

                case "Paste":
                    SectorNode.InsertDNASequence(GUIUtility.systemCopyBuffer, ReferenceNode);
                    break;

                default: break;
            }

            if(node!= null)
                SectorNode.InsertNodeBefore(ReferenceNode, node);
            SectorNode.HideInsertionChoice();
        }
    }
}
