using UnityEditor.UI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GRawImage), true)]
[CanEditMultipleObjects]
public class GRawImageEditor : RawImageEditor
{
    SerializedProperty _spRadius;
    SerializedProperty _spSpans;

    GRawImage _src;
    protected override void OnEnable()
    {
        _src = target as GRawImage;
        _spRadius = serializedObject.FindProperty("Radius");
        _spSpans = serializedObject.FindProperty("TriangleNum");
        base.OnEnable();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_spRadius);
        EditorGUILayout.PropertyField(_spSpans);
        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }
}