using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;


public class DetailPanel : GoodBehavior
{
    public static DetailPanel Left { get; private set; }
    public static DetailPanel Right { get; private set; }

    enum Position { Left, Right }
    [SerializeField]
    Position position;

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

    public bool IsOpen { get { return position == Position.Left ? Left == this : 
                                                                  Right == this; } }

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
        if (position == Position.Left)
        {
            if (Left != null)
                Left.Close();

            Left = this;
        }
        else
        {
            if (Right != null)
                Right.Close();

            Right = this;
        }

        gameObject.SetActive(true);
        

        Scene.Micro.Visualization.IsPaused = true;
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);

        if (position == Position.Left)
            Left = null;
        else
            Right = null;
    }
}