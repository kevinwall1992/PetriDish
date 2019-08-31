using UnityEngine;
using System.Collections;

public class SectorNodeSelectionBox : GoodBehavior
{
    [SerializeField]
    GoodBehavior make_sector_button, 
                  copy_button, 
                  delete_button;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if(Input.GetMouseButtonUp(0))
        {
            if (make_sector_button.IsPointedAt)
                GetComponentInParent<SectorNode>().MakeSectorFromSelection();
            else if (copy_button.IsPointedAt)
                GetComponentInParent<SectorNode>().CopySelection();
            else if (delete_button.IsPointedAt)
                GetComponentInParent<SectorNode>().DeleteSelection();
        }
    }
}
