using UnityEngine;
using System.Collections;

public class Scene : GoodBehavior
{
    static MicroScene micro_scene = null;

    public static MicroScene Micro
    {
        get
        {
            if(micro_scene == null)
                micro_scene = FindObjectOfType<MicroScene>();

            return micro_scene;
        }
    }
}
