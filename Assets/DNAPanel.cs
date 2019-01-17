using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DNAPanel : DetailPanel
{
    [SerializeField]
    CodonElement codon_element_prefab;

    [SerializeField]
    DNASequenceElement sequence_element_prefab;

    [SerializeField]
    GameObject grouping_panel_prefab;


    DNA DNA { get { return Data as DNA; } }

    [SerializeField]
    CodonElementLayout codon_layout;
    public CodonElementLayout CodonLayout
    {
        get { return codon_layout; }
    }

    [SerializeField]
    DNASequenceElementLayout sequence_layout;
    public DNASequenceElementLayout SequenceLayout
    {
        get { return sequence_layout; }
    }

    public Vector3 SpawnPosition
    {
        get;
        set;
    }

    private void Awake()
    {
        SpawnPosition = transform.position;
    }

    protected override void Start()
    {
        AddDNASequence(DNA.Sequence);

        InitializeSequenceElements();

        base.Start();
    }

    protected override void Update()
    {
        if (Input.GetKeyUp(KeyCode.C))
            GUIUtility.systemCopyBuffer = GetDNASequence();
        if (Input.GetKeyUp(KeyCode.V))
            AddDNASequence(GUIUtility.systemCopyBuffer);
        if (Input.GetKeyUp(KeyCode.B))
            ClearDNASequence();

        if (!CodonLayout.Validate())
            FormatCodons();

        base.Update();
    }

    private void OnDisable()
    {
        if (DNA == null)
            return;

        DNA.RemoveStrand(0, DNA.CodonCount);
        DNA.AddSequence(GetDNASequence());
        DNA.ActiveCodonIndex = 0;
    }

    void InitializeSequenceElements()
    {
        AddDNASequenceElement("CAAAAAAAC", "Move One Unit");
        AddDNASequenceElement("CCCAAAAAC", "Move Half Stack");
        AddDNASequenceElement("CGGAAAAAC", "Move Full Stack");
        AddDNASequenceElement("CTTAAAAAC", "Move Max");
        AddDNASequenceElement("CCAAAAAAC", "Swap");
        AddDNASequenceElement("CATTCTAAA", "Cut and Paste DNA");
        AddDNASequenceElement("CACAAAAAC", "Activate Slot");
        AddDNASequenceElement("CAGTCTAAC", "Go To Marker");
        AddDNASequenceElement("CAGTCTGAGGAAAAAGAAAAC", "Conditionally Go To");

        AddDNASequenceElement("TCTTTT", "Marked Group");

        AddDNASequenceElement("GAAAAA", "Get Size of Slot");
        AddDNASequenceElement("GAGAAAAAC", "A == B");
        AddDNASequenceElement("GACAAAAAC", "A > B");
        AddDNASequenceElement("GATAAAAAC", "A < B");

        AddDNASequenceElement("AAA", "Slot 1");

        AddDNASequenceElement("AAA", "0");

        AddDNASequenceElement(Ribozyme.GetRibozymeFamily("Interpretase")[0].Sequence, "Interpretase");
        AddDNASequenceElement(Ribozyme.GetRibozymeFamily("Rotase")[0].Sequence, "Rotase");
        AddDNASequenceElement(Ribozyme.GetRibozymeFamily("Constructase")[0].Sequence, "Constructase");

        foreach (Reaction reaction in Reaction.Reactions)
            if (reaction.Catalyst is Ribozyme)
                AddDNASequenceElement((reaction.Catalyst as Ribozyme).Sequence, (reaction.Catalyst as Ribozyme).Name);
    }

    void FormatCodons()
    {
        Dictionary<int, int> indentation_levels = GetIndentationLevels();
        foreach (int index in indentation_levels.Keys)
            CodonLayout.SetElementOffset(index, new Vector2(20 * indentation_levels[index], 0));


        GameObject grouping_panels_container = FindDescendent("command_groups");
        foreach (Transform child_transform in grouping_panels_container.transform)
            GameObject.Destroy(child_transform.gameObject);

        float element_height = (codon_element_prefab.transform as RectTransform).sizeDelta.y;
        Color gray = Color.Lerp(Color.Lerp(Color.gray, Color.white, 0.3f), Color.clear, 0.3f);

        Dictionary<int, int> command_groups = GetCommandGroups();
        bool use_gray = false;

        foreach (int command_index in command_groups.Keys)
        {
            GameObject grouping_panel = Instantiate(grouping_panel_prefab);
            grouping_panel.transform.SetParent(grouping_panels_container.transform, false);

            Vector2 size_change = new Vector2(0 * indentation_levels[command_index], (element_height + 5) * (command_groups[command_index] - 1));
            (grouping_panel.transform as RectTransform).sizeDelta += size_change;
            grouping_panel.transform.localPosition += new Vector3(0, (-(element_height + 5) * (command_index) - size_change.y / 2), 0);

            if (use_gray)
                grouping_panel.GetComponent<Image>().color = gray;
            use_gray = !use_gray;
        }
    }

    Dictionary<int, int> GetIndentationLevels()
    {
        Dictionary<int, int> indentation_levels = new Dictionary<int, int>();

        int indentation_level = 0;

        DNA dna = new DNA(GetDNASequence());
        int codon_index = 0;

        while (codon_index < dna.CodonCount)
        {
            string codon = dna.GetCodon(codon_index);
            if (codon[0] == 'C')
            {
                int command_length = Interpretase.GetCommandLength(dna, codon_index);

                for (int i = 0; i < command_length; i++)
                    indentation_levels[codon_index++] = indentation_level;

                continue;
            }
            else
                indentation_levels[codon_index] = indentation_level;

            int value = Interpretase.CodonToValue(codon);
            if (codon[0] == 'T' && value >= 55)
            {
                if (value < 63)
                    indentation_level++;
                else
                    indentation_levels[codon_index] = --indentation_level;
            }

            codon_index++;
        }

        return indentation_levels;
    }

    Dictionary<int, int> GetCommandGroups()
    {
        Dictionary<int, int> command_groups = new Dictionary<int, int>();

        DNA dna = new DNA(GetDNASequence());

        int command_codon_index = Interpretase.FindCommandCodon(dna, 0);
        while (command_codon_index >= 0)
        {
            int command_length = Interpretase.GetCommandLength(dna, command_codon_index);

            command_groups[command_codon_index] = command_length;

            command_codon_index = Interpretase.FindCommandCodon(dna, command_codon_index + command_length);
        }

        return command_groups;
    }

    bool IsValidCodon(string codon)
    {
        return (codon[0] == 'A' || codon[0] == 'C' || codon[0] == 'G' || codon[0] == 'T') &&
                (codon[1] == 'A' || codon[1] == 'C' || codon[1] == 'G' || codon[1] == 'T') &&
                (codon[2] == 'A' || codon[2] == 'C' || codon[2] == 'G' || codon[2] == 'T');
    }

    public string GetDNASequence()
    {
        string sequence = "";

        for (int i = 0; i < CodonLayout.ElementCount; i++)
            sequence += CodonLayout.GetCodonElement(i).Codon;

        return sequence;
    }

    public void AddDNASequence(string sequence, int index = -1)
    {
        for (int i = 0; i < sequence.Length / 3; i++)
        {
            string codon = sequence.Substring(i * 3, 3);
            if (!IsValidCodon(codon))
                continue;

            CodonElement codon_element = Instantiate(codon_element_prefab);
            CodonLayout.AddCodonElement(codon_element, index < 0 ? -1 : index++);

            codon_element.transform.position = SpawnPosition;

            codon_element.Codon = codon;
        }
    }

    public void ClearDNASequence()
    {
        CodonLayout.Clear();
    }

    public void AddDNASequenceElement(string sequence, string description)
    {
        DNASequenceElement dna_seqence_element = Instantiate(sequence_element_prefab);
        SequenceLayout.AddDNASequenceElement(dna_seqence_element);

        dna_seqence_element.transform.position = SpawnPosition;

        dna_seqence_element.DNASequence = sequence;
        dna_seqence_element.Description = description;
    }

    public static DNAPanel Create(DNA dna)
    {
        DNAPanel dna_panel = Instantiate(Scene.Micro.Prefabs.DNAPanel);
        dna_panel.transform.SetParent(Scene.Micro.Canvas.transform, false);

        dna_panel.Data = dna;

        return dna_panel;
    }
}
