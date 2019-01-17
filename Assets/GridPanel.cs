using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GridPanel : DetailPanel
{
    [SerializeField]
    GridLayoutGroup grid_layout_group;

    protected GridLayoutGroup GridLayoutGroup { get { return grid_layout_group; } }

    public int RowLength { get; set; }

    protected override void Start()
    {
        RowLength = 5;

        base.Start();
    }

    protected override void Update()
    {
        float width = (GridLayoutGroup.transform as RectTransform).rect.width;
        float space = width * 0.02f;
        float size = (width - space * (RowLength - 1)) / RowLength;

        GridLayoutGroup.cellSize = new Vector2(size, size);
        GridLayoutGroup.spacing = new Vector2(space, space);

        base.Update();
    }
}
