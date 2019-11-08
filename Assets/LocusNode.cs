using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LocusNode : DNAPanelNode, Choice<string>
{
    [SerializeField]
    LocusIcon icon;

    [SerializeField]
    Transform options;

    Program.LocusToken token;

    Option<string> selection = null;

    public Option<string> Selection
    {
        get { return selection; }

        set
        {
            selection = value;

            token.Location = Interpretase.CodonToValue(selection.Value) - 48;

            icon.Codon = selection.Value;

            IsCollapsed = true;
        }
    }

    public override IEnumerable<Program.Code> Codes
    {
        get { return Utility.CreateList<Program.Code>(token); }
    }

    protected override void Start()
    {
        base.Start();

        int i = 0;
        foreach (LocusIcon locus_icon in options.GetComponentsInChildren<LocusIcon>())
            locus_icon.Codon = Interpretase.ValueToCodon(i++ + 48);
    }

    protected override void Update()
    {
        base.Update();
    }

    public static LocusNode CreateInstance(Program.LocusToken token)
    {
        LocusNode locus_node = Instantiate(Scene.Micro.Prefabs.LocusNode);
        locus_node.token = token;
        locus_node.icon.Codon = token.Codon;

        return locus_node;
    }
}
