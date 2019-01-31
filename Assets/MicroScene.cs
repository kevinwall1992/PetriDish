using UnityEngine;
using System.Collections;

public class MicroScene : Scene
{
    public Canvas Canvas { get { return canvas; } }

    public MicroVisualization Visualization { get { return visualization; } }

    public MicroPrefabs Prefabs { get { return prefabs; } }

    public Camera Camera { get { return camera; } }

    public ExampleComponent ExampleComponent { get { return example_component; } }

    public MicroInputModule InputModule { get { return input_module; } }

    public TrashcanButton TrashcanButton { get { return trashcan_button; } }


    public bool IsBackgroundPointedAt { get { return background.GetComponentInChildren<GoodBehavior>().IsPointedAt; } }


    [SerializeField]
    Canvas canvas;

    [SerializeField]
    Canvas background;

    [SerializeField]
    MicroVisualization visualization;

    [SerializeField]
    MicroPrefabs prefabs;

    [SerializeField]
    Camera camera;

    [SerializeField]
    ExampleComponent example_component;

    [SerializeField]
    MicroInputModule input_module;

    [SerializeField]
    TrashcanButton trashcan_button;
}
