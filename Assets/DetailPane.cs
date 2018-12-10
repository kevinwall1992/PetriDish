using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;


public class DetailPane : GoodBehavior
{
    CodonElementLayout codon_layout;
    DNASequenceElementLayout sequence_layout;

    GameObject codon_element_prefab;
    GameObject sequence_element_prefab;

    GameObject grouping_panel_prefab;
    Vector3 grouping_panel_prefab_local_position;

    public CodonElementLayout CodonLayout
    {
        get { return codon_layout; }
    }

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
        codon_layout = FindDescendent<CodonElementLayout>("codon_list");
        sequence_layout = FindDescendent<DNASequenceElementLayout>("sequence_list");

        codon_element_prefab = codon_layout.FindDescendent("element");
        codon_element_prefab.transform.parent = null;
        codon_element_prefab.SetActive(false);

        sequence_element_prefab = sequence_layout.FindDescendent("element");
        sequence_element_prefab.transform.parent = null;
        sequence_element_prefab.SetActive(false);

        grouping_panel_prefab = FindDescendent("command_group");
        grouping_panel_prefab_local_position = grouping_panel_prefab.transform.localPosition;
        grouping_panel_prefab.transform.parent = null;
        grouping_panel_prefab.SetActive(false);

        SpawnPosition = transform.position;
    }

    private void Start()
    {
        AddDNASequence("CACTCCAATTCT" + Ribozyme.GetRibozymeFamily("Rotase")[0].Sequence + "TTTCATTCTTACTGACAATCCTACCAGTGAGACGAATCCAAATTT");

        InitializeSequenceElements();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.C))
            GUIUtility.systemCopyBuffer = GetDNASequence();
        if (Input.GetKeyUp(KeyCode.V))
            AddDNASequence(GUIUtility.systemCopyBuffer);
        if (Input.GetKeyUp(KeyCode.B))
            ClearDNASequence();

        if (Input.GetKeyUp(KeyCode.Q))
        {
            gameObject.SetActive(false);
            Scene.Micro.Visualization.OrganismComponents[0].ResetExperiment(GetDNASequence());
        }

        if (!codon_layout.Validate())
            FormatCodons();
    }

    void InitializeSequenceElements()
    {
        AddDNASequenceElement("CAATAATAC", "Move One Unit");
        AddDNASequenceElement("CCCTAATAC", "Move Half Stack");
        AddDNASequenceElement("CGGTAATAC", "Move Full Stack");
        AddDNASequenceElement("CTTTAATAC", "Move Max");
        AddDNASequenceElement("CATTCTTAA", "Cut and Paste DNA");
        AddDNASequenceElement("CACTAAAAC", "Activate Slot");
        AddDNASequenceElement("CAGTCTAAC", "Go To Marker");
        AddDNASequenceElement("CAGTCTGAGGAATAAGAATAC", "Conditionally Go To");

        AddDNASequenceElement("TCTTTT", "Marked Group");

        AddDNASequenceElement("GAATAA", "Get Size of Slot");
        AddDNASequenceElement("GAGTAATAC", "A == B");
        AddDNASequenceElement("GACTAATAC", "A > B");
        AddDNASequenceElement("GATTAATAC", "A < B");

        AddDNASequenceElement("TAA", "Slot 1");

        AddDNASequenceElement("AAA", "0");

        AddDNASequenceElement(Ribozyme.GetRibozymeFamily("Interpretase")[0].Sequence, "Interpretase");
        AddDNASequenceElement(Ribozyme.GetRibozymeFamily("Rotase")[0].Sequence, "Rotase");
        AddDNASequenceElement(Ribozyme.GetRibozymeFamily("Constructase")[0].Sequence, "Constructase");

        foreach(Reaction reaction in Reaction.Reactions)
            if(reaction.Catalyst is Ribozyme)
                AddDNASequenceElement((reaction.Catalyst as Ribozyme).Sequence, (reaction.Catalyst as Ribozyme).Name);
    }

    void FormatCodons()
    {
        Dictionary<int, int> indentation_levels = GetIndentationLevels();
        foreach (int index in indentation_levels.Keys)
            codon_layout.SetElementOffset(index, new Vector2(20 * indentation_levels[index], 0));


        GameObject grouping_panels_container = FindDescendent("command_groups");
        foreach (Transform child_transform in grouping_panels_container.transform)
            GameObject.Destroy(child_transform.gameObject);

        float element_height = (codon_element_prefab.transform as RectTransform).sizeDelta.y;
        Color gray = Color.Lerp(Color.Lerp(Color.gray, Color.white, 0.3f), Color.clear, 0.3f);

        Dictionary<int, int> command_groups = GetCommandGroups();
        bool use_gray = false;

        foreach (int command_index in command_groups.Keys)
        {
            GameObject grouping_panel = GameObject.Instantiate(grouping_panel_prefab);
            grouping_panel.SetActive(true);
            grouping_panel.transform.parent = grouping_panels_container.transform;
            grouping_panel.transform.localPosition = grouping_panel_prefab_local_position;

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

        while(codon_index < dna.CodonCount)
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
            if (codon[0]== 'T' && value >= 55)
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

        for (int i = 0; i < codon_layout.ElementCount; i++)
            sequence += codon_layout.GetCodonElement(i).Codon;

        return sequence;
    }

    public void AddDNASequence(string sequence, int index = -1)
    {
        for (int i = 0; i < sequence.Length / 3; i++)
        {
            string codon = sequence.Substring(i * 3, 3);
            if (!IsValidCodon(codon))
                continue;

            CodonElement codon_element = GameObject.Instantiate(codon_element_prefab).GetComponent<CodonElement>();
            codon_layout.AddCodonElement(codon_element, index < 0 ? -1 : index++);

            codon_element.transform.position = SpawnPosition;

            codon_element.Codon = codon;
        }
    }

    public void ClearDNASequence()
    {
        codon_layout.Clear();
    }

    public void AddDNASequenceElement(string sequence, string description)
    {
        DNASequenceElement dna_seqence_element = GameObject.Instantiate(sequence_element_prefab).GetComponent<DNASequenceElement>();
        sequence_layout.AddDNASequenceElement(dna_seqence_element);

        dna_seqence_element.transform.position = SpawnPosition;

        dna_seqence_element.DNASequence = sequence;
        dna_seqence_element.Description = description;
    }
}