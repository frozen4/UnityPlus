using UnityEngine;
[System.Serializable]
public class GEmotionAsset
{
#if UNITY_EDITOR
    private int _Id;
    public int Id { get { return _Id; } set { _Id = value; } }
#endif

    public Sprite IconSprite;
    public string Name { get { return IconSprite.name; } }
    public Vector2 Pivot { get { return IconSprite.pivot; } }
    public Rect Rect { get { return IconSprite.rect;} }
    public Vector4 GetOuterUV()
    {
        return UnityEngine.Sprites.DataUtility.GetOuterUV(IconSprite);
    }
    public Vector2 GetTextureSize()
    {
        return IconSprite.texture.texelSize;
    }
}