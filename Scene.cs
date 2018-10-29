using UnityEngine;
using System.Collections;

public static class Scenes
{
    public static class Micro
    {
        public static bool IsMicroActive { get { return Visualization != null; } }

        public static MicroVisualization Visualization
        {
            get { return Object.FindObjectOfType<MicroVisualization>(); }
        }
    }
}
