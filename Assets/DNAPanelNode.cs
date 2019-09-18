using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class DNAPanelNode : GoodBehavior
{
    [SerializeField]
    protected RectTransform collapsed_form;

    [SerializeField]
    protected RectTransform expanded_form;

    bool is_collapsed = true;

    Vector2 touch_down_position;

    public SectorNode SectorNode { get { return transform.parent.GetComponentInParent<SectorNode>(); } }

    public virtual bool IsCollapsed
    {
        get { return is_collapsed; }

        set
        {
            if (is_collapsed == value)
                return;

            is_collapsed = value;

            if(is_collapsed)
            {
                expanded_form.gameObject.SetActive(false);
                collapsed_form.gameObject.SetActive(true);
            }
            else
            {
                if (SectorNode != null)
                {
                    SectorNode.CollapseAllNodes(this);
                    SectorNode.HideInsertionChoice();
                }

                collapsed_form.gameObject.SetActive(false);
                expanded_form.gameObject.SetActive(true);

                transform.SetAsLastSibling();
            }

            RectTransform current_form = is_collapsed ? collapsed_form : expanded_form;
            (transform as RectTransform).sizeDelta = new Vector2(current_form.rect.width, 
                                                                 current_form.rect.height);
        }
    }

    public int CodonIndex
    {
        get { return SectorNode.NodeToCodonIndex(this); }
    }

    public virtual int CodonLength { get { return 1; } }

    public virtual string DNASequence
    {
        get { return GetComponentInParent<SectorNode>().Sector.DNA.GetSubsequence(CodonIndex, CodonLength); }
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetMouseButtonDown(0) && IsTouched)
            touch_down_position = Input.mousePosition;

        if (Input.GetMouseButtonUp(0) &&
            IsTouched &&
            touch_down_position == (Vector2)Input.mousePosition)
            IsCollapsed = false;
    }

    private void OnGUI()
    {
        if (!IsCollapsed)
        {
            int depth = 0;

            Transform ancestor = transform.parent;
            if (ancestor.GetComponent<DNAPanel>() != null)
                return;

            while(ancestor!= null)
            {
                if (ancestor.GetComponent<SectorNode>() != null)
                    depth++;

                ancestor = ancestor.parent;
            }

            GUI.depth = -depth - 1;

            if (Utility.ConsumeIsKeyUp(KeyCode.Escape))
                IsCollapsed = true;
        }
    }

    public void SetForms(RectTransform collapsed_form_, RectTransform expanded_form_)
    {
        collapsed_form = collapsed_form_;
        expanded_form = expanded_form_;
    }
}
