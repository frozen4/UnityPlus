using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

public class HUDText : UIBehaviour
{
    public const float MAX_LIFT_TIME = 2f;

    public Text Label;
    public Transform _NodeBG;
    public GImageGroup ImageGrpBG;
    public CanvasGroup CanvasGrp;

    public static Action<HUDText> OnComplete = null;

    private RectTransform _RectTransform;

    //private Color _DestColor = Color.white;
    //private float _FadeDuration = 0.5f;

    public RectTransform RectTrans
    {
        get
        {
            if (_RectTransform == null)
                _RectTransform = transform as RectTransform;
            if (_RectTransform == null)
                _RectTransform = gameObject.AddComponent<RectTransform>();

            return _RectTransform;
        }
    }

    public void Show(string text, Vector3 uipos, GBMFontAndMotionTextModel config)
    {
        //Debug.LogWarning("Show HUDText " + text + ", " + config.name + ", " + bg_type);

        if (!IsActive())
            gameObject.SetActive(true);


        uipos += new Vector3(config._Offset.x, config._Offset.y, 0);

        RectTrans.localPosition = uipos;
        var fontScale = config._FontScale;
        RectTrans.localScale = Vector3.one * fontScale;
        Label.font = config._Font;
        //Label.fontSize = config._FontSize;
        Label.text = text;

        if (config._BGType > -1)
        {
            GNewUITools.SetVisible(_NodeBG, true);
            ImageGrpBG.SetImageIndex(config._BGType);
        }
        else
        {
            GNewUITools.SetVisible(_NodeBG, false);
        }

        if (config._MotionList != null)
        {
            for (int i = 0; i < config._MotionList.Count; i++)
            {
                if (config._MotionList[i] == null) continue;

                var motion = config._MotionList[i];
                var duration = motion._Duration;
                var dalay = motion._Delay;
                var motionType = motion.GetMotionType();
                if (motionType == GMotionModel.MotionType.Scale)
                {
                    var model = motion as GMotionScaleModel;
                    RectTrans.DOScale(model._Scale * Vector3.one * fontScale, duration)
                        .SetDelay(dalay)
                        .SetEase(model._EaseType);
                }
                else if (motionType == GMotionModel.MotionType.Linear)
                {
                    var model = motion as GMotionLinearModel;
                    RectTrans.DOMove(model.GetDest(transform.position), duration)
                        .SetDelay(dalay)
                        .SetEase(model._EaseType);
                }
                else if (motionType == GMotionModel.MotionType.Alpha)
                {
                    var model = motion as GMotionAlphaModel;
                    //DOTween.ToAlpha(() => Label.color, x => Label.color = x, model._Alpha, duration)
                    //    .SetDelay(dalay);

                    DOTween.To(() => CanvasGrp.alpha, x => CanvasGrp.alpha = x, model._Alpha, duration)
                        .SetDelay(dalay);
                    
                }
                else if (motionType == GMotionModel.MotionType.Curve)
                {
                    var model = motion as GMotionCurveModel;
                    RectTrans.DOPath(model.GetCurvePath(transform.position), duration)
                        .SetDelay(dalay)
                        .SetEase(model._EaseType);
                }
                else if (motionType == GMotionModel.MotionType.ParaCurve)
                {
                    var model = motion as GMotionParaCurveModel;
                    RectTrans.DOPath(model.GetParaCurvePath(transform.position), duration)
                        .SetDelay(dalay)
                        .SetEase(model._EaseType);
                }
            }

            Invoke("OnMotionEnd", MAX_LIFT_TIME);
        }
    }

    private void OnMotionEnd()
    {
        DOTween.Kill(gameObject);
        //DOTween.CompleteAll();
        RectTrans.localScale = Vector3.one;
        Label.font = null;
        //Label.color = Color.white;
        CanvasGrp.alpha = 1;

        if (OnComplete != null)
            OnComplete(this);
    }

    protected override void OnDestroy()
    {
        if (IsInvoking("OnMotionEnd"))
            CancelInvoke("OnMotionEnd");

        base.OnDestroy();
    }
}
