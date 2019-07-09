using UnityEngine;
using System.Collections.Generic;
public class GTextEmotionAssets : ScriptableObject
{
    public const int EMOJI_SYSICON_START = 0;
    public const int EMOJI_ICON_START = 1001;

    [NoToLua]
    public Texture2D MainTexture;

    [SerializeField]
    private int _SysEmojiCount;

    //所有表情都存到这个list中，如果表情太多可以打成不同的图集，生成到这个list中。
    [NoToLua]
    [SerializeField]
    private List<GEmotionAsset> _EmotionAssets;

    public int Index2Id(int index)
    {
        return index < SysEmojiCount ? index : index - SysEmojiCount + GTextEmotionAssets.EMOJI_ICON_START;
    }

    public int Id2Index(int id)
    {
        if (id < GTextEmotionAssets.EMOJI_ICON_START)
        {
            return id < SysEmojiCount ? id - EMOJI_SYSICON_START : -1;
        }
        else
        {
            return id + SysEmojiCount - EMOJI_ICON_START;
        }

    }

    public GEmotionAsset GetEmotionAssetByName(string emotionName)
    {
        GEmotionAsset ast = this._EmotionAssets.Find(item => item.Name == emotionName);
        if (ast != null)
        {
            return ast;
        }
        return null;
    }

    public int SysEmojiCount { get { return _SysEmojiCount; } set { _SysEmojiCount = value; } }
    public int EmojiCount { get { return Count - _SysEmojiCount; } }

    public GEmotionAsset GetEmotionAssetById(int id)
    {
        id = Id2Index(id);

        return GetEmotionAssetAt(id);
    }

    //public GEmotionAsset GetEmotionAssetById(int id)
    //{
    //    if (_EmotionAssets != null)
    //    {
    //        return this._EmotionAssets.Find(item => item.Id == id);
    //    }
    //    return null;
    //}

    public GEmotionAsset GetEmotionAssetAt(int index)
    {
        if (_EmotionAssets != null)
        {
            if (index >= 0 && index < _EmotionAssets.Count)
            {
                return _EmotionAssets[index];
            }
        }
        return null;
    }

    public int Count
    {
        get { return _EmotionAssets != null ? this._EmotionAssets.Count : 0; }
    }

    [NoToLua]
    public void SetEmotionAssets(List<GEmotionAsset> assets)
    {
        this._EmotionAssets = assets;
    }

    public static int EmojiIndex2EmojiID(int index)
    {
        return index + EMOJI_ICON_START;
    }
}
