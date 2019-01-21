using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

//This makes assumption that the Spawner script and
//this script are both attached the object being dragged. 
public class SpawnOnDragBehavior : GoodBehavior
{
    public Spawner Spawner { get { return GetComponent<Spawner>(); } }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        Scene.Micro.InputModule.SwapDrag(gameObject, Spawner.Spawn());
    }
}

public interface Spawner
{
    GameObject Spawn();
}
