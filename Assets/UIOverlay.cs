using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class UIOverlay : GoodBehavior
{
    Dictionary<CompoundComponent, Text> compound_quantity_texts= new Dictionary<CompoundComponent, Text>();

    Text quantity_text_prefab;

    private void Awake()
    {
        quantity_text_prefab = FindDescendent<Text>("quantity_text");
        quantity_text_prefab.transform.parent = null;
    }

    void Start()
    {

    }

    void Update()
    {
        //Remove deleted compounds
        Dictionary<CompoundComponent, Text>.KeyCollection keys = compound_quantity_texts.Keys;
        List<CompoundComponent> deleted_compound_components= new List<CompoundComponent>();
        foreach (CompoundComponent compound_component in keys)
            if (compound_component == null || compound_component.Compound== null || IsActiveCatalyst(compound_component))
                deleted_compound_components.Add(compound_component);
        foreach (CompoundComponent compound_component in deleted_compound_components)
        {
            GameObject.Destroy(compound_quantity_texts[compound_component].gameObject);
            compound_quantity_texts.Remove(compound_component);
        }

        //Add new compounds
        CompoundComponent[] compound_components = GameObject.FindObjectsOfType<CompoundComponent>();
        foreach (CompoundComponent compound_component in compound_components)
        {
            if (!compound_quantity_texts.ContainsKey(compound_component) && compound_component.Compound!= null && !IsActiveCatalyst(compound_component))
            {
                compound_quantity_texts[compound_component] = GameObject.Instantiate(quantity_text_prefab);
                compound_quantity_texts[compound_component].transform.parent = transform;
                compound_quantity_texts[compound_component].gameObject.SetActive(true);
            }
        }

        //Update quantity texts
        foreach (CompoundComponent compound_component in compound_quantity_texts.Keys)
        {
            compound_quantity_texts[compound_component].text = compound_component.Compound.Quantity.ToString();
            compound_quantity_texts[compound_component].transform.position = GameObject.FindObjectOfType<Camera>().WorldToScreenPoint(compound_component.transform.position)+ new Vector3(20, -10);

            Color text_color = compound_quantity_texts[compound_component].color;
            text_color.a = compound_component.GetComponent<SpriteRenderer>().color.a;
            compound_quantity_texts[compound_component].color = text_color;
        }
    }

    bool IsActiveCatalyst(CompoundComponent compound_component)
    {
        if (compound_component.SlotComponent == null)
            return false;

        return compound_component.SlotComponent.CatalystCompoundComponent == compound_component;
    }
}
