
using System.Collections.Generic;

interface IDoer
{
    bool CanUndo { get; }
    bool CanRedo { get; }

    void Do();
    void Undo();
    void Redo();
}

class Doer<T> : IDoer where T : Versionable<T>
{
    Versionable<T> versionable;

    Stack<T> past = new Stack<T>(),
             future = new Stack<T>();

    public bool CanUndo { get { return past.Count > 1; } }
    public bool CanRedo { get { return future.Count > 0; } }

    public Doer(Versionable<T> versionable_)
    {
        versionable = versionable_;

        Do();
    }

    public void Do()
    {
        past.Push(versionable.Copy());
        future.Clear();
    }

    public void Undo()
    {
        if (!CanUndo)
            return;

        future.Push(past.Pop());
        versionable.Checkout(past.Peek());
    }

    public void Redo()
    {
        if (!CanRedo)
            return;

        versionable.Checkout(future.Peek());
        past.Push(future.Pop());
    }
}
