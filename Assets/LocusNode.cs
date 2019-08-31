using UnityEngine;
using System.Collections;

public class LocusNode : DNAPanelNode, Choice<string>
{
    [SerializeField]
    LocusIcon icon;

    [SerializeField]
    Transform options;

    Option<string> selection = null;

    public override string DNASequence
    {
        get
        {
            return icon.Codon;
        }
    }

    public Option<string> Selection
    {
        get { return selection; }

        set
        {
            selection = value;

            SectorNode.Sector.DNA.RemoveSequence(CodonIndex, 1);
            SectorNode.Sector.DNA.InsertSequence(CodonIndex, selection.Value);
            Scene.Micro.Editor.Do();

            icon.Codon = selection.Value;

            IsCollapsed = true;
        }
    }

    void Start()
    {
        int i = 0;
        foreach (LocusIcon locus_icon in options.GetComponentsInChildren<LocusIcon>())
            locus_icon.Codon = Interpretase.ValueToCodon(i++ + 48);
    }

    protected override void Update()
    {
        base.Update();
    }

    public static LocusNode CreateInstance(string codon)
    {
        LocusNode locus_node = Instantiate(Scene.Micro.Prefabs.LocusNode);
        locus_node.icon.Codon = codon;

        return locus_node;
    }
}
