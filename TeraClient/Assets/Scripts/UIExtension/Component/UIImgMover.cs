using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class UIImgMover : MonoBehaviour
{
    #region serialization

    public Rect StartValue = new Rect(1, 0, 1, 1);
    public Rect EndValue = new Rect(-1, 0, 1, 1);
    public float Duration = 2;
    public LoopType LoopEType;

    #endregion serialization

    private RawImage _ImgRaw;
    private Tween _Tween;

	[NoToLua]
    public void StartUp(Rect v_startVal, Rect v_endVal, float f_duration, LoopType e_loopType)
    {
        if (_Tween != null)
        {
            _Tween.Kill();
        }

        if (_ImgRaw == null)
        {
            _ImgRaw = GetComponent<RawImage>();
        }

        if (_ImgRaw != null)
        {
            _ImgRaw.uvRect = v_startVal;
            _Tween = DOTween.To(() => _ImgRaw.uvRect, delegate(Rect uv)
                {
                    _ImgRaw.uvRect = uv;
                }, v_endVal, f_duration).SetTarget(gameObject);
            _Tween.SetLoops(-1, e_loopType);
        }
    }

	[NoToLua]
    public void Stop()
    {
        if (_Tween != null)
        {
            _Tween.Kill();
        }
    }

    private void OnEnable()
    {
        StartUp(StartValue, EndValue, Duration, LoopEType);
    }

    private void OnDisable()
    {
        Stop();
    }
}
