using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[AddComponentMenu("UI/GuideMask")]
public class UIGuideMask : UnityEngine.UI.Graphic
{
    public Vector2 LeftOffset = Vector2.zero;
    public Vector2 RightOffset = Vector2.zero;
    public Vector2 TopOffset = Vector2.zero;
    public Vector2 BottomOffset = Vector2.zero;

    public Vector2 DesignResolution = Vector2.one;

    protected bool RectDirtyFlag = false;

    [SerializeField]
    protected Texture2D m_MaskTex = null;
    public override Texture mainTexture
    {
        get
        {
            return m_MaskTex ? m_MaskTex : base.mainTexture;
        }
    }

    public override Material materialForRendering
    {
        get
        {
            return m_Material;
        }
    }

    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
    {
        vh.Clear();

        UIVertex[] vertexs = new UIVertex[4];

        vertexs[0].position = new Vector3(-1, -1, 0);
        vertexs[1].position = new Vector3(-1, 1, 0);
        vertexs[2].position = new Vector3(1, 1, 0);
        vertexs[3].position = new Vector3(1, -1, 0);

        vertexs[0].color = color;
        vertexs[1].color = color;
        vertexs[2].color = color;
        vertexs[3].color = color;

        vertexs[0].uv0 = new Vector2(0, 0);
        vertexs[1].uv0 = new Vector2(0, 1);
        vertexs[2].uv0 = new Vector2(1, 1);
        vertexs[3].uv0 = new Vector2(1, 0);

        vh.AddUIVertexQuad(vertexs);
    }

    public override bool Raycast(Vector2 sp, Camera eventCamera)
    {
        return false;
    }

    void UpdateRect()
    {
        if (RectDirtyFlag)
        {
            RectDirtyFlag = false;

            int screenWidth = (int)DesignResolution.x;
            int screenHeight = (int)DesignResolution.y;

            Vector4 rect = new Vector4(0, 0, 1, 1);

            rect.x = LeftOffset.x + LeftOffset.y / screenWidth;
            rect.y = BottomOffset.x + BottomOffset.y / screenHeight;

            rect.z = 1 - (RightOffset.x + RightOffset.y / screenWidth);
            rect.w = 1 - (TopOffset.x + TopOffset.y / screenHeight);

            m_Material.SetVector(ShaderIDs.UIRect, rect);

            SetMaterialDirty();
        }
    }

    [ContextMenu("UpdateMaterialRect")]
    public void UpdateMaterialRect()
    {
        RectDirtyFlag = true;
        UpdateRect();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UIGuideMask))]
public class UIGuideMaskEd : Editor
{
    protected SerializedProperty m_Color;
    protected SerializedProperty m_Material;
    protected SerializedProperty m_MainTexture;
    protected SerializedProperty m_RaycastTarget;

    protected virtual void OnEnable()
    {
        m_Color = serializedObject.FindProperty("m_Color");
        m_Material = serializedObject.FindProperty("m_Material");
        m_MainTexture = serializedObject.FindProperty("m_MaskTex");
        m_RaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_Material);
        EditorGUILayout.PropertyField(m_Color);
        EditorGUILayout.PropertyField(m_MainTexture);
        EditorGUILayout.PropertyField(m_RaycastTarget);      

        UIGuideMask o = target as UIGuideMask;

        EditorGUI.BeginChangeCheck();
        o.LeftOffset = EditorGUILayout.Vector2Field("Left Offset", o.LeftOffset);
        o.RightOffset = EditorGUILayout.Vector2Field("Right Offset", o.RightOffset);
        o.TopOffset = EditorGUILayout.Vector2Field("Top Offset", o.TopOffset);
        o.BottomOffset = EditorGUILayout.Vector2Field("Bottom Offset", o.BottomOffset);
        o.DesignResolution = EditorGUILayout.Vector2Field("Design Resolution", o.DesignResolution);
        if (EditorGUI.EndChangeCheck())
        {
            o.UpdateMaterialRect();

            Canvas.ForceUpdateCanvases();

            System.Type type = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            var s_RepaintAll = type.GetMethod("RepaintAll",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            if (s_RepaintAll != null)
            {
                s_RepaintAll.Invoke(null, null);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif