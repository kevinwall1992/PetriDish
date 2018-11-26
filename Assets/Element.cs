using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Element
{
    public static Dictionary<string, Element> elements = new Dictionary<string, Element>();

    static Element()
    {
        JObject elements_file = JObject.Parse(Resources.Load<TextAsset>("elements").text);

        if (elements_file["Elements"] != null)
        {
            JObject elements_json = elements_file["Elements"] as JObject;
            foreach (var element in elements_json)
                elements[element.Key] = new Element(element.Value["Name"].ToString(), Utility.JTokenToInt(element.Value["Atomic Number"]));
        }
    }

    string name;
    int atomic_number;

    public string Name
    {
        get { return name; }
    }

    public int AtomicNumber
    {
        get { return atomic_number; }
    }

    //simplification, not sure its matters though
    public int Mass
    {
        get { return atomic_number * 2; }
    }

    Element(string name_, int atomic_number_)
    {
        name = name_;
        atomic_number = atomic_number_;
    }
}
