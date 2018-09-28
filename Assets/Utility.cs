using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Utility
{
    public static List<T> CreateList<T>(params T[] elements)
    {
        return new List<T>(elements);
    }

    public static Dictionary<T, U> CreateDictionary<T, U>(params object[] keys_and_values) where T : class where U : class
    {
        Dictionary<T, U> dictionary= new Dictionary<T, U>();

        for (int i = 0; i < keys_and_values.Length / 2; i++)
        {
            T t = keys_and_values[i * 2 + 0] as T;
            U u = keys_and_values[i * 2 + 1] as U;

            if (t != null && u != null)
                dictionary[t] = u;
        }

        return dictionary;
    }

    public static List<T> CreateNullList<T>(int size) where T : class
    {
        List<T> list = new List<T>();

        for (int i = 0; i < size; i++)
            list.Add(null);

        return list;
    }
}
