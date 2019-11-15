using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class UIOverlay : GoodBehavior
{
    [SerializeField]
    public Text quantity_text_prefab;

    Dictionary<CompoundComponent, CompoundOverlays> compound_overlays = new Dictionary<CompoundComponent, CompoundOverlays>();


    protected override void Update()
    {
        base.Update();

        //Add new compounds
        CompoundComponent[] compound_components = Scene.Micro.Visualization.GetComponentsInChildren<CompoundComponent>();
        foreach (CompoundComponent compound_component in compound_components)
            if (!compound_overlays.ContainsKey(compound_component) && compound_component.Compound != null)
                compound_overlays[compound_component] = new CompoundOverlays(this, compound_component);

        //Update overlays
        foreach (CompoundOverlays compound_overlays___ in compound_overlays.Values)
            compound_overlays___.Update();

        //Remove deleted compounds
        foreach (CompoundOverlays compound_overlays___ in new List<CompoundOverlays>(compound_overlays.Values))
            if (compound_overlays___.WasDestroyed)
                compound_overlays.Remove(compound_overlays___.compound_component);
    }

    class CompoundOverlays
    {
        public CompoundComponent compound_component;

        public Text quantity_overlay;

        public bool WasDestroyed { get; private set; }

        public CompoundOverlays(UIOverlay ui_overlay, CompoundComponent compound_component_)
        {
            compound_component = compound_component_;
            WasDestroyed = false;

            quantity_overlay = Instantiate(ui_overlay.quantity_text_prefab);
            quantity_overlay.transform.SetParent(ui_overlay.transform);
        }

        public void Update()
        {
            if (compound_component == null || compound_component.Compound == null)
            {
                Destroy();
                return;
            }

            Vector3 screen_position = Scene.Micro.Camera.WorldToScreenPoint(compound_component.transform.position);

            quantity_overlay.text = Measures.GetVisualQuantity(compound_component.Compound).ToString("n1");
            quantity_overlay.transform.position = screen_position + new Vector3(20, -10);

            Color text_color = quantity_overlay.color;
            text_color.a = compound_component.MoleculeComponent.GetComponent<SpriteRenderer>().color.a;
            quantity_overlay.color = text_color;
        }

        public void Destroy()
        {
            if(quantity_overlay != null)
                GameObject.Destroy(quantity_overlay.gameObject);

            WasDestroyed = true;
        }
    }
}
