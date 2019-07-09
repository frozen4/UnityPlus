using UnityEngine.UI;
using DG.Tweening;

public class ScreenFadeEffectManager : MonoBehaviourSingleton<ScreenFadeEffectManager>
{
    private Image _GlobalImage = null;
    private Tweener _CurTweener = null;

    private Image GlobalImage()
    {
        if (_GlobalImage == null)
            _GlobalImage = gameObject.GetComponent<Image>();

        return _GlobalImage;
    }

    public void StartFade(float from, float to, float duration, Image img = null)
    {
        Image curImage = null;

        if (img != null)
            curImage = img;
        else
            curImage = GlobalImage();

        if (curImage == null) return;

        curImage.enabled = true;

        var color = curImage.color;
        color.a = from;
        curImage.color = color;
        curImage.raycastTarget = true;
        _CurTweener = DOTween.ToAlpha(() => curImage.color, x => curImage.color = x, to, duration)
                        .OnComplete(() =>
                        {
                            if (curImage != null && curImage.color.a < 0.01f)
                                curImage.enabled = false;
                            curImage.raycastTarget = false;
                        });
    }

    public void ClearFadeEffect()
    {
        Image curImage = GlobalImage();

        if (curImage == null) return;

        if (_CurTweener != null && _CurTweener.IsPlaying())
        {
            _CurTweener.Kill();
            _CurTweener = DOTween.ToAlpha(() => curImage.color, x => curImage.color = x, 0, 0.5f)
                        .OnComplete(() =>
                        {
                            if (curImage != null && curImage.color.a < 0.01f)
                                curImage.enabled = false;
                            curImage.raycastTarget = false;
                        });
        }
        else if (curImage.color.a > 0)
        {
            curImage.raycastTarget = true;
            _CurTweener = DOTween.ToAlpha(() => curImage.color, x => curImage.color = x, 0, 0.5f)
                        .OnComplete(() =>
                        {
                            if (curImage != null && curImage.color.a < 0.01f)
                                curImage.enabled = false;
                            curImage.raycastTarget = false;
                        });
        }
    }
}
