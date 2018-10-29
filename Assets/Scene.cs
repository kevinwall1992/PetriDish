using UnityEngine;
using System.Collections;

public static class Scene
{
    public static class Micro
    {
        public static bool IsActive { get { return Visualization != null; } }

        public static MicroVisualization Visualization
        {
            get { return Object.FindObjectOfType<MicroVisualization>(); }
        }
    }
}
