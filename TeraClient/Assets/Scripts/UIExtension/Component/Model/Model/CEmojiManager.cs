using Common;
using UnityEngine;
public class CEmojiManager : Singleton<CEmojiManager>
{
    //表情资源的命名格式
    //1.表情的命名格式举例：Dog_Laugh,Monkey_Angry,Common_Smile
    //2.拿第一个举例：下划线前面的'Dog'为这个文件的文件名，此时文件夹中应该有两个文件，Dog.png(美术给的)和Dog.asset(工具生成的
    //  选中Dog.png点击生成按钮就会自动生成Dog.asset,名字是一一对应的).下划线后面的'Laugh'为Dog里面的一个单独的小图。

    private GTextEmotionAssets _EmotionAssets;
    public GTextEmotionAssets EmotionAssets
    {
        get
        {
            if (_EmotionAssets == null)
            {
#if UNITY_EDITOR && (ART_USE)
                _EmotionAssets = UnityEditor.AssetDatabase.LoadAssetAtPath<GTextEmotionAssets>("Assets/Outputs/CommonAtlas/Emotions/EmojiAssets.asset");
#elif IN_GAME
                _EmotionAssets = CAssetBundleManager.SyncLoadAssetFromBundle<GTextEmotionAssets>("Assets/Outputs/CommonAtlas/Emotions/EmojiAssets.asset", "commonatlas");

                //if (_EmotionAssets == null)       //todo: del
                //    _EmotionAssets = Resources.Load<GTextEmotionAssets>("Emotions/EmojiAssets");
#endif //UNITY_EDITOR
            }
            return _EmotionAssets;
        }
    }

    public int SysEmojiCount { get { return EmotionAssets != null ? EmotionAssets.SysEmojiCount : 0; } }
    public int EmojiCount { get { return EmotionAssets != null ? EmotionAssets.Count - EmotionAssets.SysEmojiCount : 0; } }

    //获取代表单个表情的GEmotionAsset对象，这个对象存储了单个表情的信息
    public GEmotionAsset GetEmotionAssetByName(string emotionName)
    {
        if (EmotionAssets)
        {
            return EmotionAssets.GetEmotionAssetByName(emotionName);
        }
        return null;
    }
    //获取代表单个表情的GEmotionAsset对象，这个对象存储了单个表情的信息
    public GEmotionAsset GetEmotionAssetById(int id)
    {
        if (EmotionAssets)
            return EmotionAssets.GetEmotionAssetById(id);

        return null;
    }

    public GEmotionAsset GetEmotionAssetAt(int index)
    {
        if (EmotionAssets != null)
            return EmotionAssets.GetEmotionAssetAt(index);

        return null;
    }

    public static int EmojiIndex2EmojiID(int index)
    {
        return GTextEmotionAssets.EmojiIndex2EmojiID(index);
    }

    public void Cleanup()
    {
        _EmotionAssets = null;
    }
}