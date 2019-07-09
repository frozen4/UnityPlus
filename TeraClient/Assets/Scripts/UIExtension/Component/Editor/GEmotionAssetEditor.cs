using UnityEngine;
using UnityEditor;
using System.IO;
using System;

[CustomEditor(typeof(GTextEmotionAssets))]
public class GTextEmotionAssetsEditor : Editor
{
    GTextEmotionAssets asset;
    Vector2 ve2ScorllView;

    public void OnEnable()
    {
        asset = (GTextEmotionAssets)target;
    }
    public override void OnInspectorGUI()
    {
        if (asset == null) return;

        if (GUILayout.Button("导出txt"))
        {
            Export();
        }

        ve2ScorllView = GUILayout.BeginScrollView(ve2ScorllView);
        GUILayout.Label("表情信息列表");
        for (int i = 0; i < asset.Count; i++)
        {
            GEmotionAsset emotionAsset = asset.GetEmotionAssetAt(i);
            //EditorGUILayout.ObjectField(emotionAsset.Name, emotionAsset.IconSprite, typeof(Sprite), false);
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Id:" + asset.Index2Id(i).ToString() + " Name:" + emotionAsset.Name);
            //EditorGUILayout.LabelField("Povit:" + emotionAsset.Pivot.ToString() + " Rect:" + emotionAsset.Rect.ToString());
        }

        GUILayout.EndScrollView();
    }

    public void Export()
    {
        string textPath = Application.dataPath + "/../emoji.txt";
        StreamWriter sw = null;
        try
        {
            sw = new StreamWriter(textPath, false, new System.Text.UTF8Encoding(false));
            sw.WriteLine("--所有Emoji对应ID ");
            string s_fileContent = "";
            for (int i = 0; i < asset.Count; i++)
            {
                GEmotionAsset emotionAsset = asset.GetEmotionAssetAt(i);
                s_fileContent += asset.Index2Id(i).ToString() + ", " + emotionAsset.Name + "\n";
            }
            sw.Write(s_fileContent);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            if (sw != null)
            {
                sw.Close();
            }
        }

        Debug.Log(textPath);
    }
}