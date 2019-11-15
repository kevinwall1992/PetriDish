using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

//****Need arrow to indicate code being executed
public class DNAPanel : DetailPanel
{
    public DNA DNA
    {
        get
        {
            Molecule molecule = Data as Molecule;
            if (molecule == null)
                return null;

            if (molecule is Catalyst)
                return Interpretase.GetGeneticCofactor(molecule as Catalyst);

            if (molecule is DNA)
                return molecule as DNA;

            return null;
        }
    }

    Program program = null;
    public Program Program
    {
        get
        {
            if(program == null)
                program = Scene.Micro.Visualization.OrganismComponent.Organism.GetProgram(DNA.Sequence);

            return program;
        }
    }

    public SectorNode SectorNode { get; private set; }

    private void Awake()
    {
        
    }

    protected override void Start()
    {
        SectorNode = SectorNode.CreateInstance(Program.MainSector);
        SectorNode.transform.SetParent(transform, false);
        SectorNode.IsCollapsed = false;

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

        string program_dna_sequence = Program.GenerateDNASequence();
        if (DNA.Sequence != program_dna_sequence)
        {
            int active_codon_index = DNA.ActiveCodonIndex;
            DNA.RemoveSequence(0, DNA.CodonCount);
            DNA.AppendSequence(program_dna_sequence);
            DNA.ActiveCodonIndex = active_codon_index;

            Scene.Micro.Editor.Do();
        }

        base.Update();
    }
   
    public static DNAPanel Create(Cell.Slot slot)
    {
        if (slot.Compound == null || !(slot.Compound.Molecule is DNA))
            return null;

        DNAPanel dna_panel = Instantiate(Scene.Micro.Prefabs.DNAPanel);
        dna_panel.transform.SetParent(Scene.Micro.DetailPanelContainer, false);

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
