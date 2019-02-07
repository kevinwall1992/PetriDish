using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class UndoButton : ScalingButton
{
    [SerializeField]
    bool redo_instead = false;

    bool IsAvailable
    {
        get
        {
            return redo_instead ? Scene.Micro.Editor.CanRedo : 
                                  Scene.Micro.Editor.CanUndo;
        }
    }

    protected override void Update()
    {
        IsInteractable = IsAvailable;

        base.Update();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        if(redo_instead)
            Scene.Micro.Editor.Redo();
        else
            Scene.Micro.Editor.Undo();
    }
}
