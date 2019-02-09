using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MicroEditor : MonoBehaviour, IDoer
{
    List<IDoer> doers = new List<IDoer>();

    public bool CanUndo
    {
        get
        {
            foreach (IDoer doer in doers)
                if (doer.CanUndo)
                    return true;

            return false;
        }
    }

    public bool CanRedo
    {
        get
        {
            foreach (IDoer doer in doers)
                if (doer.CanRedo)
                    return true;

            return false;
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyUp(KeyCode.Z))
                Undo();
            else if (Input.GetKeyUp(KeyCode.Y))
                Redo();
        }
    }

    public void TrackThis<T>(Versionable<T> versionable) where T : Versionable<T>
    {
        doers.Add(new Doer<T>(versionable));
    }

    public void Do()
    {
        foreach (IDoer doer in doers) doer.Do();
    }

    public void Undo()
    {
        foreach (IDoer doer in doers) doer.Undo();
    }

    public void Redo()
    {
        foreach (IDoer doer in doers) doer.Redo();
    }
}
