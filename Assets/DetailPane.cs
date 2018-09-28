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
        AddDNASequence("TCTACTGTAATCGGTTTTCATTCTTACCCCTCCTAC");
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
            World.TheWorld.GetComponentInChildren<OrganismComponent>().ResetExperiment(GetDNASequence());
        }

        if (!codon_layout.Validate())
            FormatCodons();
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

            Vector2 size_change = new Vector2(0 * indentation_levels[command_index], (element_height + 5) * command_groups[command_index]);
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

        Interpretase interpretase = new Interpretase();
        DNA dna = new DNA(GetDNASequence());

        int operand_count = 0;

        while (dna.ActiveCodonIndex < dna.GetCodonCount())
        {
            indentation_levels[dna.ActiveCodonIndex] = indentation_level;

            if (operand_count > 0)
            {
                operand_count--;
                dna.ActiveCodonIndex++;
                continue;
            }

            string codon = dna.GetCodon(dna.ActiveCodonIndex);
            if (codon[0] == 'C')
            {
                switch (codon)
                {
                    case "CAA":
                    case "CCC":
                    case "CAC":
                    case "CAT":
                        operand_count = 2;
                        break;

                    case "CAG":
                        operand_count = 1;
                        break;
                }
            }
            else if (codon[0] == 'T')
            {
                int value = Interpretase.CodonToValue(codon);

                if (value >= 55)
                {
                    if (value < 63)
                        indentation_level++;
                    else
                        indentation_levels[dna.ActiveCodonIndex] = --indentation_level;
                }
            }

            dna.ActiveCodonIndex++;
        }

        return indentation_levels;
    }

    Dictionary<int, int> GetCommandGroups()
    {
        Dictionary<int, int> command_groups = new Dictionary<int, int>();

        Interpretase interpretase = new Interpretase();
        DNA dna = new DNA(GetDNASequence());

        int command_codon_index = -1;

        while (dna.ActiveCodonIndex < dna.GetCodonCount())
        {
            string codon = dna.GetCodon(dna.ActiveCodonIndex);

            bool in_group = command_codon_index >= 0 && (dna.ActiveCodonIndex <= (command_codon_index + command_groups[command_codon_index]));

            switch (codon[0])
            {
                case 'A':
                    break;

                case 'C':
                    if (in_group)
                        command_groups[command_codon_index] = dna.ActiveCodonIndex- command_codon_index - 1;

                    command_codon_index = dna.ActiveCodonIndex;

                    switch (codon)
                    {
                        case "CAA":
                        case "CCC":
                        case "CAC":
                        case "CAG":
                        case "CAT": command_groups[command_codon_index] = 2; break;
                    }

                    in_group = true;

                    break;

                case 'G':
                    if (in_group)
                        switch (codon)
                        {
                            case "GAA": command_groups[command_codon_index] += 1; break;
                            case "GAC":
                            case "GAG":
                            case "GAT": command_groups[command_codon_index] += 2; break;
                        }
                    break;

                case 'T':
                    break;

            }

            dna.ActiveCodonIndex++;
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