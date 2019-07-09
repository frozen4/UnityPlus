using UnityEditor.UI;
using UnityEditor;

[CustomEditor(typeof(GText), true)]
[CanEditMultipleObjects]
public class GTextEditor : TextEditor
{
    //SerializedProperty _OnClickLink;
    SerializedProperty _GraphicOffset;
    SerializedProperty _GraphicScale;
    protected override void OnEnable()
    {
        base.OnEnable();
        //_OnClickLink = serializedObject.FindProperty("_OnClickLink");
        _GraphicOffset = serializedObject.FindProperty("GraphicOffset");
        _GraphicScale = serializedObject.FindProperty("GraphicScale");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        serializedObject.Update();
        //EditorGUILayout.PropertyField(_OnClickLink);
        EditorGUILayout.PropertyField(_GraphicOffset);
        EditorGUILayout.PropertyField(_GraphicScale);
        serializedObject.ApplyModifiedProperties();
    }
}