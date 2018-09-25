using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CodonSelector : GoodBehavior
{
    ScrollRect scroll_rect;
    VerticalLayoutGroup layout_group;

    static GameObject option_prefab;

    int selected_index = 0;
    bool selection_changed = true;

    public bool IsBackgroundVisible
    {
        set
        {
            GetComponent<Image>().color = value ? Color.white : Color.clear;

            if (!value)
                SelectedCodon = GetNearestCodon();
        }
    }

    public string SelectedCodon
    {
        get { return GetCodon(selected_index); }
        set
        {
            selected_index = GetCodonIndex(value);
            scroll_rect.verticalNormalizedPosition = 1 - selected_index / (float)(layout_group.transform.childCount - 1);

            selection_changed = true;
        }
    }

    public List<string> CodonOptions
    {
        set
        {
            ClearOptions();

            foreach (string codon in value)
            {
                GameObject option = GameObject.Instantiate(option_prefab);
                option.GetComponent<Text>().text = codon;
                option.SetActive(true);
                option.transform.parent = layout_group.transform;
            }

            SelectedCodon = value[0];
        }
    }

    private void Awake()
    {
        if (option_prefab == null)
        {
            option_prefab = FindDescendent("codon_option");
            option_prefab.transform.parent = null;
            option_prefab.SetActive(false);
        }

        scroll_rect = FindDescendent<ScrollRect>("codon_selector_scroll");
        layout_group = FindDescendent<VerticalLayoutGroup>("codon_selector_list");
    }

    private void Start()
    {

    }

    private void Update()
    {

    }

    string GetNearestCodon()
    {
        for (int i = 0; i < layout_group.transform.childCount; i++)
            if (RectTransformUtility.RectangleContainsScreenPoint(layout_group.transform.GetChild(i).transform as RectTransform, transform.position))
                return GetCodon(i);

        return GetCodon(0);
    }

    int GetCodonIndex(string codon)
    {
        for (int i = 0; i < layout_group.transform.childCount; i++)
            if (layout_group.transform.GetChild(i).GetComponent<Text>().text == codon)
                return i;

        return 0;
    }

    string GetCodon(int index)
    {
        return layout_group.transform.GetChild(index).GetComponent<Text>().text;
    }

    void ClearOptions()
    {
        while (layout_group.transform.childCount > 0)
        {
            GameObject option = layout_group.transform.GetChild(0).gameObject;

            option.transform.parent = null;
            GameObject.Destroy(option);
        }
    }

    public bool Validate()
    {
        if (selection_changed)
        {
            selection_changed = false;
            return false;
        }

        return true;
    }
}