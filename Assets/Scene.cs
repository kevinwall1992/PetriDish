using UnityEngine;
using System.Collections;

public class Scene : GoodBehavior
{
    public static MicroScene Micro { get { return FindObjectOfType<MicroScene>(); } }
}
