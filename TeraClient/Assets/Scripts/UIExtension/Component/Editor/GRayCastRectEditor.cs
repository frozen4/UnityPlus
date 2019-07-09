
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(GRayCastRect), true)]
[CanEditMultipleObjects]
public class GRayCastRectEditor : Editor
{
    protected void OnEnable()
    {
    }
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("This component is just to make a ray-cast area."); 
    }
}