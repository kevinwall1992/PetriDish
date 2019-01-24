using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEditor;

public class BoundingBoxLayout : LayoutElement
{
    public bool constrain_height = true;
    public bool constrain_width = true;

    protected override void Start()
    {
        base.Start();
    }

    protected void Update()
    {
        Vector2 dimensions= GetBoundingBoxDimensions();

        if (constrain_width)
            minWidth = dimensions.x;
        if (constrain_height)
            minHeight = dimensions.y;
    }

    public Vector2 GetBoundingBoxDimensions()
    {
        float left = 0, right = 0, bottom = 0, top = 0;
        bool bounding_box_initialized = false;

        foreach (Transform child in transform)
        {
            if (!(child is RectTransform))
                continue;

            RectTransform rect_transform = child as RectTransform;

            float child_left = rect_transform.position.x - rect_transform.rect.width / 2;
            float child_right = rect_transform.position.x + rect_transform.rect.width / 2;
            float child_bottom = rect_transform.position.y - rect_transform.rect.height / 2;
            float child_top = rect_transform.position.y + rect_transform.rect.height / 2;

            if (!bounding_box_initialized)
            {
                left = child_left;
                right = child_right;
                bottom = child_bottom;
                top = child_top;

                bounding_box_initialized = true;
            }
            else
            {
                left = Mathf.Min(left, child_left);
                right = Mathf.Max(right, child_right);
                bottom = Mathf.Min(bottom, child_bottom);
                top = Mathf.Max(top, child_top);
            }
        }

        return new Vector2(right - left, top - bottom);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(BoundingBoxLayout))]
[CanEditMultipleObjects]
public class BoundingBoxLayoutEditor : Editor
{
    SerializedProperty constrain_height, constrain_width;

    void OnEnable()
    {
        constrain_height = serializedObject.FindProperty("constrain_height");
        constrain_width = serializedObject.FindProperty("constrain_width");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(constrain_height);
        EditorGUILayout.PropertyField(constrain_width);
        serializedObject.ApplyModifiedProperties();
    }
}

#endif