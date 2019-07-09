using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public abstract class GTweenUIBase : GNewUIBase
{
    //public enum TweenTarget
    //{
    //    PositionX,
    //    PositionY,
    //    PositionZ,
    //    RotationX,
    //    RotationY,
    //    RotationZ,
    //    SizeX,
    //    SizeY,
    //    SizeZ,
    //    Alpha
    //}

    [NoToLua]
    public enum TweenType
    {
        Simple,
        Curv,
    }

    [NoToLua]
    public enum Phase
    {
        End,
        Delay,
        Running,
    }

    [NoToLua]
    [System.Serializable]
    public class TweenInfo
    {
        public bool IsEnabled;

        public TweenType TweenType;
        //public TweenTarget tweenTarget;

        public AnimationCurve AnimCurv;
        public float StartValue;
        public float EndValue;
    }

    [NoToLua]
    public TweenInfo[] TweenList = null;

    public float Duration = 1;
    public float Delay = 0;

    public bool AutoPlay;
    public bool InitState = true;
    public bool StopWhenDisabled = false;

    [System.Serializable]
    [NoToLua]
    public class TweenCallBack : UnityEvent<GTweenUIBase> { }
    [NoToLua]
    public TweenCallBack OnTweenStart = new TweenCallBack();
    [NoToLua]
    public TweenCallBack OnTweenFinish = new TweenCallBack();

    public bool InverseCurve { get { return _InverseCurve; } }
    public float NormalizedTime { get { return _RealDuration > 0 ? (_SelfTime - _RealDelay) / _RealDuration : 0; } }

    protected float _SelfTime = 0;
    protected Phase _Phase;  //!pause

    private bool _IsToPlay;
    private bool _InverseCurve = false;

    private float _RealDuration = 1;
    private float _RealDelay = 0;

    public void PlayTweenOnce(bool forward)
    {
        PlayTween(0, 0, !forward);
    }

    [NoToLua]
    //the delay and duration will add to the setting of the component
    public void PlayTween(float delay = 0, float duration = 0, bool inverse_curv = false)
    {
        StopTween();

        if (_Phase == Phase.End)
        {
            _IsToPlay = true;

            _InverseCurve = inverse_curv;

            _RealDelay = delay + this.Delay;
            _RealDuration = duration > 0 ? duration : this.Duration;

            enabled = true;
        }
    }

    //Go to the state of a specified time
    //Can be done only when stopped
    public void TakeSample(float normalized_time)
    {
        if (_Phase == Phase.End)
        {
            DoTween(normalized_time);
        }
    }

    public void StopTween()
    {
        if (_Phase != Phase.End)
        {
            InternalStopTween();
        }
    }

    protected void InternalStopTween()
    {
        DoTween(1);

        OnStop();

        _Phase = Phase.End;

        if (OnTweenFinish != null)
        {
            OnTweenFinish.Invoke(this);
        }
        _SelfTime = 0;

        _InverseCurve = false;

        _IsToPlay = false;

        if (!AutoPlay)
        {
            enabled = false;
        }
    }

    protected abstract void DoTween(float nml_time);

    protected virtual void OnStart() { }

    protected virtual void OnStop() { }

    protected float InterpolateValue(float normalized_time, TweenInfo ti)
    {
        float ip_value = 0;

        if (ti.TweenType == TweenType.Curv)
        {
            ip_value = ti.AnimCurv.Evaluate(normalized_time) * (ti.EndValue - ti.StartValue) + ti.StartValue;
        }
        else
        {
            ip_value = normalized_time * (ti.EndValue - ti.StartValue) + ti.StartValue;
        }

        if (_InverseCurve)
        {
            ip_value = 1 - ip_value;
        }

        return ip_value;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (!_IsToPlay && !AutoPlay)
        {
            enabled = false;
        }
        else
        {
            if (_Phase == Phase.End)
            {
                if (!_IsToPlay)
                {
                    _RealDelay = Delay;
                    _RealDuration = Duration;
                }

                _SelfTime = 0;
                _Phase = Phase.Delay;

                OnStart();

                if (InitState)
                {
                    DoTween(0);     //in case it should happen in late update
                }
            }
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (StopWhenDisabled)
        {
            StopTween();
        }
    }

    private void Update()
    {
        ProcessTween();
    }

    private void ProcessTween()
    {
        if (_Phase == Phase.End)
        {
            if (!AutoPlay)
            {
                enabled = false;
            }
            return;
        }

#if ART_USE
        _SelfTime += Time.deltaTime;
#else
        _SelfTime += Time.unscaledDeltaTime;
#endif

        if (_SelfTime < _RealDelay)
        {
            return;
        }

        if (_SelfTime > _RealDelay && _Phase == Phase.Delay)
        {
            _Phase = Phase.Running;
            if (OnTweenStart != null)
            {
                OnTweenStart.Invoke(this);
            }
        }

        if (_SelfTime > _RealDelay + _RealDuration && _Phase == Phase.Running)
        {
            InternalStopTween();
            return;
        }

        DoTween(NormalizedTime);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (Duration <= 0)
        {
            Duration = 1;
        }

        if (Delay < 0)
        {
            Delay = 0;
        }

        //bool is_useful = false;
        //for (int i = 0; i < TweenList.Length; i++)
        //{
        //    if (TweenList[i].IsEnabled)
        //    {
        //        is_useful = true;
        //        break;
        //    }
        //}
        //enabled = is_useful && enabled;
    }

    [NoToLua]
    public virtual string ItemName4Editor(int idx)
    {
        return "item " + idx;
    }

    [NoToLua]
    public void DebugOnTWStart(GTweenUIBase tweenUIBase)
    {
#if ART_USE
        Debug.Log(tweenUIBase.name + " started at " + Time.time);
#else
        Debug.Log(tweenUIBase.name + " started at " + Time.unscaledTime);
#endif
    }

    [NoToLua]
    public void DebugOnTWFinished(GTweenUIBase tweenUIBase)
    {
#if ART_USE
        Debug.Log(tweenUIBase.name + " finished at " + Time.time);
#else
        Debug.Log(tweenUIBase.name + " finished at " + Time.unscaledTime);
#endif
    }

    [NoToLua]
    [ContextMenu("PlayTween")]
    public void PlayTweenInEditor()
    {
        PlayTween();
    }

#endif
}




