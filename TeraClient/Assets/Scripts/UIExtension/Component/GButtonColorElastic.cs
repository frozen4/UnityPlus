using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
public class GButtonColorElastic : GButtonColor
{
    [SerializeField]
    private bool _EnableScale = true;

    [SerializeField]
    private float _ScaleButtonDown = .9f;
    [SerializeField]
    private float _ScaleButtonUp = 1f;
    [SerializeField]
    private float _ScaleButtonSelect = 1.05f;
    //[SerializeField]
    //private float _ScaleButtonUnselect = 1f;
    [SerializeField]
    private float _Time = .3f;

    [SerializeField]
    private Transform m_tweenTarget;
    public Transform tweenTarget
    {
        get
        {
            if (_EnableScale)
            {
                if (m_tweenTarget == null)
                {
                    m_tweenTarget = targetGraphic != null ? targetGraphic.transform : Trans;
                }
                return m_tweenTarget;
            }
            else
            {
                return null;
            }
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (tweenTarget)
        {
            //DOTween.Kill(tweenTarget.gameObject);
            //tweenTarget.transform.DOScale(Vector3.one * _ScaleButtonUp, _Time).SetUpdate(true);
            tweenTarget.localScale = Vector3.one * _ScaleButtonUp;
        }
    }

    public override void ButtonDownEffect()
    {
        base.ButtonDownEffect();
        if (tweenTarget && _Time >= 0)
        {
            //DOTween.Kill(tweenTarget.gameObject);
            tweenTarget.DOScale(Vector3.one * _ScaleButtonDown, _Time).SetUpdate(true);
        }
    }

    public override void ButtonUpEffect()
    {
        base.ButtonUpEffect();
        if (tweenTarget && _Time >= 0)
        {
            //DOTween.Kill(tweenTarget.gameObject);
            tweenTarget.DOScale(Vector3.one * _ScaleButtonUp, _Time).SetUpdate(true);
        }
    }

    public override void ButtonSelectEffect()
    {
        base.ButtonSelectEffect();
        if (tweenTarget && _Time >= 0)
        {
            //DOTween.Kill(tweenTarget.gameObject);
            tweenTarget.DOScale(Vector3.one * _ScaleButtonSelect, .1f).SetUpdate(true);
        }
    }
    public override void ButtonDeselectEffect()
    {
        base.ButtonDeselectEffect();
        if (tweenTarget && _Time >= 0)
        {
            //DOTween.Kill(tweenTarget.gameObject);
            tweenTarget.DOScale(Vector3.one * _ScaleButtonUp, .1f).SetUpdate(true);
        }
    }
}
