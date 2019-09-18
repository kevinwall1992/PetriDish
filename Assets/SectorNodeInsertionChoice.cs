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

            string dna_sequence = "";
            switch(Selection.Value)
            {
                case "Command": dna_sequence = "CVVVVV"; break;
                case "Locus": dna_sequence = "LLC"; break;
                case "Paste": dna_sequence = GUIUtility.systemCopyBuffer; break;
            }

            SectorNode.InsertDNASequence(dna_sequence, ReferenceNode);

            SectorNode.HideInsertionChoice();
        }
    }
}
