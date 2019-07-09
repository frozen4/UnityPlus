using UnityEngine;
public class GTextEmotionModel
{
    private int _TextIndex;
    private string _SpriteName;
    private int _Size;
    private Vector3 _TextLowerLeftPosition;
    public Vector3 PosMin;
    public Vector3 PosMax;
    public int TextIndex { get { return _TextIndex; } set { _TextIndex = value; } }
    public string SpriteName { get { return _SpriteName; } set { _SpriteName = value; } }
    public int Size { get { return _Size; } set { _Size = value; } }
    public Vector3 TextLowerLeftPosition { get { return _TextLowerLeftPosition; } set { _TextLowerLeftPosition = value; } }
    public GEmotionAsset GetEmotionAsset()
    {
        int id;
        //return CEmojiManager.Instance.GetEmotionAssetByName(SpriteName);
        if (int.TryParse(SpriteName, out id))
        {
            return CEmojiManager.Instance.GetEmotionAssetById(id);
        }
        return null;
    }

    public Vector2 GetSizeVector2()
    {
        return Vector2.one * Size;
    }
    public Vector3 GetSizeVector3()
    {
        return new Vector3(Size, Size, 0);
    }
}