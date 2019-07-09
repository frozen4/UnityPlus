// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2015/03/12 15:55

//Added OnComplete2Lua to support Tera

using System;
using System.Collections.Generic;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if DOTWEEN_TMP
	using TMPro;
#endif

#pragma warning disable 1591
namespace DG.Tweening
{
    public enum DOTweenAnimationType_My
    {
        None = 0,
        Move = 1,
        LocalMove = 2,
        Rotate = 3,
        LocalRotate = 4,
        Scale = 5,
        Color = 6,
        Fade = 7,
        Text = 8,
        PunchPosition = 9,
        PunchRotation = 10,
        PunchScale = 11,
        ShakePosition = 12,
        ShakeRotation = 13,
        ShakeScale = 14,
        CameraAspect = 15,
        CameraBackgroundColor = 16,
        CameraFieldOfView = 17,
        CameraOrthoSize = 18,
        CameraPixelRect = 19,
        CameraRect = 20,
        UIWidthHeight = 21,
        Fill = 22,
    }


    /// <summary>
    /// Attach this to a GameObject to create a tween
    /// </summary>
    [AddComponentMenu("DOTween/DOTween Animation")]
    public class DOTweenAnimation : ABSAnimationComponent
    {
        public float delay;
        public float duration = 1;
        public Ease easeType = Ease.OutQuad;
        public AnimationCurve easeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public LoopType loopType = LoopType.Restart;
        public int loops = 1;
        public string id = "";
        public bool isRelative;
        public bool isFrom;
        public bool isIndependentUpdate = false;
        public bool autoKill = true;
        public bool noRestoreOnStart = false;

        public bool isActive = true;
        public bool isValid;
        public Component target;
        public DOTweenAnimationType_My animationType;
        public TargetType targetType;
        public TargetType forcedTargetType; // Used when choosing between multiple targets
        public bool autoPlay = true;
        public bool useTargetAsV3;

        public float endValueFloat;
        public Vector3 endValueV3;
        public Vector2 endValueV2;
        public Color endValueColor = new Color(1, 1, 1, 1);
        public string endValueString = "";
        public Rect endValueRect = new Rect(0, 0, 0, 0);
        public Transform endValueTransform;

        public bool optionalBool0;
        public float optionalFloat0;
        public int optionalInt0;
        public RotateMode optionalRotationMode = RotateMode.Fast;
        public ScrambleMode optionalScrambleMode = ScrambleMode.None;
        public string optionalString;

        bool _tweenCreated; // TRUE after the tweens have been created
        bool _isAwake;      //If not awake, dont do anything about playing; 'Cause Cant secure any initial state.
        int _playCount = -1; // Used when calling DOPlayNext

        #region Unity Methods

        void Awake()
        {
            //Debug.Log("DA Awake");
            if (!isActive || !isValid) return;

            if (forcedTargetType != TargetType.Unset) targetType = forcedTargetType;

            MakeSnapshot(ref _InitalVal);

            //if (animationType != DOTweenAnimationType_My.Move || !useTargetAsV3)
            //{
            //    // Don't create tweens if we're using a RectTransform as a Move target,
            //    // because that will work only inside Start
            //    CreateTween();
            //    _tweenCreated = true;
            //}
            ////RecvNapshot();
            _isAwake = true;
        }

        void Start()
        {
            if (_tweenCreated || !isActive || !isValid) return;

            if (autoPlay)
            {
                //if (animationType == DOTweenAnimationType_My.Move)
                //Debug.Log(name + " " + id + " move " + (transform as RectTransform).anchoredPosition3D);

                RecvNapshot(_InitalVal);

                CreateTween();
                _tweenCreated = true;
            }
        }

        void OnDestroy()
        {
            if (tween != null && tween.IsActive()) tween.Kill();
            tween = null;
        }

        [NoToLua]
        // Used also by DOTweenAnimationInspector when applying runtime changes and restarting
        public void CreateTween()
        {
            if (target == null)
            {
                Debug.LogWarning(string.Format("{0} :: This tween's target is NULL, because the animation was created with a DOTween Pro version older than 0.9.255. To fix this, exit Play mode then simply select this object, and it will update automatically", this.gameObject.name), this.gameObject);
                return;
            }

            if (forcedTargetType != TargetType.Unset) targetType = forcedTargetType;
            if (targetType == TargetType.Unset)
            {
                // Legacy DOTweenAnimation (made with a version older than 0.9.450) without stored targetType > assign it now
                targetType = TypeToDOTargetType(target.GetType());
            }

            switch (animationType)
            {
                case DOTweenAnimationType_My.None:
                    break;
                case DOTweenAnimationType_My.Move:
                    if (useTargetAsV3)
                    {
                        isRelative = false;
                        if (endValueTransform == null)
                        {
                            Debug.LogWarning(string.Format("{0} :: This tween's TO target is NULL, a Vector3 of (0,0,0) will be used instead", this.gameObject.name), this.gameObject);
                            endValueV3 = Vector3.zero;
                        }
                        else
                        {
                            if (targetType == TargetType.RectTransform)
                            {
                                RectTransform endValueT = endValueTransform as RectTransform;
                                if (endValueT == null)
                                {
                                    Debug.LogWarning(string.Format("{0} :: This tween's TO target should be a RectTransform, a Vector3 of (0,0,0) will be used instead", this.gameObject.name), this.gameObject);
                                    endValueV3 = Vector3.zero;
                                }
                                else
                                {
                                    RectTransform rTarget = target as RectTransform;
                                    if (rTarget == null)
                                    {
                                        Debug.LogWarning(string.Format("{0} :: This tween's target and TO target are not of the same type. Please reassign the values", this.gameObject.name), this.gameObject);
                                    }
                                    else
                                    {
                                        // Problem: doesn't work inside Awake (ararargh!)
                                        endValueV3 = DOTweenUtils46.SwitchToRectTransform(endValueT, rTarget);
                                    }
                                }
                            }
                            else endValueV3 = endValueTransform.position;
                        }
                    }
                    switch (targetType)
                    {
                        case TargetType.RectTransform:

                            //Debug.Log(((RectTransform)target).anchoredPosition3D.ToString() + "->" + endValueV3.ToString());

                            tween = ((RectTransform)target).DOAnchorPos3D(endValueV3, duration, optionalBool0);
                            break;
                        case TargetType.Transform:
                            tween = ((Transform)target).DOMove(endValueV3, duration, optionalBool0);
                            break;
                        case TargetType.Rigidbody2D:
                            tween = ((Rigidbody2D)target).DOMove(endValueV3, duration, optionalBool0);
                            break;
                        case TargetType.Rigidbody:
                            tween = ((Rigidbody)target).DOMove(endValueV3, duration, optionalBool0);
                            break;
                    }
                    break;
                case DOTweenAnimationType_My.LocalMove:
                    tween = transform.DOLocalMove(endValueV3, duration, optionalBool0);
                    break;
                case DOTweenAnimationType_My.Rotate:
                    switch (targetType)
                    {
                        case TargetType.Transform:
                            tween = ((Transform)target).DORotate(endValueV3, duration, optionalRotationMode);
                            break;
                        case TargetType.Rigidbody2D:
                            tween = ((Rigidbody2D)target).DORotate(endValueFloat, duration);
                            break;
                        case TargetType.Rigidbody:
                            tween = ((Rigidbody)target).DORotate(endValueV3, duration, optionalRotationMode);
                            break;
                    }
                    break;
                case DOTweenAnimationType_My.LocalRotate:
                    tween = transform.DOLocalRotate(endValueV3, duration, optionalRotationMode);
                    break;
                case DOTweenAnimationType_My.Scale:
                    switch (targetType)
                    {
#if DOTWEEN_TK2D
                case TargetType.tk2dTextMesh:
                    tween = ((tk2dTextMesh)target).DOScale(optionalBool0 ? new Vector3(endValueFloat, endValueFloat, endValueFloat) : endValueV3, duration);
                    break;
                case TargetType.tk2dBaseSprite:
                    tween = ((tk2dBaseSprite)target).DOScale(optionalBool0 ? new Vector3(endValueFloat, endValueFloat, endValueFloat) : endValueV3, duration);
                    break;
#endif
                        default:
                            tween = transform.DOScale(optionalBool0 ? new Vector3(endValueFloat, endValueFloat, endValueFloat) : endValueV3, duration);
                            break;
                    }
                    break;
                case DOTweenAnimationType_My.UIWidthHeight:
                    tween = ((RectTransform)target).DOSizeDelta(optionalBool0 ? new Vector2(endValueFloat, endValueFloat) : endValueV2, duration);
                    break;
                case DOTweenAnimationType_My.Color:
                    isRelative = false;
                    switch (targetType)
                    {
                        case TargetType.SpriteRenderer:
                            tween = ((SpriteRenderer)target).DOColor(endValueColor, duration);
                            break;
                        case TargetType.Renderer:
                            tween = ((Renderer)target).material.DOColor(endValueColor, duration);
                            break;
                        case TargetType.Image:
                            tween = ((Image)target).DOColor(endValueColor, duration);
                            break;
                        case TargetType.Text:
                            tween = ((Text)target).DOColor(endValueColor, duration);
                            break;
                        case TargetType.Light:
                            tween = ((Light)target).DOColor(endValueColor, duration);
                            break;
#if DOTWEEN_TK2D
                case TargetType.tk2dTextMesh:
                    tween = ((tk2dTextMesh)target).DOColor(endValueColor, duration);
                    break;
                case TargetType.tk2dBaseSprite:
                    tween = ((tk2dBaseSprite)target).DOColor(endValueColor, duration);
                    break;
#endif
#if DOTWEEN_TMP
                case TargetType.TextMeshProUGUI:
                    tween = ((TextMeshProUGUI)target).DOColor(endValueColor, duration);
                    break;
                case TargetType.TextMeshPro:
                    tween = ((TextMeshPro)target).DOColor(endValueColor, duration);
                    break;
#endif
                    }
                    break;

                case DOTweenAnimationType_My.Fill:
                    isRelative = false;
                    switch (targetType)
                    {
                        case TargetType.Image:
                            tween = ((Image)target).DOFillAmount(endValueFloat, duration);
                            break;
                    }
                    break;

                case DOTweenAnimationType_My.Fade:
                    isRelative = false;
                    switch (targetType)
                    {
                        case TargetType.SpriteRenderer:
                            tween = ((SpriteRenderer)target).DOFade(endValueFloat, duration);
                            break;
                        case TargetType.Renderer:
                            tween = ((Renderer)target).material.DOFade(endValueFloat, duration);
                            break;
                        case TargetType.Image:
                            tween = ((Image)target).DOFade(endValueFloat, duration);
                            break;
                        case TargetType.Text:
                            tween = ((Text)target).DOFade(endValueFloat, duration);
                            break;
                        case TargetType.Light:
                            tween = ((Light)target).DOIntensity(endValueFloat, duration);
                            break;
                        case TargetType.CanvasGroup:
                            tween = ((CanvasGroup)target).DOFade(endValueFloat, duration);
                            break;
#if DOTWEEN_TK2D
                case TargetType.tk2dTextMesh:
                    tween = ((tk2dTextMesh)target).DOFade(endValueFloat, duration);
                    break;
                case TargetType.tk2dBaseSprite:
                    tween = ((tk2dBaseSprite)target).DOFade(endValueFloat, duration);
                    break;
#endif
#if DOTWEEN_TMP
                case TargetType.TextMeshProUGUI:
                    tween = ((TextMeshProUGUI)target).DOFade(endValueFloat, duration);
                    break;
                case TargetType.TextMeshPro:
                    tween = ((TextMeshPro)target).DOFade(endValueFloat, duration);
                    break;
#endif
                    }
                    break;
                case DOTweenAnimationType_My.Text:
                    switch (targetType)
                    {
                        case TargetType.Text:
                            tween = ((Text)target).DOText(endValueString, duration, optionalBool0, optionalScrambleMode, optionalString);
                            break;
#if DOTWEEN_TK2D
                case TargetType.tk2dTextMesh:
                    tween = ((tk2dTextMesh)target).DOText(endValueString, duration, optionalBool0, optionalScrambleMode, optionalString);
                    break;
#endif
#if DOTWEEN_TMP
                case TargetType.TextMeshProUGUI:
                    tween = ((TextMeshProUGUI)target).DOText(endValueString, duration, optionalBool0, optionalScrambleMode, optionalString);
                    break;
                case TargetType.TextMeshPro:
                    tween = ((TextMeshPro)target).DOText(endValueString, duration, optionalBool0, optionalScrambleMode, optionalString);
                    break;
#endif
                    }
                    break;
                case DOTweenAnimationType_My.PunchPosition:
                    switch (targetType)
                    {
                        case TargetType.RectTransform:
                            tween = ((RectTransform)target).DOPunchAnchorPos(endValueV3, duration, optionalInt0, optionalFloat0, optionalBool0);
                            break;
                        case TargetType.Transform:
                            tween = ((Transform)target).DOPunchPosition(endValueV3, duration, optionalInt0, optionalFloat0, optionalBool0);
                            break;
                    }
                    break;
                case DOTweenAnimationType_My.PunchScale:
                    tween = transform.DOPunchScale(endValueV3, duration, optionalInt0, optionalFloat0);
                    break;
                case DOTweenAnimationType_My.PunchRotation:
                    tween = transform.DOPunchRotation(endValueV3, duration, optionalInt0, optionalFloat0);
                    break;
                case DOTweenAnimationType_My.ShakePosition:
                    switch (targetType)
                    {
                        case TargetType.RectTransform:
                            tween = ((RectTransform)target).DOShakeAnchorPos(duration, endValueV3, optionalInt0, optionalFloat0, optionalBool0);
                            break;
                        case TargetType.Transform:
                            tween = ((Transform)target).DOShakePosition(duration, endValueV3, optionalInt0, optionalFloat0, optionalBool0);
                            break;
                    }
                    break;
                case DOTweenAnimationType_My.ShakeScale:
                    tween = transform.DOShakeScale(duration, endValueV3, optionalInt0, optionalFloat0);
                    break;
                case DOTweenAnimationType_My.ShakeRotation:
                    tween = transform.DOShakeRotation(duration, endValueV3, optionalInt0, optionalFloat0);
                    break;
                case DOTweenAnimationType_My.CameraAspect:
                    tween = ((Camera)target).DOAspect(endValueFloat, duration);
                    break;
                case DOTweenAnimationType_My.CameraBackgroundColor:
                    tween = ((Camera)target).DOColor(endValueColor, duration);
                    break;
                case DOTweenAnimationType_My.CameraFieldOfView:
                    tween = ((Camera)target).DOFieldOfView(endValueFloat, duration);
                    break;
                case DOTweenAnimationType_My.CameraOrthoSize:
                    tween = ((Camera)target).DOOrthoSize(endValueFloat, duration);
                    break;
                case DOTweenAnimationType_My.CameraPixelRect:
                    tween = ((Camera)target).DOPixelRect(endValueRect, duration);
                    break;
                case DOTweenAnimationType_My.CameraRect:
                    tween = ((Camera)target).DORect(endValueRect, duration);
                    break;
            }

            if (tween == null) return;

            if (isFrom)
            {
                ((Tweener)tween).From(isRelative);
            }
            else
            {
                tween.SetRelative(isRelative);
            }
            tween.SetTarget(this.gameObject).SetDelay(delay).SetLoops(loops, loopType).SetAutoKill(autoKill)
                .OnKill(() => { tween = null; });        //keep this thing safe, after killing
            if (isSpeedBased) tween.SetSpeedBased();
            if (easeType == Ease.INTERNAL_Custom) tween.SetEase(easeCurve);
            else tween.SetEase(easeType);
            if (!string.IsNullOrEmpty(id)) tween.SetId(id);
            tween.SetUpdate(isIndependentUpdate);

            if (hasOnStart)
            {
                if (onStart != null) tween.OnStart(onStart.Invoke);
            }
            else onStart = null;
            if (hasOnPlay)
            {
                if (onPlay != null) tween.OnPlay(onPlay.Invoke);
            }
            else onPlay = null;
            if (hasOnUpdate)
            {
                if (onUpdate != null) tween.OnUpdate(onUpdate.Invoke);
            }
            else onUpdate = null;
            if (hasOnStepComplete)
            {
                if (onStepComplete != null) tween.OnStepComplete(onStepComplete.Invoke);
            }
            else onStepComplete = null;

            if (hasOnComplete)
            {
                if (onComplete != null) //tween.OnComplete(onComplete.Invoke);
                    tween.OnComplete(_InternalOnComplete);
            }
            else onComplete = null;
            //Support Tera Lua Script
            if (IsCallLuaOnComplete)
            {
                tween.OnComplete(_InternalOnComplete);
            }
            //if (hasOnComplete)
            //{
            //    if (onComplete != null) tween.OnComplete(onComplete.Invoke);
            //}
            //else onComplete = null;
            if (hasOnRewind)
            {
                if (onRewind != null) tween.OnRewind(onRewind.Invoke);
            }
            else onRewind = null;

            if (autoPlay) tween.Play();
            else tween.Pause();

            if (hasOnTweenCreated && onTweenCreated != null) onTweenCreated.Invoke();
        }

        #endregion

        #region Public Methods

        #region Do**
        // These methods are here so they can be called directly via Unity's UGUI event system

        [ContextMenu("DOPlay")]
        public override void DOPlay()
        {
            DOTween.Play(this.gameObject);
        }

        [ContextMenu("DOPlayBackwards")]
        public override void DOPlayBackwards()
        {
            DOTween.PlayBackwards(this.gameObject);
        }

        [ContextMenu("DOPlayForward")]
        public override void DOPlayForward()
        {
            DOTween.PlayForward(this.gameObject);
        }

        public override void DOPause()
        {
            DOTween.Pause(this.gameObject);
        }

        [ContextMenu("DOTogglePause")]
        public override void DOTogglePause()
        {
            DOTween.TogglePause(this.gameObject);
        }

        [ContextMenu("DORewind")]
        public override void DORewind()
        {
            _playCount = -1;
            // Rewind using Components order (in case there are multiple animations on the same property)
            DOTweenAnimation[] anims = this.gameObject.GetComponents<DOTweenAnimation>();
            for (int i = anims.Length - 1; i > -1; --i)
            {
                Tween t = anims[i].tween;
                if (t != null && t.IsInitialized()) anims[i].tween.Rewind();
            }
            // DOTween.Rewind(this.gameObject);
        }

        /// <summary>
        /// Restarts the tween
        /// </summary>
        /// <param name="fromHere">If TRUE, re-evaluates the tween's start and end values from its current position.
        /// Set it to TRUE when spawning the same DOTweenAnimation in different positions (like when using a pooling system)</param>
        public override void DORestart(bool fromHere = false)
        {
            _playCount = -1;
            if (tween == null)
            {
                if (Debugger.logPriority > 1) Debugger.LogNullTween(tween); return;
            }
            if (fromHere && isRelative) ReEvaluateRelativeTween();
            DOTween.Restart(this.gameObject);
        }

        [ContextMenu("DOComplete")]
        public override void DOComplete()
        {
            DOTween.Complete(this.gameObject);
        }

        public override void DOKill()
        {
            DOTween.Kill(this.gameObject);
            tween = null;
        }
        #endregion

        #region Specifics

        public void DOPlayById(string id)
        {
            DOTween.Play(this.gameObject, id);
        }
        public void DOPlayAllById(string id)
        {
            DOTween.Play(id);
        }

        public void DOPauseAllById(string id)
        {
            DOTween.Pause(id);
        }

        public void DOPlayBackwardsById(string id)
        {
            DOTween.PlayBackwards(this.gameObject, id);
        }
        public void DOPlayBackwardsAllById(string id)
        {
            DOTween.PlayBackwards(id);
        }

        public void DOPlayForwardById(string id)
        {
            DOTween.PlayForward(this.gameObject, id);
        }
        public void DOPlayForwardAllById(string id)
        {
            DOTween.PlayForward(id);
        }

        public void DOPlayNext()
        {
            DOTweenAnimation[] anims = this.GetComponents<DOTweenAnimation>();
            while (_playCount < anims.Length - 1)
            {
                _playCount++;
                DOTweenAnimation anim = anims[_playCount];
                if (anim != null && anim.tween != null && !anim.tween.IsPlaying() && !anim.tween.IsComplete())
                {
                    anim.tween.Play();
                    break;
                }
            }
        }

        public void DORewindAndPlayNext()
        {
            _playCount = -1;
            DOTween.Rewind(this.gameObject);
            DOPlayNext();
        }

        public void DORestartById(string id)
        {
            _playCount = -1;
            DOTween.Restart(this.gameObject, id);
        }
        public void DORestartAllById(string id)
        {
            _playCount = -1;
            DOTween.Restart(id);
        }

        /// <summary>
        /// Returns the tweens created by this DOTweenAnimation, in the same order as they appear in the Inspector (top to bottom)
        /// </summary>
        [NoToLua]
        public List<Tween> GetTweens()
        {
            //            return DOTween.TweensByTarget(this.gameObject);

            List<Tween> result = new List<Tween>();
            DOTweenAnimation[] anims = this.GetComponents<DOTweenAnimation>();
            foreach (DOTweenAnimation anim in anims) result.Add(anim.tween);
            return result;
        }

        #region Internal Static Helpers (also used by Inspector)
        [NoToLua]
        public static TargetType TypeToDOTargetType(Type t)
        {
            string str = t.ToString();
            int dotIndex = str.LastIndexOf(".");
            if (dotIndex != -1) str = str.Substring(dotIndex + 1);
            if (str.IndexOf("Renderer") != -1 && (str != "SpriteRenderer")) str = "Renderer";
            return (TargetType)Enum.Parse(typeof(TargetType), str);
        }

        #endregion


        #endregion

        #endregion

        #region Private

        // Re-evaluate relative position of path
        void ReEvaluateRelativeTween()
        {
            if (animationType == DOTweenAnimationType_My.Move)
            {
                ((Tweener)tween).ChangeEndValue(transform.position + endValueV3, true);
            }
            else if (animationType == DOTweenAnimationType_My.LocalMove)
            {
                ((Tweener)tween).ChangeEndValue(transform.localPosition + endValueV3, true);
            }
        }

        #endregion

        #region My_Extension

        bool _IsSmartKilling;

        private void _SecureTween(bool b_restart)
        {
            if (!isActive || !isValid) return;

            if (!_isAwake)
            {
                //Debug.LogWarning("Not awake, dont do anything about playing; 'Cause Cant secure any initial state.");
                return;  //If not awake, dont do anything about playing; 'Cause Cant secure any initial state.
            }

            if (_tweenCreated || !autoPlay) //If autoPlay, just leave it
            {
                if (tween == null)
                {
                    RecvNapshot(_InitalVal);
                    CreateTween();
                    _tweenCreated = true;
                }
                else if (b_restart)
                {
                    tween.Pause();

                    tween.SetDelay(delay);

                    if (!isFrom)
                    {
                        RecvNapshot(_InitalVal);
                    }

                    tween.Restart();
                }
            }
        }

        [NoToLua]
        public void SinglePlay()
        {
            if (!isActive || !isValid || !enabled || !gameObject.activeInHierarchy) return;

            _SecureTween(false);

            if (tween != null && !tween.IsPlaying())
            {
                tween.Play();
            }
        }

        [NoToLua]
        public void SingleRestart(bool fromHere = false)
        {
            if (!isActive || !isValid || !enabled || !gameObject.activeInHierarchy) return;

            _SecureTween(true);

            if (tween != null)
            {
                if (fromHere && isRelative) ReEvaluateRelativeTween();

                if (!tween.IsPlaying())
                {
                    tween.Play();
                }
            }
        }

        [NoToLua]
        public void SinglePlayOnce()
        {
            if (!isActive || !isValid) return;
            if ((!autoPlay || _tweenCreated))
            {
                if (tween != null)
                    tween.Kill();

                CreateTween();
                tween.SetAutoKill(true);
                tween.Play();
            }
        }

        [NoToLua]
        public void SingleDirectional(bool is_forward)
        {
            if (!isActive || !isValid || !enabled || !gameObject.activeInHierarchy) return;

            _SecureTween(false);

            if (tween != null)
            {
                if (is_forward)
                {
                    tween.PlayForward();
                }
                else
                {
                    tween.PlayBackwards();
                }
            }
        }

        [NoToLua]
        public void SinglePause()
        {
            if (!isActive || !isValid) return;
            if (tween != null)
            {
                tween.Pause();
            }
        }

        [NoToLua]
        public void SingleRewind()
        {
            if (!isActive || !isValid) return;
            if (tween != null)
            {
                tween.Rewind();
            }
        }

        [NoToLua]
        public void SingleKill(bool b_complete = false, bool b_smart = false)
        {
            if (!isActive || !isValid) return;
            if (tween != null)
            {
                //if (tween.IsPlaying())
                //{
                //    if (b_completeFrom && isFrom)
                //    {
                //        b_complete = true;
                //    }

                //    if (b_rewindTo && !isFrom)
                //    {
                //        tween.Rewind();
                //    }

                //    tween.Kill(b_complete);
                //    tween = null;
                //}
                //else
                //{
                //    tween.Pause();
                //}

                //revert twice, but I need it;
                //cant use IsInitlized 'cause 2 of them can ether be not ...

                _IsSmartKilling = b_smart;

                if (b_smart)
                {
                    if (isFrom && !tween.IsComplete())
                    {
                        b_complete = true;
                    }

                    if (!isFrom)
                    {
                        //if (IsCallLuaOnComplete && tween.IsPlaying())
                        //{
                        //    _InternalOnComplete();
                        //}

                        if (tween.IsInitialized())
                        {
                            tween.Rewind();
                        }
                    }
                }

                tween.Kill(b_complete);
                tween = null;

                _IsSmartKilling = false;
            }
        }

        [NoToLua]
        public void SingleDestroyObj()
        {
            if (!isActive || !isValid) return;
            if (tween != null)
            {
                tween.Kill();
                tween = null;
            }
            UnityEngine.Object.Destroy(gameObject);
        }

        #endregion

        #region Call Lua

        [NoToLua]
        public bool IsCallLuaOnComplete;      //whether to call Lua On Complete

        [System.NonSerialized]
        [NoToLua]
        public UnityAction<string, string> OnComplete2Lua;

        private void _InternalOnComplete()
        {
            if (_IsSmartKilling) return;

#if ART_USE
            if (IsCallLuaOnComplete)
                Debug.Log(gameObject.name + " has called OnComplete2Lua, " + id);
#endif

            if (IsCallLuaOnComplete && OnComplete2Lua != null)
            {
                OnComplete2Lua(gameObject.name, id);
            }

            if (hasOnComplete && onComplete != null)
            {
                onComplete.Invoke();
            }
        }

        #endregion

        #region Snapshot

        private object _InitalVal;
        //object _TmpVal;
        void MakeSnapshot(ref object save_data)
        {
            if (noRestoreOnStart) return;

            switch (animationType)
            {
                case DOTweenAnimationType_My.Color:
                    switch (targetType)
                    {
                        case TargetType.SpriteRenderer:
                            save_data = ((SpriteRenderer)target).color;
                            break;
                        case TargetType.Renderer:
                            save_data = ((Renderer)target).material.color;
                            break;
                        case TargetType.Image:
                            save_data = ((Image)target).color;
                            break;
                        case TargetType.Text:
                            save_data = ((Text)target).color;
                            break;
                        case TargetType.Light:
                            save_data = ((Light)target).color;
                            break;
                    }
                    break;
                case DOTweenAnimationType_My.Fade:
                    switch (targetType)
                    {
                        case TargetType.SpriteRenderer:
                            save_data = ((SpriteRenderer)target).color.a;
                            break;
                        case TargetType.Renderer:
                            save_data = ((Renderer)target).material.color.a;
                            break;
                        case TargetType.Image:
                            save_data = ((Image)target).color.a;
                            break;
                        case TargetType.Text:
                            save_data = ((Text)target).color.a;
                            break;
                        case TargetType.Light:
                            save_data = ((Light)target).color.a;
                            break;
                        case TargetType.CanvasGroup:
                            save_data = ((CanvasGroup)target).alpha;
                            break;
                    }
                    break;
                case DOTweenAnimationType_My.Fill:
                    switch (targetType)
                    {
                        case TargetType.Image:
                            save_data = ((Image)target).fillAmount;
                            break;
                    }
                    break;
                case DOTweenAnimationType_My.LocalMove:
                case DOTweenAnimationType_My.Move:
                    Transform t = transform;
                    switch (targetType)
                    {
                        case TargetType.RectTransform:
                            save_data = (t as RectTransform).anchoredPosition3D;
                            //Debug.Log(id+" pos: " + save_data);
                            break;
                        default:
                            save_data = transform.localPosition;
                            //Debug.Log(id + " pos: " + save_data);
                            break;
                    }
                    break;
                case DOTweenAnimationType_My.LocalRotate:
                case DOTweenAnimationType_My.Rotate:
                    save_data = transform.localRotation;
                    break;
                case DOTweenAnimationType_My.Scale:
                    //Debug.Log(name + " " + id + " scale " + transform.localScale);

                    save_data = transform.localScale;
                    break;
            }
        }

        void RecvNapshot(object save_data)
        {
            //if (noRestoreOnStart) return;
            if (save_data == null) return;
            switch (animationType)
            {
                case DOTweenAnimationType_My.Color:
                    switch (targetType)
                    {
                        case TargetType.SpriteRenderer:
                            ((SpriteRenderer)target).color = (Color)save_data;
                            break;
                        case TargetType.Renderer:
                            ((Renderer)target).material.color = (Color)save_data;
                            break;
                        case TargetType.Image:
                            ((Image)target).color = (Color)save_data;
                            break;
                        case TargetType.Text:
                            ((Text)target).color = (Color)save_data;
                            break;
                        case TargetType.Light:
                            ((Light)target).color = (Color)save_data;
                            break;
                    }
                    break;
                case DOTweenAnimationType_My.Fade:
                    switch (targetType)
                    {
                        case TargetType.SpriteRenderer:
                            {
                                SpriteRenderer item = (SpriteRenderer)target;
                                Color col = item.color;
                                col.a = (float)save_data;
                                item.color = col;
                            }
                            break;
                        case TargetType.Renderer:
                            {
                                Material item = ((Renderer)target).material;
                                Color col = item.color;
                                col.a = (float)save_data;
                                item.color = col;
                            }
                            break;
                        case TargetType.Image:
                            {
                                Image item = ((Image)target);
                                Color col = item.color;
                                col.a = (float)save_data;
                                item.color = col;
                            }
                            break;
                        case TargetType.Text:
                            {
                                Text item = ((Text)target);
                                Color col = item.color;
                                col.a = (float)save_data;
                                item.color = col;
                            }
                            break;
                        case TargetType.Light:
                            {
                                Light item = ((Light)target);
                                Color col = item.color;
                                col.a = (float)save_data;
                                item.color = col;
                            }
                            break;
                        case TargetType.CanvasGroup:
                            {
                                CanvasGroup item = ((CanvasGroup)target);
                                item.alpha = (float)save_data;
                            }
                            break;
                    }
                    break;
                case DOTweenAnimationType_My.Fill:
                    switch (targetType)
                    {
                        case TargetType.Image:
                            {
                                Image item = ((Image)target);
                                item.fillAmount = (float)save_data;
                            }
                            break;
                    }
                    break;
                case DOTweenAnimationType_My.LocalMove:
                case DOTweenAnimationType_My.Move:
                    //transform.localPosition = (Vector3)save_data;
                    Transform t = transform;
                    switch (targetType)
                    {
                        case TargetType.RectTransform:
                            (t as RectTransform).anchoredPosition3D = (Vector3)save_data;
                            break;
                        default:
                            transform.localPosition = (Vector3)save_data;
                            break;
                    }
                    break;
                case DOTweenAnimationType_My.LocalRotate:
                case DOTweenAnimationType_My.Rotate:
                    transform.localRotation = (Quaternion)save_data;
                    break;
                case DOTweenAnimationType_My.Scale:
                    transform.localScale = (Vector3)save_data;
                    //Debug.Log(name + " " + id + " scale recv " + transform.localScale);

                    break;
            }
        }

        void GoToTargetPos()
        {
            if (target == null)
            {
                Debug.LogWarning("This tween's target is NULL", this.gameObject);
                return;
            }

            if (forcedTargetType != TargetType.Unset) targetType = forcedTargetType;

            switch (animationType)
            {
                case DOTweenAnimationType_My.Move:
                    switch (targetType)
                    {
                        case TargetType.RectTransform:
                            ((RectTransform)target).anchoredPosition3D = endValueV3;
                            break;
                        default:
                            transform.localPosition = endValueV3;
                            break;
                    }
                    break;
                case DOTweenAnimationType_My.LocalMove:
                    transform.localPosition = endValueV3;
                    break;

                case DOTweenAnimationType_My.Rotate:
                    transform.localRotation = Quaternion.Euler(endValueV3);
                    break;
                case DOTweenAnimationType_My.LocalRotate:

                    break;
                case DOTweenAnimationType_My.Scale:
                    transform.localScale = new Vector3(endValueFloat, endValueFloat, endValueFloat);
                    //Debug.Log(name + " " + id + " scale recv " + transform.localScale);

                    break;
                case DOTweenAnimationType_My.Color:
                    switch (targetType)
                    {
                        case TargetType.SpriteRenderer:
                            ((SpriteRenderer)target).color = endValueColor;
                            break;
                        case TargetType.Renderer:
                            ((Renderer)target).material.color = endValueColor;
                            break;
                        case TargetType.Image:
                            ((Image)target).color = endValueColor;
                            break;
                        case TargetType.Text:
                            ((Text)target).color = endValueColor;
                            break;
                        case TargetType.Light:
                            ((Light)target).color = endValueColor;
                            break;
                    }
                    break;
                case DOTweenAnimationType_My.Fill:
                    switch (targetType)
                    {
                        case TargetType.Image:
                            {
                                Image item = ((Image)target);
                                item.fillAmount = (float)endValueFloat;
                            }
                            break;
                    }
                    break;
                case DOTweenAnimationType_My.Fade:
                    switch (targetType)
                    {
                        case TargetType.SpriteRenderer:
                            {
                                SpriteRenderer item = (SpriteRenderer)target;
                                Color col = item.color;
                                col.a = (float)endValueFloat;
                                item.color = col;
                            }
                            break;
                        case TargetType.Renderer:
                            {
                                Material item = ((Renderer)target).material;
                                Color col = item.color;
                                col.a = (float)endValueFloat;
                                item.color = col;
                            }
                            break;
                        case TargetType.Image:
                            {
                                Image item = ((Image)target);
                                Color col = item.color;
                                col.a = (float)endValueFloat;
                                item.color = col;
                            }
                            break;
                        case TargetType.Text:
                            {
                                Text item = ((Text)target);
                                Color col = item.color;
                                col.a = (float)endValueFloat;
                                item.color = col;
                            }
                            break;
                        case TargetType.Light:
                            {
                                Light item = ((Light)target);
                                Color col = item.color;
                                col.a = (float)endValueFloat;
                                item.color = col;
                            }
                            break;
                        case TargetType.CanvasGroup:
                            {
                                CanvasGroup item = ((CanvasGroup)target);
                                item.alpha = (float)endValueFloat;
                            }
                            break;
                    }
                    break;
                default:
                    Debug.LogWarning("<GoToEndPos>Type not Supported " + animationType);
                    break;
            }
        }

        public void GoToStartPos()
        {
            if (isFrom)
            {
                GoToTargetPos();
            }
            else
            {
                RecvNapshot(_InitalVal);
            }
        }

        public void GoToEndPos()
        {
            if (isFrom)
            {
                RecvNapshot(_InitalVal);
            }
            else
            {
                GoToTargetPos();
            }
        }

        #endregion
    }

    public static class DOTweenAnimationExtensions
    {
        //        // Doesn't work on Win 8.1
        //        public static bool IsSameOrSubclassOf(this Type t, Type tBase)
        //        {
        //            return t.IsSubclassOf(tBase) || t == tBase;
        //        }

        [NoToLua]
        public static bool IsSameOrSubclassOf<T>(this Component t)
        {
            return t is T;
        }
    }
}