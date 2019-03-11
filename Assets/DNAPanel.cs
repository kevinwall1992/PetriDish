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

    [SerializeField]
    Image arrow;


    public DNA DNA
    {
        get
        {
            Molecule molecule = Data as Molecule;
            if (molecule == null)
                return null;

            if (molecule is Catalyst)
            {
                Compound cofactor = (molecule as Catalyst).GetCofactor<DNA>();
                if (cofactor == null)
                    return null;

                return cofactor.Molecule as DNA;
            }

            if (molecule is DNA)
                return molecule as DNA;

            return null;
        }
    }

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
        GetComponent<CanvasGroup>().interactable = !Scene.Micro.Visualization.IsVisualizingStep;

        if (DNA == null)
        {
            Destroy(gameObject);
            return;
        }

        bool show_arrow = false;

        if (!IsBeingEdited())
        {
            UpdateDNASequence();

            if (DNA.Sequence.Length > 0)
                show_arrow = true;
        }

        if (show_arrow)
        {
            arrow.gameObject.SetActive(true);

            int logical_index = GetLogicalActiveCommandCodonIndex();
            if (logical_index < 0)
            {
                arrow.gameObject.SetActive(false);
                return;
            }

            int visual_index = GetVisualActiveCommandCodonIndex();
            if (logical_index != visual_index)
                arrow.transform.SetParent(CodonLayout.GetCodonElement(logical_index).transform);

            arrow.transform.position = Vector3.Lerp(arrow.transform.position,
                                                    arrow.GetComponentInParent<CodonElement>().ArrowTransform.position,
                                                    Time.deltaTime * 5);   
        }
        else
            arrow.gameObject.SetActive(false);

        if (Input.GetKeyUp(KeyCode.C))
            GUIUtility.systemCopyBuffer = DNA.Sequence;
        if (Input.GetKeyUp(KeyCode.V))
            AddDNASequence(GUIUtility.systemCopyBuffer);
        if (Input.GetKeyUp(KeyCode.B))
            ClearDNASequence();

        if (!CodonLayout.Validate())
            FormatCodons();

        base.Update();
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

        DNA dna = DNA.Copy() as DNA;
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
            if (codon[0] == 'T')
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

        DNA dna = DNA.Copy() as DNA;

        int command_codon_index = Interpretase.FindCommandCodon(dna, 0);
        while (command_codon_index >= 0)
        {
            int command_length = Interpretase.GetCommandLength(dna, command_codon_index);

            command_groups[command_codon_index] = command_length;

            command_codon_index = Interpretase.FindCommandCodon(dna, command_codon_index + command_length);
        }

        return command_groups;
    }

    bool IsBeingEdited()
    {
        foreach (DNAPanelElement element in GetComponentsInChildren<DNAPanelElement>())
            if (element.IsBeingDragged)
                return true;

        return false;
    }

    string GetVisualDNASequence()
    {
        string sequence = "";

        for (int i = 0; i < CodonLayout.ElementCount; i++)
            sequence += CodonLayout.GetCodonElement(i).Codon;

        return sequence;
    }

    string GetVisualCodon(int index)
    {
        return CodonLayout.GetCodonElement(index).Codon;
    }

    int GetVisualActiveCommandCodonIndex()
    {
        CodonElement codon_element = arrow.GetComponentInParent<CodonElement>();
        if (codon_element == null)
            return -1;

        return CodonLayout.GetElementIndex(codon_element.gameObject);
    }

    int GetLogicalActiveCommandCodonIndex()
    {
        int command_codon_index = DNA.ActiveCodonIndex - 1;
        while (DNA.GetCodon(++command_codon_index)[0] != 'C')
            if (command_codon_index >= DNA.CodonCount - 1)
                return -1;

        return command_codon_index;
    }

    bool IsValidCodon(string codon)
    {
        return (codon[0] == 'A' || codon[0] == 'C' || codon[0] == 'G' || codon[0] == 'T') &&
                (codon[1] == 'A' || codon[1] == 'C' || codon[1] == 'G' || codon[1] == 'T') &&
                (codon[2] == 'A' || codon[2] == 'C' || codon[2] == 'G' || codon[2] == 'T');
    }

    void UpdateDNASequence()
    {
        if (GetVisualDNASequence() == DNA.Sequence)
            return;

        int visual_codon_count = GetVisualDNASequence().Length / 3;
        int length_difference = DNA.CodonCount - visual_codon_count;

        //addition
        if (length_difference > 0)
        {
            int new_codon_index = visual_codon_count;

            //Could abstract this as everyone uses this same loop
            for (int i = 0; i < visual_codon_count; i++)
                if (DNA.GetCodon(i) != GetVisualCodon(i))
                {
                    new_codon_index = i;
                    break;
                }

            AddDNASequence(DNA.Sequence.Substring(new_codon_index * 3, +length_difference * 3), new_codon_index);
        }

        //replacement or move
        else if (length_difference == 0)
        {
            int first_change_index = -1,
                second_change_index = -1;

            for (int i = 0; i < visual_codon_count; i++)
                if (DNA.GetCodon(i) != GetVisualCodon(i))
                {
                    first_change_index = i;
                    break;
                }

            for (int i = visual_codon_count - 1; i >= 0; i--)
                if (DNA.GetCodon(i) != GetVisualCodon(i))
                {
                    second_change_index = i;
                    break;
                }

            //replacement
            if (first_change_index == second_change_index)
                CodonLayout.GetCodonElement(first_change_index).Codon = DNA.GetCodon(first_change_index);

            //move
            else
            {
                if (DNA.GetCodon(first_change_index) == GetVisualCodon(first_change_index + 1))
                    CodonLayout.AddCodonElement(CodonLayout.RemoveCodonElement(first_change_index), second_change_index);
                else
                    CodonLayout.AddCodonElement(CodonLayout.RemoveCodonElement(second_change_index), first_change_index);
            }
        }

        //removal
        else if (length_difference < 0)
        {
            int removed_codon_index = DNA.CodonCount;

            for (int i = 0; i < DNA.CodonCount; i++)
                if (DNA.GetCodon(i) != GetVisualCodon(i))
                {
                    removed_codon_index = i;
                    break;
                }

            for (int i = 0; i < -length_difference; i++)
                Destroy(CodonLayout.RemoveCodonElement(removed_codon_index).gameObject);
        }
    }

    public void ApplyChanges()
    {
        DNA.RemoveStrand(0, DNA.CodonCount);
        DNA.AddSequence(GetVisualDNASequence());
        DNA.ActiveCodonIndex = 0;

        Scene.Micro.Editor.Do();
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
        ApplyChanges();
    }

    public void AddDNASequenceElement(string sequence, string description)
    {
        DNASequenceElement dna_seqence_element = Instantiate(sequence_element_prefab);
        SequenceLayout.AddDNASequenceElement(dna_seqence_element);

        dna_seqence_element.transform.position = SpawnPosition;

        dna_seqence_element.DNASequence = sequence;
        dna_seqence_element.Description = description;
    }

    public static DNAPanel Create(Cell.Slot slot)
    {
        if (slot.Compound == null || !(slot.Compound.Molecule is DNA))
            return null;

        DNAPanel dna_panel = Instantiate(Scene.Micro.Prefabs.DNAPanel);
        dna_panel.transform.SetParent(Scene.Micro.Canvas.transform, false);

        Organism organism = slot.Cell.Organism;
        Vector2Int cell_position = slot.Cell.Organism.GetCellPosition(slot.Cell);
        int slot_index = slot.Index;

        dna_panel.DataFunction =
            delegate ()
            {
                Compound compound = organism.GetCell(cell_position).Slots[slot_index].Compound;
                if (compound == null)
                    return null;

                return compound.Molecule;
            };

        return dna_panel;
    }
}
