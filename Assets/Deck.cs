using System;
using System.Collections.Generic;

public class Deck : List<Catalyst>
{
    public override bool Equals(object other)
    {
        Deck other_deck = other as Deck;
        if (other_deck == null)
            return false;

        foreach (Catalyst catalyst in this)
            if (!other_deck.Contains(catalyst))
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
