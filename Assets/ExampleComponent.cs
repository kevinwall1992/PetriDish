using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ExampleComponent : MonoBehaviour
{
    Example example;

    WaterLocale water_locale;
    OrganismComponent organism_component;

    int current_step = 0;

    float time_waited = 0;
    float time_to_wait = 0;
    bool Waiting
    {
        get
        {
            time_waited += Time.deltaTime;

            return time_waited < time_to_wait;
        }
    }

    public Example Example
    {
        get { return example; }

        set
        {
            example = value;

            if (example != null)
                ResetVisualization();
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        SetChildLayers();

        if (example == null)
            return;

        if (organism_component.IsVisualizingStep)
            return;

        if (Waiting)
            return;

        if (current_step >= example.Length)
        {
            ResetVisualization();
            return;
        }

        organism_component.BeginStepVisualization();
        current_step++;

        if (current_step >= example.Length)
            Wait(1.0f);
    }

    void SetChildLayers()
    {
        Stack<Transform> transforms = new Stack<Transform>();
        transforms.Push(transform);

        while(transforms.Count > 0)
        {
            Transform transform = transforms.Pop();
            transform.gameObject.layer = gameObject.layer;

            foreach (Transform child_transform in transform)
                transforms.Push(child_transform);
        }
    }

    void ResetVisualization()
    {
        if (water_locale == null)
            water_locale = new WaterLocale();

        if (organism_component != null)
            water_locale.RemoveOrganism(organism_component.Organism);
        else
        {
            organism_component = Instantiate(Scene.Micro.Prefabs.OrganismComponent);
            organism_component.transform.parent = transform;
        }

        organism_component.SetOrganism(example.Organism.Copy());
        water_locale.AddOrganism(organism_component.Organism);

        current_step = 0;

        Wait(0.5f);
    }

    void Wait(float time_to_wait_)
    {
        time_to_wait = time_to_wait_;
        time_waited = 0;
    }
}
