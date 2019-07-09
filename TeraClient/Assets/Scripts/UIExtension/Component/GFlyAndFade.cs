using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GFlyAndFade : GBase
{
    [SerializeField]
    private float _StayTime = 1f;
    [SerializeField]
    private float _FlyTime = 1f;//此时间默认等于FadeTime
    private float _FlyDistance = 2f;

    [SerializeField]
    private Ease _easeType = Ease.OutElastic;//ease type效果预览 ： http://robertpenner.com/easing/easing_demo.html
    private Vector2 _OriginPos = Vector2.zero;
    protected override void Awake()
    {
        _OriginPos = RectTrans.anchoredPosition;
    }
    protected override void OnEnable()
    {
        Invoke("DoRun", _StayTime);
    }

    private void DoRun()
    {
        CancelInvoke();
        Graphic graphic = GetComponent<Graphic>();
        Color newColor = graphic.color;
        newColor.a = 0f;
        graphic.CrossFadeColor(newColor, _FlyTime * .7f, true, true);//*.7 means the color will change faster than position

        Graphic[] child_graphics = GetComponentsInChildren<Graphic>();
        for(int i = 0; i < child_graphics.Length; ++i)
        {
            child_graphics[i].CrossFadeColor(newColor, _FlyTime * .7f, true, true);
        }
        RectTrans.DOMoveY(transform.position.y + _FlyDistance, _FlyTime)
        .SetEase(_easeType)
        .OnComplete(TweenComplete);
    }
    void TweenComplete()
    {
        Destroy(gameObject);
    }
}
