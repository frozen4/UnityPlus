using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;

public class MapOffset : EditorWindow
{
    public Transform sceneObj1;
    public Transform sceneObj2;
    public RectTransform mapObj1;
    public RectTransform mapObj2;

    private double A1;
    private double A2;
    private double OffsetX;
    private double OffsetY;
    private string str1;
    private string str2;

    string info1 = string.Empty;
    string info2 = string.Empty;
    [MenuItem("Window/MapOffset")]
    public static void ShowWindow()
    {
        Rect wr = new Rect(0, 0, 500, 500);
        MapOffset window = (MapOffset)EditorWindow.GetWindowWithRect(typeof(MapOffset), wr, true, "Map Offset");
        window.Show();
    }

    void OnGUI()
    {
        EditorGUI.BeginChangeCheck();

        GUILayout.BeginHorizontal();
        GUILayout.Label("SceneObject1", EditorStyles.boldLabel);
        sceneObj1 = EditorGUILayout.ObjectField(sceneObj1, typeof(Transform)) as Transform;
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("MapObject1", EditorStyles.boldLabel);
        mapObj1 = EditorGUILayout.ObjectField(mapObj1, typeof(RectTransform)) as RectTransform;
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("SceneObject2", EditorStyles.boldLabel);
        sceneObj2 = EditorGUILayout.ObjectField(sceneObj2, typeof(Transform)) as Transform;
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("MapObject2", EditorStyles.boldLabel);
        mapObj2 = EditorGUILayout.ObjectField(mapObj2, typeof(RectTransform)) as RectTransform;
        GUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            info1 = string.Empty;
            info2 = string.Empty;
        }

        if (GUILayout.Button("Calculate", GUILayout.Width(200)))
        {
            CalculateFunction();
            info1 = "A1:" + A1 + "," + "A2:" + A2;
            info2 = "OffsetX:" + OffsetX + "," + "OffsetY:" + OffsetY;
        }
        EditorGUILayout.TextField(info1);
        EditorGUILayout.TextField(info2);

    }
    private void CalculateFunction()
    {
        float a1 = (mapObj1.localPosition.x - mapObj2.localPosition.x) / (sceneObj1.position.x - sceneObj2.position.x);
        float a2 = (mapObj1.localPosition.y - mapObj2.localPosition.y) / (sceneObj1.position.z - sceneObj2.position.z);
        float offsetX = mapObj1.localPosition.x - a1 * sceneObj1.position.x;
        float offsetY = mapObj1.localPosition.y - a2 * sceneObj1.position.z;
        A1 = Math.Round(a1, 3);
        A2 = Math.Round(a2, 3);
        OffsetX = Math.Round(offsetX, 3);
        OffsetY = Math.Round(offsetY, 3);

    }

}
