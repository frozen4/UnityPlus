using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(GScaleScroll), true)]
[CanEditMultipleObjects]
public class GScaleScrollEditor : ScrollRectEditor
{
    SerializedProperty maxScale;
    SerializedProperty minScale;
    SerializedProperty mouseWheelSensitivity;

    protected override void OnEnable()
    {
        base.OnEnable();

        maxScale = serializedObject.FindProperty("maxScale");
        minScale = serializedObject.FindProperty("minScale");
        mouseWheelSensitivity = serializedObject.FindProperty("MouseWheelSensitivity");
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.PropertyField(maxScale);
        EditorGUILayout.PropertyField(minScale);
        EditorGUILayout.PropertyField(mouseWheelSensitivity);

        serializedObject.ApplyModifiedProperties();
    }
}
