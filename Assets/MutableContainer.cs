using System.Collections.Generic;

public interface IMutableContainer<T>
{
    IEnumerable<T> Items { get; }

    bool Contains(T element);

    void AddItem(T item);
    T RemoveItem(T item);

    bool WasModified(object stakeholder);
}

public abstract class MutableContainer<T> : IMutableContainer<T>
{
    Dictionary<object, bool> stakeholders = new Dictionary<object, bool>();

    public abstract IEnumerable<T> Items { get; }

    public bool Contains(T item)
    {
        return Utility.Contains(Items, item);
    }

    public abstract void AddItem(T item);
    public abstract T RemoveItem(T item);

    protected void Modify()
    {
        List<object> keys = new List<object>(stakeholders.Keys);

        foreach (object stakeholder in keys)
            stakeholders[stakeholder] = true;
    }

    public bool WasModified(object stakeholder)
    {
        if (!stakeholders.ContainsKey(stakeholder))
            stakeholders[stakeholder] = true;

        bool was_modified = stakeholders[stakeholder];
        stakeholders[stakeholder] = false;

        return was_modified;
    }
}
