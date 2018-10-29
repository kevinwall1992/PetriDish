using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface Volume
{
    List<Molecule> Molecules { get; }

    float GetQuantityPerArea(Molecule molecule);
}
