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

        GameObject drag_target = Spawner.Spawn();

        if(drag_target != null)
            Scene.Micro.InputModule.SwapDrag(gameObject, drag_target);
    }
}

public interface Spawner
{
    GameObject Spawn();
}
