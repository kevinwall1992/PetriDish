using System;
using System.Collections.Generic;

public class Deck : List<Catalyst>
{
    public override bool Equals(object obj)
    {
        if (obj == this)
            return true;

        Deck other = obj as Deck;
        if (other == null)
            return false;

        foreach (Catalyst catalyst in this)
            if (!other.Contains(catalyst))
                return false;

        foreach (Catalyst catalyst in this)
            if (!this.Contains(catalyst))
                return false;

        return true;
    }

    public override int GetHashCode()
    {
        int hash = 17;

        foreach (Catalyst catalyst in this)
            hash = hash * 23 + catalyst.GetHashCode();

        return hash;
    }
}
