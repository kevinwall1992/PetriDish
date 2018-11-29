﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;
using System.IO;

public class Utility
{
    public static List<T> CreateList<T>(params T[] elements)
    {
        return new List<T>(elements);
    }

    public static Dictionary<T, U> CreateDictionary<T, U>(params object[] keys_and_values)
    {
        Dictionary<T, U> dictionary = new Dictionary<T, U>();

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

    public static void ForEach<T>(IEnumerable enumerable, Action<T> action)
    {
        foreach (T element in enumerable)
            action(element);
    }

    public static Dictionary<T, U> MergeBIntoA<T, U>(Dictionary<T, U> a, Dictionary<T, U> b)
    {
        Dictionary<T, U> merged = new Dictionary<T, U>(a);

        foreach (T key in b.Keys)
            merged[key] = b[key];

        return merged;
    }

    public static string Trim(string string_, int trim_count)
    {
        return string_.Substring(0, string_.Length - trim_count);
    }

    public static List<T> Sorted<T, U>(List<T> list, Func<T, U> comparable_fetcher) where U : IComparable
    {
        list.Sort(delegate(T a, T b) { return comparable_fetcher(a).CompareTo(comparable_fetcher(b)); });

        return list;
    }

    public static List<T> Sorted<T, U>(IEnumerable<T> enumerable, Func<T, U> comparable_fetcher) where U : IComparable
    {
        return Sorted(new List<T>(enumerable), comparable_fetcher);
    }

    public static bool Contains<T>(IEnumerable<T> enumerable, T element)
    {
        foreach (T other_element in enumerable)
            if (EqualityComparer<T>.Default.Equals(other_element, element))
                return true;

        return false;
    }

    public static int Count<T>(IEnumerable<T> enumerable)
    {
        return MathUtility.Sum(enumerable, delegate (T element) { return 1; });
    }

    public static int CountDuplicates<T>(IEnumerable<T> enumerable, T element)
    {
        return MathUtility.Sum(enumerable, delegate (T other_element) { return EqualityComparer<T>.Default.Equals(other_element, element) ? 1 : 0; });
    }

    public static List<T> RemoveDuplicates<T>(IEnumerable<T> enumerable)
    {
        List<T> without_duplicates = new List<T>();

        foreach (T element in enumerable)
            if (!without_duplicates.Contains(element))
                without_duplicates.Add(element);

        return without_duplicates;
    }


    public static string JTokenToString(JToken json, string default_value = "")
    {
        if (json == null)
            return default_value;

        return Convert.ToString(json);
    }

    public static float JTokenToFloat(JToken json, float default_value = 0)
    {
        if (json == null)
            return default_value;

        return Convert.ToSingle(json);
    }

    public static int JTokenToInt(JToken json, int default_value = 0)
    {
        if (json == null)
            return default_value;

        return Convert.ToInt32(json);
    }

    public static bool JTokenToBool(JToken json, bool default_value = false)
    {
        if (json == null)
            return default_value;

        return Convert.ToBoolean(json);
    }


    public static string ReadTextFile(string path)
    {
        return new StreamReader(path).ReadToEnd(); 
    }

    public static void WriteTextFile(string path, string text, bool append = false)
    {
        new StreamWriter(path, append).WriteLine(text);
    }

    public static void OutputText(string text, string name, bool append = false)
    {
        WriteTextFile("Output/" + name + ".txt", text, append);
    }
}