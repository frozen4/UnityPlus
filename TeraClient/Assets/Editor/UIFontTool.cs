using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class UIFontTool : EditorWindow
{
    // [MenuItem("Tools/批量操作（请谨慎）/操作UI字体")]
    public static void ChangeUIFont()
    {
        Rect rc = new Rect(0, 0, 1000, 640);
        UIFontTool window = (UIFontTool)EditorWindow.GetWindowWithRect(typeof(UIFontTool), rc, true, "UIFontTool");
        window.Init();
        window.Show();
    }

    public List<GameObject> Prefabs = new List<GameObject>();

    public void Init()
    {

    }

    Font sourceFont = null;
    Font targetFont = null;
    Font selectFont = null;

    int fontSize = 0;
    int enumInt = 0;
    public string[] desPath = new string[] { "Assets/Resources" };
    public void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        sourceFont = EditorGUILayout.ObjectField("将预设上的", sourceFont, typeof(Font), false) as Font;
        EditorGUILayout.LabelField("字体修改成============>>>>>>>>>>>");
        targetFont = EditorGUILayout.ObjectField("", targetFont, typeof(Font), false) as Font;
        EditorGUILayout.LabelField("字体");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();


        if (GUILayout.Button("替换字体"))
        {
            if (null == sourceFont || null == targetFont)
            {
                return;
            }
            string[] allGuids = AssetDatabase.FindAssets("t:Prefab", desPath);
            //string[] allGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Prefabs/UI"});
            int guidIndex = 0;
            foreach (string guid in allGuids)
            {
                EditorUtility.DisplayProgressBar("Info", "正在替换请稍后", (float)(++guidIndex) / allGuids.Length);
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.EndsWith(".prefab"))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    //if (null == prefab) continue;
                    //Prefabs.Add(prefab);
                    Text[] texts = prefab.GetComponentsInChildren<Text>(true);
                    foreach (Text text in texts)
                    {
                        if (text.font == sourceFont)
                        {
                            text.font = targetFont;
                        }
                    }
                    EditorUtility.SetDirty(prefab);
                }
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }



        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<============>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        EditorGUILayout.HelpBox("修改特定字体的大小", MessageType.Error);

        selectFont = EditorGUILayout.ObjectField("需要修改的字体", selectFont, typeof(Font), false) as Font;
        EditorGUILayout.LabelField("当前的字体大小便宜量为   " + fontSize);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("增大一号"))
        {
            fontSize = fontSize + 1;
        }
        if (GUILayout.Button("减小一号"))
        {
            fontSize = fontSize - 1;
        }
        if (GUILayout.Button("确认修改"))
        {
            if (null == selectFont) return;
            string[] allGuids2 = AssetDatabase.FindAssets("t:Prefab", desPath);
            int guidIndex2 = 0;
            foreach (string guid in allGuids2)
            {
                EditorUtility.DisplayProgressBar("Info", "正在替换请稍后", (float)(++guidIndex2) / allGuids2.Length);
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.EndsWith(".prefab"))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    Text[] texts = prefab.GetComponentsInChildren<Text>(true);
                    foreach (Text text in texts)
                    {
                        if (text.font == selectFont)
                        {
                            text.fontSize = text.fontSize + fontSize;
                        }
                    }
                    EditorUtility.SetDirty(prefab);
                }
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();


        }
        EditorGUILayout.EndHorizontal();
    }

    [MenuItem("Tools/操作UI字体/转成中文")]
    public static void ChangeLanToCN()
    {
        ChangeLan(LanType.CN);
    }

    enum LanType
    {
        CN = 1,
        KR = 2
    }
    [MenuItem("Tools/操作UI字体/转成韩文")]
    public static void ChangeLanToKR()
    {
        ChangeLan(LanType.KR);
    }
    static void ChangeLan(LanType lantype)
    {

        #region Load

        Font sourceFont1;
        Font sourceFont2;
        Font targetFont1;
        Font targetFont2;
        if (lantype == LanType.CN)
        {
             sourceFont1 = AssetDatabase.LoadAssetAtPath<Font>("Assets/Font/2002.TTF");
             sourceFont2 = AssetDatabase.LoadAssetAtPath<Font>("Assets/Font/RIXGOB.TTF");
             targetFont1 = AssetDatabase.LoadAssetAtPath<Font>("Assets/Font/Fzzh2.TTF");
             targetFont2 = AssetDatabase.LoadAssetAtPath<Font>("Assets/Font/Fzzh1.TTF");
        }
        else
        {
             targetFont1 = AssetDatabase.LoadAssetAtPath<Font>("Assets/Font/2002.TTF");
             targetFont2 = AssetDatabase.LoadAssetAtPath<Font>("Assets/Font/RIXGOB.TTF");
             sourceFont1 = AssetDatabase.LoadAssetAtPath<Font>("Assets/Font/Fzzh2.TTF");
             sourceFont2 = AssetDatabase.LoadAssetAtPath<Font>("Assets/Font/Fzzh1.TTF");
        }

        if (null == sourceFont1 || null == targetFont2 || null == targetFont1 || null == targetFont2)
        {
            EditorUtility.DisplayDialog("Error", "有字体路径不对，请与洋葱联系", "OK");
            return;
        }


        #endregion
   
        string[] allGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Resources" });
        int guidIndex = 0;
        foreach (string guid in allGuids)
        {
            EditorUtility.DisplayProgressBar("Info", "正在替换请稍后", (float)(++guidIndex) / allGuids.Length);
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.EndsWith(".prefab"))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (null == prefab) continue;
                //Prefabs.Add(prefab);
                Text[] texts = prefab.GetComponentsInChildren<Text>(true);
                foreach (Text text in texts)
                {
                    if (text.font == sourceFont1)
                    {
                        text.font = targetFont1;
                    }
                    if (text.font == sourceFont2)
                    {
                        text.font = targetFont2;
                    }
                }
                EditorUtility.SetDirty(prefab);
            }
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
