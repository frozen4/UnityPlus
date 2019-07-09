using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class GTextEmotionAssetsCreator
{
    public const string EMOJI_ICON_PREFIX = "CBT_Emoji_";       //系统 CBT_SysEmoji_ Id(1-1000), 聊天 CBT_Emoji_ Id(1001 +)

    [MenuItem("Hoba Tools/EmojiMaker", false, 10)]
    static void Create()
    {
        Object target = Selection.activeObject;
        if (target == null || target.GetType() != typeof(Texture2D))
            return;

        Texture2D sourceTex = target as Texture2D;
        //整体路径
        string filePathWithName = AssetDatabase.GetAssetPath(sourceTex);
        //带后缀的文件名
        string fileNameWithExtension = Path.GetFileName(filePathWithName);
        //不带后缀的文件名
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePathWithName);
        //不带文件名的路径
        string filePath = filePathWithName.Replace(fileNameWithExtension, "");

        GTextEmotionAssets asset = AssetDatabase.LoadAssetAtPath(filePath + fileNameWithoutExtension + "Assets.asset", typeof(GTextEmotionAssets)) as GTextEmotionAssets;

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<GTextEmotionAssets>();
            asset.MainTexture = sourceTex;
            int sysIconCount;
            asset.SetEmotionAssets(GetSpritesInfor(sourceTex, out sysIconCount));
            asset.SysEmojiCount = sysIconCount;

            AssetDatabase.CreateAsset(asset, filePath + fileNameWithoutExtension + "Assets.asset");
        }
    }
    public static List<GEmotionAsset> GetSpritesInfor(Texture2D tex, out int sysIconCount)
    {
        List<GEmotionAsset> emotionAssets = new List<GEmotionAsset>();
        string filePath = UnityEditor.AssetDatabase.GetAssetPath(tex);
        Object[] objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(filePath);

        sysIconCount = 0;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].GetType() == typeof(Sprite))
            {
                GEmotionAsset temp = new GEmotionAsset();
                Sprite sprite = objects[i] as Sprite;
                //temp.Id = i;
                temp.IconSprite = sprite;

#if UNITY_EDITOR
                temp.Id = GetIDFromName(sprite.name);

                if (temp.Id < GTextEmotionAssets.EMOJI_ICON_START)
                {
                    sysIconCount += 1;
                }
#endif //UNITY_EDITOR

                emotionAssets.Add(temp);
            }
        }

#if UNITY_EDITOR
        emotionAssets.Sort(CompareAsset);   //sort by name
#endif //UNITY_EDITOR

        if (sysIconCount >= GTextEmotionAssets.EMOJI_ICON_START)
        {
            Debug.LogError("sysIconCount > " + (GTextEmotionAssets.EMOJI_ICON_START - 1));
        }

        for (int i = 1; i < sysIconCount; i++)
        {
            if (emotionAssets[i].Id - emotionAssets[i - 1].Id != 1)
            {
                Debug.LogError("CBT_SysEmoji 数字不连续 At" + emotionAssets[i - 1].Id);
            }
        }

        for (int i = sysIconCount + 1; i < emotionAssets.Count; i++)
        {
            if (emotionAssets[i].Id - emotionAssets[i - 1].Id != 1)
            {
                Debug.LogError("CBT_Emoji 数字不连续 At" + emotionAssets[i - 1].Id);
            }
        }

        return emotionAssets;
    }

    static int GetIDFromName(string sp_name)
    {
        string[] sa = sp_name.Split('_');

        int id = -1;
        if (sa.Length > 0)
        {
            if (!int.TryParse(sa[sa.Length - 1], out id))
            {
                id = -1;
            }

            if (sp_name.StartsWith(EMOJI_ICON_PREFIX))
            {
                id += GTextEmotionAssets.EMOJI_ICON_START;
            }
        }
        return id;
    }

    #if UNITY_EDITOR
    static int CompareAsset(GEmotionAsset x, GEmotionAsset y)
    {
        if (x.Id > y.Id)
        {
            return 1;
        }
        else if (x.Id == y.Id)
        {
            return 0;
        }
        else
        {
            return -1;
        }
    }
    #endif //UNITY_EDITOR
}
