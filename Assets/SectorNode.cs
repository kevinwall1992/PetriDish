using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public class SectorNode : DNAPanelNode
{
    [SerializeField]
    Text icon_text;

    [SerializeField]
    InputField name_input_field, description_input_field;

    [SerializeField]
    SectorNodeBackgroundStrand background_strand_prefab;

    [SerializeField]
    SectorNodeInsertionButton insertion_button_prefab;

    [SerializeField]
    RectTransform node_container,
                  background_strand_container,
                  insertion_button_container,
                  grabbed_node_container,
                  child_sector_node_container;

    [SerializeField]
    SectorNodeInsertionChoice insertion_choice;

    [SerializeField]
    SectorNodeSelectionBox selection_box;

    List<DNAPanelNode> node_order = new List<DNAPanelNode>();
    DummyNode dummy_node;

    DNAPanelNode grabbed_node = null;
    string grabbed_dna_sequence = "";

    SectorNode child_sector_node = null;

    float strand_scroll_position = 0;
    int row_count = 0;

    bool is_blank = true;

    public DNAPanel DNAPanel { get { return GetComponentInParent<DNAPanel>(); } }

    public DNA.Sector Sector { get; private set; }

    public override int CodonLength
    {
        get { return Sector.LastCodonIndex - Sector.FirstCodonIndex + 1; }
    }

    public override string DNASequence
    {
        get
        {
            if(IsCollapsed)
                return base.DNASequence;

            string dna_sequence = "";

            foreach (DNAPanelNode node in node_order)
                dna_sequence += node.DNASequence;

            return dna_sequence;
        }
    }

    public override bool IsCollapsed
    {
        set
        {
            if (IsCollapsed == value)
                return;

            base.IsCollapsed = value;

            if (!IsCollapsed)
            {
                RectTransform dna_panel_rect_transform = DNAPanel.transform as RectTransform;
                (transform as RectTransform).sizeDelta = new Vector2(dna_panel_rect_transform.rect.width,
                                                                     dna_panel_rect_transform.rect.height);
                if (Depth == 0)
                    transform.position = dna_panel_rect_transform.position;

                name_input_field.text = Sector.Name;
                description_input_field.text = Sector.Description;

                GetComponent<Image>().raycastTarget = false;

                Reload();
            }
            else
                GetComponent<Image>().raycastTarget = true;
        }
    }

    DNAPanelNode selection_start_node, selection_stop_node;
    public bool IsSelecting { get; private set; }

    public float Scale { get { return (transform as RectTransform).rect.height / 594.0f; } }
    public float Spacing { get { return 10 * Scale; } }
    public float LerpSpeed { get { return 3; } }

    public int Depth
    {
        get
        {
            SectorNode parent = transform.parent.GetComponentInParent<SectorNode>();
            if (parent == null)
                return 0;

            return parent.Depth + 1;
        }
    }

    public int MaxDepth
    {
        get
        {
            foreach (SectorNode sector_node in GetComponentsInChildren<SectorNode>())
                if (sector_node!= this && !sector_node.IsCollapsed)
                    return sector_node.MaxDepth;

            return Depth;
        }
    }

    public bool IsOnScreen
    {
        get
        {
            RectTransform rect_transform = transform as RectTransform;
            RectTransform dna_panel_rect_transform = DNAPanel.transform as RectTransform;

            return (rect_transform.position.y - rect_transform.rect.height / 2) <
                   (dna_panel_rect_transform.position.y + dna_panel_rect_transform.rect.height);
        }
    }

    void Start()
    {
        name_input_field.onValueChanged.AddListener(
            delegate
            {
                Sector.Name = name_input_field.text;
                Scene.Micro.Editor.Do();
            });

        description_input_field.onValueChanged.AddListener(
            delegate
            {
                Sector.Description = description_input_field.text;
                Scene.Micro.Editor.Do();
            });
    }

    protected override void Update()
    {
        base.Update();

        if (IsCollapsed)
        {
            icon_text.text = Sector.Name;
            return;
        }


        //Positioning of SectorNode hierarchy
        if (Depth == 0)
            transform.position =
                Vector3.Lerp(transform.position,
                             DNAPanel.transform.position + new Vector3(0, (transform as RectTransform).rect.height * MaxDepth),
                             Time.deltaTime * LerpSpeed);


        //Positioning of child SectorNode
        if (Depth != MaxDepth)
        {
            foreach (Transform child in node_container)
            {
                SectorNode sector_node = child.GetComponent<SectorNode>();

                if (sector_node != null && sector_node != this && !sector_node.IsCollapsed)
                    child_sector_node = sector_node;
            }

            if (child_sector_node.transform.parent == node_container)
                child_sector_node.transform.SetParent(child_sector_node_container, true);

            child_sector_node.transform.position = 
                Vector3.Lerp(child_sector_node.transform.position,
                             transform.position - new Vector3(0, (transform as RectTransform).rect.height),
                             Time.deltaTime * LerpSpeed);

            //if (!IsOnScreen && !is_blank)
            //    Clear();

            return;
        }


        //Check if DNA has changed (generally, due to Undo/Redo)
        if(Sector.DNA != DNAPanel.DNA)
        {
            foreach (DNA.Sector sector in DNAPanel.DNA.Sectors)
                if (Sector.Identity == sector.Identity)
                    Sector = sector;

            if (Sector.DNA != DNAPanel.DNA)
                IsCollapsed = true;
            else
            {
                bool must_reload = false;

                if (Sector.Sequence != DNASequence)
                    must_reload = true;
                else
                {
                    foreach (DNAPanelNode node in node_order)
                    {
                        if (Sector.DNA.GetSector(node.CodonIndex) != Sector)
                            must_reload = true;
                        else if (node is SectorNode && (node as SectorNode).Sector.DNA != DNAPanel.DNA)
                            must_reload = true;
                    }
                }

                if (must_reload)
                    Reload();
            }
        }


        //Update name, description
        if (name_input_field.text != Sector.Name)
            name_input_field.text = Sector.Name;
        if (description_input_field.text != Sector.Description)
            description_input_field.text = Sector.Description;


        //Grabbed node stuff
        if (Input.GetMouseButtonDown(0))
        {
            foreach (DNAPanelNode node in node_container.GetComponentsInChildren<DNAPanelNode>())
                if (node.IsPointedAt)
                {
                    grabbed_node = node;
                    grabbed_dna_sequence = node.DNASequence;
                    break;
                }
        }
        if (grabbed_node != null)
        {
            if (!node_order.Contains(grabbed_node))
                grabbed_node.transform.position = Vector3.Lerp(grabbed_node.transform.position,
                                                               Input.mousePosition,
                                                               Time.deltaTime * LerpSpeed * 3);
            else
                strand_scroll_position += grabbed_node.transform.position.x - Input.mousePosition.x;

            if (Input.GetMouseButtonUp(0))
            {
                if (!node_order.Contains(grabbed_node))
                {
                    DNAPanelNode reference_node = GetReferenceNodeFromMousePosition();
                    if (!IsPointedAt)
                        Destroy(grabbed_node.gameObject);
                    else
                    {
                        DNAPanel.DNA.InsertSequence(reference_node.CodonIndex, grabbed_dna_sequence);
                        InsertNodeBefore(reference_node, grabbed_node);
                    }
                }

                grabbed_node = null;
            }
        }


        //Positioning of DNANodes
        float scroll_speed = 100;
        if (Input.GetKey(KeyCode.Comma))
            strand_scroll_position += scroll_speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.Period))
            strand_scroll_position -= scroll_speed * Time.deltaTime;
        if (strand_scroll_position < 0)
            strand_scroll_position = 0;

        float sector_height = (transform as RectTransform).rect.height;
        float mask_width = (node_container.transform.parent as RectTransform).rect.width;
        float initial_horizontal_offset = Spacing * 2;

        int row = 0;
        float horizontal_offset = initial_horizontal_offset - strand_scroll_position;

        if (child_sector_node != null)
        {
            child_sector_node.transform.SetParent(node_container, transform);
            child_sector_node = null;
        }

        foreach (DNAPanelNode node in new List<DNAPanelNode>(node_order))
        {
            Vector2 local_mouse_position = node.transform.parent.InverseTransformPoint(Input.mousePosition);

            if(insertion_choice.gameObject.activeSelf && node == insertion_choice.ReferenceNode)
            {
                float insertion_choice_width = (insertion_choice.transform as RectTransform).rect.width + Spacing;

                if ((horizontal_offset + insertion_choice_width) > mask_width)
                {
                    row++;
                    horizontal_offset = initial_horizontal_offset;
                }

                insertion_choice.transform.position = 
                    Vector3.Lerp(insertion_choice.transform.position,
                                 node_container.transform.position + new Vector3(horizontal_offset + insertion_choice_width / 2,
                                                                                -row * 88 * Scale),
                                 Time.deltaTime * LerpSpeed);

                horizontal_offset += insertion_choice_width;
            }

            CompoundTile dragged_compound_tile = GoodBehavior.DraggedElement as CompoundTile;
            if (((grabbed_node != null && !node_order.Contains(grabbed_node)) || 
                (dragged_compound_tile != null && dragged_compound_tile.Compound.Molecule is Catalyst)) && 
                (Input.mousePosition.y - node.transform.position.y) < Spacing * 4 && 
                GetReferenceNodeFromMousePosition() == node)
                horizontal_offset += Spacing * 4;

            float width = (node.transform as RectTransform).rect.width + Spacing;

            if ((horizontal_offset + width) > mask_width)
            {
                row++;
                horizontal_offset = initial_horizontal_offset;
            }

            Vector2 target_position = new Vector2(horizontal_offset + width / 2,
                                                  -row * 88 * Scale);
            float adjusted_lerp_speed = LerpSpeed;

            if (node == grabbed_node)
            {
                adjusted_lerp_speed *= 3;

                if ((node.transform.localPosition.y - target_position.y) > 18)
                {
                    DNAPanel.DNA.RemoveSequence(grabbed_node.CodonIndex, grabbed_node.CodonLength);
                    RemoveNode(node, false);
                    node.transform.SetParent(grabbed_node_container, true);
                    continue;
                }
                else
                {
                    float target_weight = 0 + (target_position - (Vector2)node.transform.localPosition).magnitude / 1.75f;
                    target_position = (target_position * target_weight + local_mouse_position * 4) / (4 + target_weight);
                }
            }

            int node_index = node_order.IndexOf(node);
            if ((node_index > 0 && node_order[node_index - 1] == grabbed_node) || 
                ((node_index < node_order.Count - 1) && node_order[node_index + 1] == grabbed_node))
                target_position = (target_position * 8 + local_mouse_position) / (1 + 8);


            node.transform.localPosition = Vector2.Lerp(node.transform.localPosition, 
                                                       target_position, 
                                                       Time.deltaTime * adjusted_lerp_speed);

            horizontal_offset += width;
        }


        //Background strand length and number of rows determination
        if (row != (row_count - 1))
        {
            foreach (SectorNodeBackgroundStrand background_strand in GetComponentsInChildren<SectorNodeBackgroundStrand>())
                Destroy(background_strand.gameObject);

            row_count = row + 1;

            for (int i = 0; i < row_count; i++)
            {
                SectorNodeBackgroundStrand background_strand = Instantiate(background_strand_prefab);
                if (i < row)
                    background_strand.ShowReturnStrand();

                background_strand.transform.SetParent(background_strand_container, false);

                background_strand.transform.localPosition = new Vector3(0, -88 * i * Scale);

                background_strand.Length = mask_width;
            }
        }

        SectorNodeBackgroundStrand[] background_strands = GetComponentsInChildren<SectorNodeBackgroundStrand>();

        foreach (SectorNodeBackgroundStrand background_strand in background_strands)
            background_strand.HorizontalOffset = -strand_scroll_position; /*Mathf.Lerp(background_strand.HorizontalOffset, 
                                                            -strand_scroll_position, 
                                                            Time.deltaTime * LerpSpeed);*/

        if (background_strands.Length > 0)
            background_strands[background_strands.Length - 1].Length = Mathf.Lerp(background_strands[row_count - 1].Length, 
                                                                                  horizontal_offset - Spacing, 
                                                                                  Time.deltaTime * LerpSpeed);


        //Selection
        if (Input.GetMouseButtonUp(0))
        {
            IsSelecting = false;

            if (selection_start_node == selection_stop_node)
                selection_start_node = selection_stop_node = null;
        }

        if (selection_start_node != null)
        {
            selection_box.gameObject.SetActive(true);

            if (IsSelecting)
            {
                selection_stop_node = GetReferenceNodeFromMousePosition();
                if (node_order.IndexOf(selection_start_node) > node_order.IndexOf(selection_stop_node))
                    selection_stop_node = selection_start_node;
            }

            foreach (SectorNodeInsertionButton insertion_button in GetComponentsInChildren<SectorNodeInsertionButton>())
                if (node_order.IndexOf(insertion_button.DNAPanelNode) >= node_order.IndexOf(selection_start_node) &&
                    node_order.IndexOf(insertion_button.DNAPanelNode) < node_order.IndexOf(selection_stop_node))
                    insertion_button.IsHighlighted = true;
                else
                    insertion_button.IsHighlighted = false;
        }
        else
        {
            selection_box.gameObject.SetActive(false);

            foreach (SectorNodeInsertionButton insertion_button in GetComponentsInChildren<SectorNodeInsertionButton>())
                insertion_button.IsHighlighted = false;
        }


        
    }

    void Clear()
    {
        foreach (DNAPanelNode node in node_container.GetComponentsInChildren<DNAPanelNode>())
            RemoveNode(node);

        foreach (SectorNodeBackgroundStrand background_strand in GetComponentsInChildren<SectorNodeBackgroundStrand>())
            Destroy(background_strand.gameObject);

        is_blank = true;
    }

    void Reload()
    {
        if (!is_blank)
            Clear();

        InsertNodeBefore(null, dummy_node = DummyNode.CreateInstance());

        IntegrateNewDNASequence(dummy_node, CodonLength);

        foreach (DNAPanelNode node in node_container.GetComponentsInChildren<DNAPanelNode>())
            node.transform.position = transform.position;
    }

    public void AddNode(DNAPanelNode node)
    {
        InsertNodeBefore(dummy_node, node);
    }

    public void InsertNodeBefore(DNAPanelNode reference_node, DNAPanelNode node)
    {
        node.transform.SetParent(node_container);
        node.transform.position = Input.mousePosition;
        node_order.Insert(reference_node != null ? node_order.IndexOf(reference_node) : node_order.Count, node);

        SectorNodeInsertionButton insertion_button = Instantiate(insertion_button_prefab);
        insertion_button.DNAPanelNode = node as DNAPanelNode;
        insertion_button.transform.SetParent(insertion_button_container, false);

        is_blank = false;
    }

    public void RemoveNode(DNAPanelNode node, bool destroy_node = true)
    {
        node_order.Remove(node);
        node.transform.SetParent(null);
        if(destroy_node)
            Destroy(node.gameObject);

        foreach(SectorNodeInsertionButton insertion_button in transform.GetComponentsInChildren<SectorNodeInsertionButton>())
            if(insertion_button.DNAPanelNode == node)
                Destroy(insertion_button.gameObject);
    }

    void IntegrateNewDNASequence(DNAPanelNode reference_node, int length)
    {
        int codon_index = reference_node.CodonIndex;
        int last_codon_index = reference_node.CodonIndex + length - 1;

        while (codon_index <= last_codon_index)
        {
            DNAPanelNode node = null;

            if (Sector.DNA.GetSector(codon_index) == Sector)
            {
                string codon = Sector.DNA.GetCodon(codon_index);

                switch (codon[0])
                {
                    case 'V':
                    case 'F':
                        node = CatalystNode.CreateInstance(Interpretase.GetCatalyst(Sector.DNA, codon_index));
                        break;

                    case 'C':
                        node = CommandNode.CreateInstance(
                            Sector.DNA.GetSubsequence(codon_index, Interpretase.GetOperandCount(Sector.DNA, codon_index) + 1));

                        break;

                    case 'L':
                        node = LocusNode.CreateInstance(codon);

                        break;

                    default:
                        Image image = new GameObject("error_image").AddComponent<Image>();
                        image.sprite = Resources.Load<Sprite>("error");
                        image.transform.SetParent(node_container);

                        break;
                }
            }
            else
                node = SectorNode.CreateInstance(Sector.DNA.GetSector(codon_index));

            InsertNodeBefore(reference_node, node);
            codon_index += node.CodonLength;
        }
    }

    public void AddDNASequence(string dna_sequence)
    {
        InsertDNASequence(dna_sequence, dummy_node);
    }

    public void InsertDNASequence(string dna_sequence, DNAPanelNode reference_node = null)
    {
        if (reference_node == null)
            reference_node = GetReferenceNodeFromMousePosition();

        Sector.DNA.InsertSequence(reference_node.CodonIndex, dna_sequence);
        Scene.Micro.Editor.Do();

        IntegrateNewDNASequence(reference_node, dna_sequence.Length / 3);
    }

    public void BeginSelect(DNAPanelNode node)
    {
        selection_start_node = node;
        IsSelecting = true;
    }

    public void MakeSectorFromSelection()
    {
        int first_codon_index = selection_start_node.CodonIndex;
        int last_codon_index = selection_stop_node.CodonIndex - 1;

        DNA.Sector new_sector = Sector.DNA.AddSector("Unnamed Sector", "", first_codon_index, last_codon_index);
        Scene.Micro.Editor.Do();

        SectorNode sector_node = SectorNode.CreateInstance(new_sector);
        InsertNodeBefore(selection_start_node, sector_node);


        foreach(DNAPanelNode node in GetSelectedDNANodes())
            RemoveNode(node);


        selection_start_node = null;
    }

    public void CopySelection()
    {
        string dna_sequence = "";

        foreach(DNAPanelNode node in GetSelectedDNANodes())
            dna_sequence += node.DNASequence;

        GUIUtility.systemCopyBuffer = dna_sequence;
    }

    public void DeleteSelection()
    {
        foreach (DNAPanelNode node in GetSelectedDNANodes())
        {
            Sector.DNA.RemoveSequence(node.CodonIndex, node.CodonLength);
            Scene.Micro.Editor.Do();

            RemoveNode(node);
        }

        selection_start_node = null;
    }

    public void ShowInsertionChoice(DNAPanelNode reference_node)
    {
        CollapseAllNodes();

        insertion_choice.gameObject.SetActive(true);
        insertion_choice.ReferenceNode = reference_node;
    }

    public void HideInsertionChoice()
    {
        insertion_choice.gameObject.SetActive(false);
    }

    public int NodeToCodonIndex(DNAPanelNode node)
    {
        int offset = 0;
        foreach(DNAPanelNode other_node in node_order)
        {
            if (other_node == node)
                break;

            offset += other_node.CodonLength;
        }

        return Sector.FirstCodonIndex + offset;
    }

    List<DNAPanelNode> GetSelectedDNANodes()
    {
        List<DNAPanelNode> selected_nodes = new List<DNAPanelNode>();

        int first_index = node_order.IndexOf(selection_start_node);
        int last_index = node_order.IndexOf(selection_stop_node) - 1;

        foreach (DNAPanelNode node in node_order)
        {
            int index = node_order.IndexOf(node);

            if (index >= first_index && index <= last_index)
                selected_nodes.Add(node);
        }

        return selected_nodes;
    }

    DNAPanelNode GetReferenceNodeFromMousePosition()
    {
        Vector2 position = Input.mousePosition;

        System.Func<SectorNodeInsertionButton, float> Distance = (insertion_button) =>
                    (((Vector2)insertion_button.transform.position - position).magnitude);

        SectorNodeInsertionButton[] insertion_buttons = GetComponentsInChildren<SectorNodeInsertionButton>();
        System.Array.Sort(insertion_buttons, (a, b) => (Distance(a).CompareTo(Distance(b))));

        return insertion_buttons[0].DNAPanelNode;
    }

    public void CollapseAllNodes(DNAPanelNode except = null)
    {
        foreach (DNAPanelNode node in GetComponentsInChildren<DNAPanelNode>())
            if (node != this && node != except)
                node.IsCollapsed = true;
    }

    public SectorNode GetDeepestVisibleSectorNode()
    {
        if (MaxDepth == Depth)
            return this;

        return child_sector_node.GetDeepestVisibleSectorNode();
    }


    public static SectorNode CreateInstance(DNA.Sector sector)
    {
        SectorNode sector_node = Instantiate(Scene.Micro.Prefabs.SectorNode);
        sector_node.Sector = sector;

        return sector_node;
    }


    class DummyNode : DNAPanelNode
    {
        public override int CodonLength { get { return 0; } }

        public override string DNASequence
        {
            get { return ""; }
        }

        public static DummyNode CreateInstance()
        {
            DummyNode dummy_node = new GameObject().AddComponent<DummyNode>();
            (dummy_node.collapsed_form = dummy_node.expanded_form = 
                dummy_node.gameObject.AddComponent<RectTransform>()).sizeDelta = new Vector2(1, 1);

            return dummy_node;
        }
    }
}
