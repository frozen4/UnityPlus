using System.Collections.Generic;
using UnityEngine;

public class CFxProxy : MonoBehaviour
{
    /// <summary>
    ///  1 对于Fx美术资源中所使用组件的统一管理
    ///  2 复用逻辑的统一操作入口 
    ///     CFxOne -> CFxProxy -> (FxLOD + Delay + MeshRender + Animator + ParticalSystem + XWeaponTrail + XffectComponent)
    ///  
    /// Why?
    /// 1 当前版本中的特效复用管理效率较低，各种Active/Deactive操作引起
    /// 2 对于Active/Deactive操作者较多(CFxOne + FxLOD + Delay + ...) 逻辑混乱
    /// </summary>

    private EFxLODLevel _CurLODLevel = EFxLODLevel.Invaild;
    private float _ActiveBeginTime = 0;
    private FxComponentInfo _ComponentsInfo = null;

    private ArcReactor_Arc _ArcReactorComp = null;
    private FxYujingProgressEx _FxYujingProgressComp = null;
    private RadialBlurBoot _RadialBlurBootComp = null;

    public void Init()
    {
        _ComponentsInfo = gameObject.GetComponent<FxComponentInfo>();

        if (_ComponentsInfo != null)
        {
            _ComponentsInfo.SetPrefabName(gameObject.name);
            _ComponentsInfo.TryEnable(EFxLODLevel.All, false, 0);
        }
        else
        {
            Common.HobaDebuger.LogWarningFormat("gfx {0} does not have FxComponentInfo", gameObject.name);
        }
    }

    public void Active(EFxLODLevel lv)
    {
        if(lv == _CurLODLevel) return;

        _ActiveBeginTime = Time.time;
        _CurLODLevel = lv;

        if (_ComponentsInfo != null)
            _ComponentsInfo.TryEnable(lv, true, Util.FloatZero);
    }

    public void Deactive()
    {
        if (_ComponentsInfo != null)
            _ComponentsInfo.TryEnable(_CurLODLevel, false, 0);

        if (_RadialBlurBootComp != null)
            _RadialBlurBootComp.enabled = false;

        _ActiveBeginTime = 0;
        _CurLODLevel = EFxLODLevel.Invaild;
    }

    public void Tick(float dt)
    {
        if (_CurLODLevel == EFxLODLevel.Invaild) return;

        if (_ComponentsInfo != null)
        {
            var lastTime = Time.time - _ActiveBeginTime;
            _ComponentsInfo.TryEnable(_CurLODLevel, true, lastTime);
        }
    }

    public void Pause()
    {
        ChangePlaySpeed(0, 0);
    }

    public void Restart(float speed)
    {
        ChangePlaySpeed(speed, speed);
    }

    public void ChangePlaySpeed(float psSpeed, float animatorSpeed)
    {
        if (_CurLODLevel == EFxLODLevel.Invaild) return;

        // 其他第三方插件制作的特效暂不支持
        if (_ComponentsInfo != null)
            _ComponentsInfo.ChangePlaySpeed(_CurLODLevel, psSpeed, animatorSpeed);
    }

    public void SetScale(float scale)
    {
        if (_CurLODLevel == EFxLODLevel.Invaild) return;

        if (_ComponentsInfo != null)
            _ComponentsInfo.SetScale(_CurLODLevel, scale);
    }

    public ArcReactor_Arc GetArcReactor()
    {
        if (_ArcReactorComp != null) return _ArcReactorComp;

        _ArcReactorComp = gameObject.GetComponentInChildren<ArcReactor_Arc>();
        return _ArcReactorComp;
    }

    public FxYujingProgressEx GetYujingProgressComp()
    {
        if (_FxYujingProgressComp != null) return _FxYujingProgressComp;

        _FxYujingProgressComp = gameObject.GetComponent<FxYujingProgressEx>();
        return _FxYujingProgressComp;
    }

    public RadialBlurBoot GetRadialBlurBootComp()
    {
        if (_CurLODLevel == EFxLODLevel.Invaild) return null;

        if (_RadialBlurBootComp != null) return _RadialBlurBootComp;

        var root = gameObject;
        var hangpoint = root.FindChildRecursively("HangPoint_Blur");
        if (hangpoint != null)
        {
            _RadialBlurBootComp = hangpoint.GetComponent<RadialBlurBoot>();
            if (null == _RadialBlurBootComp)
                _RadialBlurBootComp = hangpoint.AddComponent<RadialBlurBoot>();
        }

        return _RadialBlurBootComp;
    }
}
