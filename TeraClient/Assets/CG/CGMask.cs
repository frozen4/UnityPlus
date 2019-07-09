using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class CGMask
{
    //CG遮罩路径
    private readonly string CgMaskPath = "Assets/Outputs/CG/CGMask.prefab";

    //CG遮罩
    private GameObject _MaskGameObject = null;
    private Image _ImageComp = null;
    private Text _TextComp = null;
    private float _ImageBenginAlpha = 0;
    private float _TextBenginAlpha = 0;

    public void Show(TweenCallback cb)
    {
        if (_MaskGameObject == null)
        {
            Action<UnityEngine.Object> callback = (asset) =>
            {
                _MaskGameObject = CUnityUtil.Instantiate(asset) as GameObject;
                if(_MaskGameObject == null) return;

                _MaskGameObject.transform.localScale = Vector3.one;
                _ImageComp = _MaskGameObject.transform.Find("Canvas/Image").GetComponent<Image>();
                _TextComp = _MaskGameObject.transform.Find("Canvas/Text").GetComponent<Text>();

                if (_ImageComp != null)
                {
                    _ImageBenginAlpha = _ImageComp.color.a;
                    var tweener = _ImageComp.DOFade(1, 0.5F);
                    if (cb != null && tweener != null)
                        tweener.OnComplete<Tweener>(cb);
                }
                if (_TextComp != null)
                {
                    _TextBenginAlpha = _TextComp.color.a;
                    _TextComp.DOFade(1, 0.5F);
                }
            };

            CAssetBundleManager.AsyncLoadResource(CgMaskPath, callback, false);
        }
        else
        {
            _MaskGameObject.SetActive(true);
            _MaskGameObject.transform.localScale = Vector3.one;
            if (_ImageComp != null)
            {
                var c = _ImageComp.color;
                c.a = _ImageBenginAlpha;
                _ImageComp.color = c;
                var tweener = _ImageComp.DOFade(1, 0.5F);
                if (cb != null)
                    tweener.OnComplete<Tweener>(cb);
            }

            if (_TextComp != null)
            {
                var c = _TextComp.color;
                c.a = _TextBenginAlpha;
                _TextComp.color = c;
                _TextComp.DOFade(1, 0.5F);
            }

        }
    }

    public void HideText()
    {
        if (_TextComp != null)
            _TextComp.DOFade(0, 1F);
    }

    public void Hide()
    {
        if (_MaskGameObject != null && _ImageComp != null)
        {
            TweenCallback cb = delegate
            {
#if !CACHED
                _MaskGameObject.SetActive(false);
#else
                    _ImageComp = null;
                    _TextComp = null;
                    GameObject.Destroy(_MaskGameObject);
                    _MaskGameObject = null;
#endif
            };

            //由1到0的时间
            _ImageComp.DOFade(0, 0.5F).OnComplete<Tweener>(cb);
        }
    }
}