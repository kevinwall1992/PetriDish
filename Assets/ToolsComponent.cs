using UnityEngine;
using System.Collections;

public class ToolsComponent : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
            Tools.Run();
    }
}
