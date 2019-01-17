using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;


public class DetailPanel : GoodBehavior
{
    static DetailPanel opened_detail_panel = null;

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

    public bool IsOpen { get { return opened_detail_panel == this; } }

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        
    }

    private void OnGUI()
    {
        if (Utility.ConsumeIsKeyUp(KeyCode.Escape))
            Close();
    }

    public virtual void Open()
    {
        if (opened_detail_panel != null)
            opened_detail_panel.Close();

        gameObject.SetActive(true);
        opened_detail_panel = this;

        Scene.Micro.Visualization.IsPaused = true;
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
        opened_detail_panel = null;
    }
}