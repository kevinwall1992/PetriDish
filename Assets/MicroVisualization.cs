﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MicroVisualization : MonoBehaviour
{
    Camera camera;

    OrganismComponent organism_component;

    WaterLocale water_locale;

    bool is_paused = true;
    public bool IsPaused
    {
        get { return is_paused; }

        set
        {
            if (is_paused == value)
                return;

            if(is_paused)
            {
                is_paused = false;
            }
            else
            {
                is_paused = true;
            }
        }
    }

    public List<OrganismComponent> OrganismComponents
    {
        get { return Utility.CreateList(organism_component); }
    }

    private void Awake()
    {
        camera = GameObject.Find("Main Camera").GetComponent<Camera>();

        water_locale = WaterLocale.CreateVentLocale();
    }

    void Start()
    {
        organism_component = GetComponentInChildren<OrganismComponent>();
        water_locale.AddOrganism(organism_component.Organism);

        organism_component.ResetExperiment("CACACAAATTCT" + Ribozyme.GetRibozymeFamily("Rotase")[0].Sequence + "TTTCATTCTAAGTGACAAACAAACCAGTGAGACGAAACAAAATTT");
    }

    void Update()
    {
        float scroll_speed = 3.5f;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            camera.transform.Translate(new Vector3(0, scroll_speed * Time.deltaTime));
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            camera.transform.Translate(new Vector3(0, -scroll_speed * Time.deltaTime));
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            camera.transform.Translate(new Vector3(-scroll_speed * Time.deltaTime, 0));
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            camera.transform.Translate(new Vector3(scroll_speed * Time.deltaTime, 0));

        if (Input.GetKey(KeyCode.Space))
            IsPaused = !IsPaused;

        if (IsPaused)
            return;

        if (organism_component.IsVisualizingStep)
            return;

        organism_component.BeginStepVisualization();
    }

    public OrganismComponent GetOrganismComponent(Organism organism)
    {
        return organism_component;
    }
}
