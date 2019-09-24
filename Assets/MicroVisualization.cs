using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MicroVisualization : GoodBehavior
{
    WaterLocale water_locale;

    bool take_one_step = false;

    public OrganismComponent OrganismComponent { get; private set; }

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

    public bool IsVisualizingStep { get; private set; }

    public float Speed { get; set; }

    private void Awake()
    {
        water_locale = WaterLocale.CreateVentLocale();
    }

    protected override void Start()
    {
        base.Start();

        OrganismComponent = GetComponentInChildren<OrganismComponent>();

        OrganismComponent.LoadOrganism("Organisms/test_organism");
        water_locale.AddOrganism(OrganismComponent.Organism);

        Scene.Micro.Editor.TrackThis(OrganismComponent.Organism);

        Speed = 1.0f;
    }

    protected override void Update()
    {
        base.Update();

        float scroll_speed = 3.5f;
        if (Input.GetKey(KeyCode.UpArrow))
            Scene.Micro.Camera.transform.Translate(new Vector3(0, scroll_speed * Time.deltaTime));
        if (Input.GetKey(KeyCode.DownArrow))
            Scene.Micro.Camera.transform.Translate(new Vector3(0, -scroll_speed * Time.deltaTime));
        if (Input.GetKey(KeyCode.LeftArrow))
            Scene.Micro.Camera.transform.Translate(new Vector3(-scroll_speed * Time.deltaTime, 0));
        if (Input.GetKey(KeyCode.RightArrow))
            Scene.Micro.Camera.transform.Translate(new Vector3(scroll_speed * Time.deltaTime, 0));

        if (OrganismComponent.IsVisualizingStep)
            return;

        if (IsVisualizingStep)
        {
            IsVisualizingStep = false;

            Scene.Micro.Editor.Do();
        }
        else if (Input.GetKey(KeyCode.RightBracket))
            FileUtility.Save(OrganismComponent.Organism, "Organisms/saved_organism.json");

        if (IsPaused && !take_one_step)
            return;

        OrganismComponent.BeginStepVisualization();
        IsVisualizingStep = true;

        take_one_step = false;
    }

    public void TakeOneStep()
    {
        IsPaused = true;

        take_one_step = true;
    }

    void SwitchOrganism(int relative_index)
    {
        int index = MathUtility.Mod(water_locale.Organisms.IndexOf(OrganismComponent.Organism) + relative_index, 
                                    water_locale.Organisms.Count);

        OrganismComponent.SetOrganism(water_locale.Organisms[index]);
    }

    public void NextOrganism()
    {
        SwitchOrganism(1);
    }

    public void PreviousOrganism()
    {
        SwitchOrganism(-1);
    }
}
