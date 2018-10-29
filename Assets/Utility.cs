using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Utility
{
    public static List<T> CreateList<T>(params T[] elements)
    {
        return new List<T>(elements);
    }

    public static Dictionary<T, U> CreateDictionary<T, U>(params object[] keys_and_values)
    {
        Dictionary<T, U> dictionary= new Dictionary<T, U>();

        for (int i = 0; i < keys_and_values.Length / 2; i++)
        {
            T t = (T)keys_and_values[i * 2 + 0];
            U u = (U)keys_and_values[i * 2 + 1];

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

    public static T RemoveElement<T>(List<T> list, T element)
    {
        list.Remove(element);

        return element;
    }

    public static T RemoveElementAt<T>(List<T> list, int index)
    {
        T element = list[index];
        list.RemoveAt(index);

        return element;
    }

    public static void ForEach<T>(List<T> list, Action<T> action)
    {
        foreach (T element in list)
            action(element);
    }
}
