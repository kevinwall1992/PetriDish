using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TrashcanPanel : DetailPanel
{
    [SerializeField]
    Hole hole_prefab;

    [SerializeField]
    RectTransform foreshortener, hole_parent, thrown_trash;

    List<Hole> holes = new List<Hole>();

    Dictionary<Compound, Hole> compound_hole_map = new Dictionary<Compound, Hole>();

    Trashcan Trashcan { get { return Data as Trashcan; } }

    protected override void Start()
    {
        base.Start();

        for (int i = 0; holes.Count < Trashcan.Capacity * 2; i++)
            MakeHole();

        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }

    protected override void Update()
    {
        base.Update();

        foreach (Hole hole in holes)
            if (hole.Object != null)
            {
                CompoundTile compound_tile = hole.Object.GetComponent<CompoundTile>();

                if (compound_tile.IsBeingDragged && !hole.ObjectIsInsideHole)
                {
                    hole.RemoveObject();
                    compound_tile.transform.SetParent(Scene.Micro.Canvas.transform);
                }
                else if (!compound_tile.IsBeingDragged && hole.ObjectHasSunk)
                    Trashcan.Bury(compound_tile.Compound);
            }

        if (Trashcan.WasModified(this))
            UpdateCompoundTiles();
    }

    void SortHoles()
    {
        List<Hole> sorted_holes = new List<Hole>();
        sorted_holes.AddRange(holes);
        sorted_holes.Sort((a, b) => (b.transform.position.y.CompareTo(a.transform.position.y)));
        foreach (Hole hole in holes)
            hole.transform.SetSiblingIndex(sorted_holes.IndexOf(hole));
    }

    int halton_index = 0;
    Hole MakeHole()
    {
        Vector2 center = new Vector2(0.5f, 0.5f);
        Vector2 normalized_position;

        do
            normalized_position = new Vector2(MathUtility.HaltonSequence(2, halton_index), MathUtility.HaltonSequence(3, halton_index++));
        while ((normalized_position - center).magnitude > 0.4f);

        Vector2 local_position = (normalized_position - new Vector2(0.5f, 0)) * new Vector2(foreshortener.rect.width, -foreshortener.rect.height);

        Hole hole = Instantiate(hole_prefab);
        hole.transform.SetParent(hole_parent);
        hole.transform.position = foreshortener.TransformPoint(local_position);

        holes.Add(hole);

        SortHoles();

        return hole;
    }

    Hole GetEmptyHole()
    {
        foreach (Hole hole in holes)
            if (hole.Object == null)
                return hole;
            else if (hole.ObjectHasSunk)
            {
                hole.RemoveObject();
                return hole;
            }

        return MakeHole();
    }

    void UpdateCompoundTiles()
    {
        foreach (Compound compound in new List<Compound>(compound_hole_map.Keys))
            if (!Trashcan.Contains(compound))
            {
                if (Utility.Contains(Trashcan.BuriedContents, compound))
                    compound_hole_map[compound].Sink();
                else
                    compound_hole_map[compound].RemoveObject();

                compound_hole_map.Remove(compound);
            }

        foreach(Compound compound in new List<Compound>(Trashcan.Contents))
            if(!compound_hole_map.ContainsKey(compound))
            {
                compound_hole_map[compound] = GetEmptyHole();

                CompoundTile compound_tile = Instantiate(Scene.Micro.Prefabs.CompoundTile);
                compound_tile.Compound = compound;
                compound_tile.transform.SetParent(thrown_trash);
                compound_tile.Size = (transform as RectTransform).rect.width / 7;
                compound_tile.transform.position = Input.mousePosition;

                compound_hole_map[compound].Object = compound_tile.gameObject;

                if (Trashcan.IsAnomaly(compound))
                    compound_hole_map[compound].BubbleUp();
                
            }
    }

    public static TrashcanPanel Create(Trashcan trashcan)
    {
        TrashcanPanel trashcan_panel = Instantiate(Scene.Micro.Prefabs.TrashcanPanel);
        trashcan_panel.transform.SetParent(Scene.Micro.Canvas.transform, false);

        trashcan_panel.Data = trashcan;

        return trashcan_panel;
    }
}