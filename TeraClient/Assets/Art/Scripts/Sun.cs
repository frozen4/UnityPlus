using UnityEngine;

#if UNITY_EDITOR && ART_USE
using UnityEditor;
#endif

[ExecuteInEditMode]
public class Sun : MonoBehaviour
{
    public Color SunColor = Color.white;
    [Range(0, 8)]
    public float SunColorIntensity = 1;

    public Color AmbientColor = Color.white;
    [Range(0, 8)]
    public float AmbientColorIntensity = 1;

    // public static implicit operator Color(Vector4 v);
    private Vector4 _Color = Vector4.zero;

    public void Apply()
    {
        Shader.SetGlobalVector(ShaderIDs.SunDir, -transform.forward);

        _Color.x = SunColor.r;
        _Color.y = SunColor.g;
        _Color.z = SunColor.b;
        _Color.w = SunColorIntensity;

        Shader.SetGlobalColor(ShaderIDs.SunColor, _Color);

        _Color.x = AmbientColor.r;
        _Color.y = AmbientColor.g;
        _Color.z = AmbientColor.b;
        _Color.w = AmbientColorIntensity;
        Shader.SetGlobalColor(ShaderIDs.SunAmbientColor, _Color);
    }

    void OnEnable()
    {
        Apply();
    }

#if ART_USE
    void Update()
    {
        Apply();
    }
#endif
}

#if UNITY_EDITOR && ART_USE
[CustomEditor(typeof(Sun))]
public class SunEd : Editor
{
    SerializedObject so = null;

    SerializedProperty sp1 = null;
    SerializedProperty sp2 = null;
    SerializedProperty sp3 = null;
    SerializedProperty sp4 = null;
    SerializedProperty sp5 = null;

    void OnEnable()
    {
        SerializedObject so = new SerializedObject(target);

        sp1 = so.FindProperty("SunDirectionCharacter");
        sp2 = so.FindProperty("SunColor");
        sp3 = so.FindProperty("SunColorIntensity");
        sp4 = so.FindProperty("AmbientColor");
        sp5 = so.FindProperty("AmbientColorIntensity");

        Sun sun = target as Sun;
        sun.Apply();
    }

    void OnDisable()
    {
        so = null;

        sp1 = null;
        sp2 = null;
        sp3 = null;
        sp4 = null;
        sp5 = null;
    }

    public override void OnInspectorGUI()
    {
        Sun sun = target as Sun;

        EditorGUILayout.PropertyField(sp2);
        EditorGUILayout.PropertyField(sp3);
        EditorGUILayout.PropertyField(sp4);
        EditorGUILayout.PropertyField(sp5);

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }
}

#endif