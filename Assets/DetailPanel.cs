using UnityEngine;

public class DetailPanel : GoodBehavior
{
    public static DetailPanel Left { get; private set; }
    public static DetailPanel Right { get; private set; }

    enum Position { Left, Right }
    [SerializeField]
    Position position;

    System.Func<object> data_function;
    public System.Func<object> DataFunction
    {
        get { return data_function; }

        set
        {
            if (data_function == null)
                data_function = value;
            else
                Debug.Assert(false, "DetailPane.DataFunction may not be set more than once");
        }
    }

    public virtual object Data
    {
        get { return DataFunction(); }
        set { DataFunction = () => (value); }
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