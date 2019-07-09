using UnityEngine;
using System;
using Common;

public class ArcReactorInfo
{
    public Transform StartTrans = null;
    public Transform EndTrans = null;

    public void Set(Transform start, Transform end)
    {
        StartTrans = start;
        EndTrans = end;
    }

    public bool IsValid()
    {
        return StartTrans != null && EndTrans != null;
    }
}

public class EarlyWarningInfo
{
    public float StartTime = 0;
    public float Duration = 0;
    public Vector3 Scale = Vector3.zero;
    public float OuterR = 0;
    public float InnerR = 0;
    public bool DoNotUseProjector = false;
    public void Set(float t, float d, Vector3 s, bool doNotUseProjector)
    {
        StartTime = t;
        Duration = d;
        Scale = s;
        OuterR = s.x;
        InnerR = s.z;
        DoNotUseProjector = doNotUseProjector;
    }

    public bool IsValid()
    {
        return StartTime > 0 && Duration > 0 && !Scale.IsZero();
    }
}

public class RadialBlurBootInfo
{
    public float FadeIn = 0;
    public float Duration = 0;
    public float FadeOut = 0;
    public float Intensity = 0;
    public float Radius = 0;

    public void Set(float fade_in, float duration, float fade_out, float intensity, float radius)
    {
        FadeIn = fade_in;
        Duration = duration;
        FadeOut = fade_out;
        Intensity = intensity;
        Radius = radius;
    }

    public bool IsValid()
    {
        return Intensity > 0 && Duration > 0 && Radius > 0;
    }
}

public delegate void GFXActionEventHandler(CFxOne sender);

public class CFxOne : MonoBehaviour 
{
    [NoToLua]
	public string Name {get; set;}
    [NoToLua]
    public int ID = 0;
    [NoToLua]
    public float Duration = -1;   //set by fx ,can overwrite by user
    [NoToLua]
    public int Priority = 50;     //值越大，优先级越低

    [NoToLua]
    public bool IsFixRot = false;
    [NoToLua]
    public bool IsCached = false;

    [NoToLua]
    public GFXActionEventHandler OnPlay;
    [NoToLua]
    public GFXActionEventHandler OnStop;

    // 仅在ArcFx下使用
    [NoToLua]
    public ArcReactorInfo ArcReactor = null;

    // 仅在预警效果下生效
    [NoToLua]
    public EarlyWarningInfo EarlyWarning = null;

    // 仅在特效具有Blur效果时生效
    [NoToLua]
    public RadialBlurBootInfo RadialBlurBoot = null;

    private bool _IsPlaying = false;
    private float _StartTime;
    private Quaternion _BeginRotation;
    private float _CorrectBluntSpeed = 0;
    private bool _Is2Destroy = false;
    private GameObject _RealFxGameObject = null;
    private CFxProxy _FxProxy = null;
    private float _PlaySpeed = -1;

    [NoToLua]
    public float RealScale = 1f;

    [NoToLua]
    public float StartTime
    {
        get { return _StartTime; }
    }

    public bool IsPlaying
    {
        get { return _IsPlaying; } 
    }

    public void Play(float duration)
    {
        if (_IsPlaying) return;  //特效的重复play是无效的，只能调用Stop停止播放被管理器回收

        _PlaySpeed = -1;
        _IsPlaying = true;

        if (IsInvoking("OnLifeEnd"))
            CancelInvoke("OnLifeEnd");

        _StartTime = Time.time;

        Duration = -1;
        if (duration >= 0)
            Duration = duration;
        if (Duration >= 0.0f)
            Invoke("OnLifeEnd", Duration);

        if (OnPlay != null)
            OnPlay(this);

        if (IsFixRot)
        {
            transform.localRotation = Quaternion.identity;
            _BeginRotation = transform.rotation;
        }

        DoRealPlay();
    }

    public void Stop()
    {
        if (IsInvoking())
            CancelInvoke();

        OnLifeEnd();
    }

    [NoToLua]
    public void DoRealPlay()
    {
        if (_RealFxGameObject == null) return;

        SetScale(RealScale);

        if (_FxProxy != null)
        {
            var lodLv = GFXConfig.Instance.GetFxLODLevel();
            _FxProxy.Active(lodLv);
        }

        if (_PlaySpeed > 0)
            ChangePlaySpeed(_PlaySpeed);

        // 是闪电链特效
        #region ArcReactor
        if (ArcReactor != null && ArcReactor.IsValid() && _FxProxy != null)
        {
            var arcComp = _FxProxy.GetArcReactor();
            if (null != arcComp)
            {
                arcComp.SetData(ArcReactor.StartTrans, ArcReactor.EndTrans);
                arcComp.SetActive(true);
            }
        }
        #endregion

        // 是预警特效
        #region EarlyWarning  
        if (EntryPoint.Instance.DebugOptionShowProjector && EarlyWarning != null && EarlyWarning.IsValid() && _FxProxy != null)
        {
            var ew = _FxProxy.GetYujingProgressComp();
            if (null != ew)
            {
                ew.SetData(EarlyWarning.Scale, EarlyWarning.StartTime, EarlyWarning.Duration, EarlyWarning.Scale.x, EarlyWarning.Scale.z, EarlyWarning.DoNotUseProjector);
                ew.SetActive(true);
            }
        }
        #endregion

        // 特效Blur
        #region RadialBlurBoot
        if (RadialBlurBoot != null && RadialBlurBoot.IsValid())
            EnableRadialBlur();
        #endregion

        // 如果处于顿帧中
        if (_CorrectBluntSpeed > 0)
        {
            if (_FxProxy != null)
                _FxProxy.Pause();
        }
    }

    [NoToLua]
    public void Blunt(float bluntTime, float fixSpeed)
    {
        if (!_IsPlaying) return;

        if (_FxProxy != null)
            _FxProxy.Pause();

        _CorrectBluntSpeed = fixSpeed;

        if (IsInvoking("ExitBluntState"))
            CancelInvoke("ExitBluntState");
        Invoke("ExitBluntState", bluntTime);
    }

    [NoToLua]
    public void EnableRadialBlur()
    {
        if (_RealFxGameObject == null) return;
        if (RadialBlurBoot == null || !RadialBlurBoot.IsValid()) return;

        RadialBlurBoot radialBlurComp = null;
        if (_FxProxy != null)
            radialBlurComp = _FxProxy.GetRadialBlurBootComp();
        if(radialBlurComp != null)
        {
            radialBlurComp.enabled = true;
            radialBlurComp.StartEffect(RadialBlurBoot.FadeIn, RadialBlurBoot.Duration, RadialBlurBoot.FadeOut, RadialBlurBoot.Intensity, RadialBlurBoot.Radius);
        }
        else
        {
            Common.HobaDebuger.LogWarningFormat("Can not find HangPoint_Blur at {0}", _RealFxGameObject.name);
        }
        RadialBlurBoot.Set(0, 0, 0, 0, 0);
    }

    [NoToLua]
    public void SetScale(float s)
    {
        RealScale = s;
        if (_FxProxy != null)
            _FxProxy.SetScale(s);
    }

    [NoToLua]
    public void Tick(float dt)
    {
        if (IsFixRot)
            transform.rotation = _BeginRotation;

        if (_FxProxy != null)
            _FxProxy.Tick(dt);
    }

    [NoToLua]
    public void LateTick(float dt)
    {
        //if (_FxProxy != null)
        //    _FxProxy.LateTick(dt);
    }

    protected void OnLifeEnd()
	{
        if (IsCached && ID == 0) return;

        _IsPlaying = false;
	    _PlaySpeed = -1;
        Priority = 50;
        //gameObject.SetActive(false);
        ID = 0;
        RealScale = 1f;
        transform.localScale = Vector3.one;

        if (ArcReactor != null)
            ArcReactor.Set(null, null);

        if (EarlyWarning != null)
            EarlyWarning.Set(-1, 0, Vector3.zero,false);

        if (RadialBlurBoot != null)
            RadialBlurBoot.Set(0, 0, 0, 0, 0);

        _CorrectBluntSpeed = 0f;
        if (IsInvoking("ExitBluntState"))
            CancelInvoke("ExitBluntState");

        if(_FxProxy != null)
            _FxProxy.Deactive();

        if (OnStop != null)
            OnStop(this);

        OnPlay = null;
        OnStop = null;
    }

    private void ExitBluntState()
    {
        if(_CorrectBluntSpeed > 0 && _FxProxy != null)
            _FxProxy.Restart(_CorrectBluntSpeed);

        _CorrectBluntSpeed = 0f;

        if (IsInvoking("ExitBluntState"))
            CancelInvoke("ExitBluntState");
    }

    [NoToLua]
    public GameObject GetFxGameObject()
    {
        return _RealFxGameObject;
    }

    [NoToLua]
    public void SetFxGameObject(GameObject fx)
    {
        _RealFxGameObject = fx;

        if (_RealFxGameObject != null)
        {
            _FxProxy = _RealFxGameObject.GetComponent<CFxProxy>();
            if (_FxProxy == null)
            {
                _FxProxy = _RealFxGameObject.AddComponent<CFxProxy>();
                _FxProxy.Init();
            }
        }
        else
        {
            _FxProxy = null;
        }
            
    }

    [NoToLua]
    public void Active(bool active, EFxLODLevel lv = EFxLODLevel.L0)
    {
        if (_FxProxy != null)
        {
            if(active)
                _FxProxy.Active(lv);
            else
                _FxProxy.Deactive();
        }
    }

    [NoToLua]
    public void ChangePlaySpeed(float speed)
    {
        if (!_IsPlaying) return;

        _PlaySpeed = speed;

        if (_FxProxy != null)
        {
            // 即时生效，无需记录
            _PlaySpeed = -1;
            _FxProxy.ChangePlaySpeed(speed, speed * 0.5f);  // 0.5 是一个经验系数
        }
    }

    #region UNITY函数区
    void OnDestroy()
    {
        // 如果是CacheMan主动清理，则不走以下提示逻辑
        if (_Is2Destroy /*|| EntryPoint.Instance.IsQuiting*/) return;

        if (_IsPlaying && IsCached)
        {
            // 如果 “非技能结束时被打断”的特效，正好在技能结束和特效结束中间间隔 关闭游戏，则会出现调用至此
            Stop();
            CFxCacheMan.Instance.RemoveFxOneUsingTheWrongWay(this);
        }
    }
    #endregion
}
