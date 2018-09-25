using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class World : MonoBehaviour
{
    static World the_world;

    public static World TheWorld
    {
        get { return the_world; }
    }


    List<OrganismComponent> active_organism_components = new List<OrganismComponent>(),
                            finished_organism_components = new List<OrganismComponent>();

    public DetailPane detail_pane;

    OrganismComponent[] GetOrganismComponents()
    {
        return Resources.FindObjectsOfTypeAll<OrganismComponent>();
    }

    private void Awake()
    {
        the_world = this;
    }

    void Start()
    {
        InitializeSequenceWidgets();   
    }

    void InitializeSequenceWidgets()
    {
        detail_pane.AddDNASequenceElement("CCCTAATAC", "Move Single Unit");
        detail_pane.AddDNASequenceElement("CAATAATAC", "Move Stack");
        detail_pane.AddDNASequenceElement("CATTCTTAA", "Cut and Paste DNA");
        detail_pane.AddDNASequenceElement("CACTAAAAC", "Activate Slot");
        detail_pane.AddDNASequenceElement("CAGTCTAAC", "Go To Marker");
        detail_pane.AddDNASequenceElement("CAGTCTGAGGAATAAGAATAC", "Conditionally Go To");

        detail_pane.AddDNASequenceElement("TCTTTT", "Marked Group");

        detail_pane.AddDNASequenceElement("GAATAA", "Get Size of Slot");
        detail_pane.AddDNASequenceElement("GAGTAATAC", "A == B");
        detail_pane.AddDNASequenceElement("GACTAATAC", "A > B");
        detail_pane.AddDNASequenceElement("GATTAATAC", "A < B");

        detail_pane.AddDNASequenceElement("TAA", "Slot 1");

        detail_pane.AddDNASequenceElement("AAA", "0");

        detail_pane.AddDNASequenceElement("TCTACCGGAATCGGCTTT", "Interpretase");
    }

    void Update()
    {
        if (detail_pane.gameObject.activeSelf)
            return;

        if (active_organism_components.Count > 0)
            return;

        OrganismComponent[] organism_components = GetOrganismComponents();
        if (finished_organism_components.Count== organism_components.Length)
        {
            finished_organism_components.Clear();
            return;
        }

        foreach(OrganismComponent organism_component in organism_components)
            if(!finished_organism_components.Contains(organism_component))
            {
                active_organism_components.Add(organism_component);
                organism_component.TakeTurn();
                return;
            }
    }

    public OrganismComponent GetOrganismComponent(Organism organism)
    {
        foreach (OrganismComponent organism_component in GetOrganismComponents())
            if (organism_component.Organism == organism)
                return organism_component;

        return null;
    }

    public bool IsTakingTurn(OrganismComponent organism_component)
    {
        return active_organism_components.Contains(organism_component);
    }

    public void FinishTurn(OrganismComponent organism_component)
    {
        active_organism_components.Remove(organism_component);
        finished_organism_components.Add(organism_component);
    }
}
