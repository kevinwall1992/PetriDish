using System.Collections.Generic;

public interface IMutableContainer<T>
{
    List<T> Elements { get; }

    bool Contains(T element);

    void AddElement(T element);
    T RemoveElement(T element);

    bool WasModified(object stakeholder);
}

public abstract class MutableContainer<T> : IMutableContainer<T>
{
    Dictionary<object, bool> stakeholders = new Dictionary<object, bool>();

    public abstract List<T> Elements { get; }

    public bool Contains(T element)
    {
        return Elements.Contains(element);
    }

    public abstract void AddElement(T element);
    public abstract T RemoveElement(T element);

    protected void Touch()
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
