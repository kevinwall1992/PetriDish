using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface Versionable<T> : Copiable<T>
{
    void Checkout(T version);
    bool IsSameVersion(T version);
}

