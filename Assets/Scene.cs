using UnityEngine;
using System.Collections;

public static class Scene
{
    public static class Micro
    {
        public static bool IsActive { get { return Visualization != null; } }

        public static Canvas Canvas
        {
            get { return Object.FindObjectOfType<Canvas>(); }
        }

        public static MicroVisualization Visualization
        {
            get { return Object.FindObjectOfType<MicroVisualization>(); }
        }

        public static MicroPrefabs Prefabs
        {
            get { return Object.FindObjectOfType<MicroPrefabs>(); }
        }
    }
}
