using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;


public class DetailPanel : GoodBehavior
{
    object data;
    public virtual object Data
    {
        get { return data; }

        set
        {
            if (data == null)
                data = value;
            else
                Debug.Assert(false, "DetailPane.Data may not be set more than once");
        }
    }

    protected virtual void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            gameObject.SetActive(false);
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }
}

public interface HasDetailPanel
{
    DetailPanel DetailPanel { get; }
}