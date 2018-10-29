using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class World : Chronal
{
    static World the_world;

    static World()
    {
        the_world = new World();
    }

    public static World TheWorld{ get { return the_world; } }


    List<Locale> locales = new List<Locale>();

    public World()
    {

    }

    public void Step()
    {
        foreach (Locale locale in locales)
            locale.Step();
    }
}
