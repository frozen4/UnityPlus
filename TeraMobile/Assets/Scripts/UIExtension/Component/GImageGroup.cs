using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
//[RequireComponent(typeof(Image))]
public class GImageGroup : GBase
{
    #region serialzation
    public GAtlas Atlas;

    [SerializeField]
    public Sprite[] _Sprites;
    //[SerializeField]
    //private int _Default = 0;

    public bool UseNativeSize;

    #endregion serialzation

    //private int _CurIndex = 0;
    //private int _LastDefault = -1;
    private Image _Image;

    //protected override void Awake()
    //{
    //    _Image = GetComponent<Image>();
    //}

    public override void SetData(object data)
    {
        if (data != null && data is string)
        {
            SetImage(data as string);
        }
    }

    //public void SetDataBool(bool data)
    //{
    //    SetImageIndex(data ? 1 : 0);
    //}

    //public int GetImageIndex()
    //{
    //    return _CurIndex;
    //}

    public void SetImageIndex(int index)
    {
        Sprite sprite = GetSprite(index);

        ApplySprite(sprite);

        //_CurIndex = index;
    }

    public void SetImage(string sprite_name)
    {
        if (string.IsNullOrEmpty(sprite_name))
            return;

        Sprite sprite = GetSprite(sprite_name);

        ApplySprite(sprite);
    }

    public Sprite GetSprite(int sprite_id)
    {
        Sprite sprite = null;
        if (_Sprites != null)
        {
            if (sprite_id >= 0 && sprite_id < _Sprites.Length)
                sprite = _Sprites[sprite_id];
        }
        return sprite;
    }

    public Sprite GetSprite(string sprite_name)
    {
        Sprite sprite = null;
        if (Atlas != null)
        {
            sprite = Atlas.FindSprite(sprite_name);
        }

        if (sprite == null && _Sprites != null)
        {
            for (int i = 0; i < _Sprites.Length; i++)
            {
                sprite = _Sprites[i];
                if (sprite != null && sprite.name == sprite_name)
                    break;
            }
        }

        return sprite;
    }

    private void ApplySprite(Sprite sprite)
    {
        if (sprite == null) return;

        if (_Image == null)
        {
            _Image = GetComponent<Image>();
        }

        if (_Image != null)
        {
            _Image.overrideSprite = sprite;
            if (UseNativeSize)
            {
                _Image.SetNativeSize();
            }
        }
        else
        {
           // Debug.LogError("This node doesnt have an Image on it! " + GNewUITools.PrintScenePath(transform));
        }

    }

}
