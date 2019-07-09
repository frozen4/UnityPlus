using UnityEngine;

[System.Serializable]
public class ComponentInfo
{
    [System.Serializable]
    public struct ParticleSystemInfo
    {
        public ParticleSystem Comp;
        public float Delay;
    }
    public ParticleSystemInfo[] ParticleSystems;

    [System.Serializable]
    public struct AnimatorInfo
    {
        public Animator Comp;
        public float Delay;
    }
    public AnimatorInfo[] Animators;

    [System.Serializable]
    public struct MeshRendererInfo
    {
        public MeshRenderer Comp;
        public float Delay;
    }
    public MeshRendererInfo[] Renderers;

    [System.Serializable]
    public struct TrailRendererInfo
    {
        public TrailRenderer Comp;
        public float Delay;
    }
    public TrailRendererInfo[] TrailRenderers;

    [System.Serializable]
    public struct ProjectorInfo
    {
        public Projector Comp;
        public float Delay;
    }
    public ProjectorInfo[] Projectors;

    [System.Serializable]
    // 第三方组件
    public struct ReusableFxCompInfo
    {
        public ReusableFx Comp;
        public float Delay;
    }
    public ReusableFxCompInfo[] ReusableFxCompnents;

    public string PrefabName;

    // 特效缩放逻辑使用
    public RFX4_ScaleCurves[] ScaleCurves;

    private bool[] _ParticleSystemEnableState = null;
    private float[] _ProjectorStartScale = null;

    private static int MaxActiveProjectorCount = 3;
    private static int _CurActiveProjectorCount = 0;

    public void TryEnable(bool enabled, float curLastTime)
    {
        if (_ParticleSystemEnableState == null && ParticleSystems.Length > 0)
            _ParticleSystemEnableState = new bool[ParticleSystems.Length];

        if (_ProjectorStartScale == null && Projectors.Length > 0)
        {
            _ProjectorStartScale = new float[Projectors.Length];
            for (int i = 0; i < Projectors.Length; i++)
            {
                var v = Projectors[i];
                if (v.Comp != null)
                    _ProjectorStartScale[i] = v.Comp.orthographicSize;
            }
        }

        if (enabled)
        {
            for (int i = 0; i < ParticleSystems.Length; i++)
            {
                var v = ParticleSystems[i];
                if (v.Comp != null && !_ParticleSystemEnableState[i] && curLastTime >= v.Delay)
                {
                    v.Comp.Play();
                    _ParticleSystemEnableState[i] = true;
                }
            }

            foreach (var v in Animators)
            {
                if (v.Comp != null && !v.Comp.enabled && curLastTime >= v.Delay)
                {
                    v.Comp.enabled = true;
                    var info = v.Comp.GetCurrentAnimatorStateInfo(0);
                    if (info.fullPathHash != 0)
                        v.Comp.Play(info.fullPathHash, 0, 0);
                    else
                        Common.HobaDebuger.LogWarningFormat("AnimatorComp of Gfx {0} has no Current AnimatorState", PrefabName);

                }
            }

            foreach (var v in Projectors)
            {
                if (v.Comp != null && !v.Comp.enabled && curLastTime >= v.Delay)
                {
                    if(_CurActiveProjectorCount < MaxActiveProjectorCount)
                    {
                        v.Comp.enabled = true;
                        //_CurActiveProjectorCount++;
                    }
                }
            }

            foreach (var v in Renderers)
            {
                if (v.Comp != null && !v.Comp.enabled && curLastTime >= v.Delay)
                    v.Comp.enabled = true;
            }

            foreach (var v in TrailRenderers)
            {
                if (v.Comp != null && !v.Comp.enabled && curLastTime >= v.Delay)
                    v.Comp.enabled = true;
            }

            foreach (var v in ReusableFxCompnents)
            {
                if (v.Comp != null && !v.Comp.enabled && curLastTime >= v.Delay)
                    v.Comp.SetActive(true);
            } 
        }
        else
        {
            for (int i = 0; i < ParticleSystems.Length; i++)
            {
                var v = ParticleSystems[i];
                if (v.Comp != null)
                {
                    v.Comp.Stop();
                    v.Comp.Clear();
                }

                _ParticleSystemEnableState[i] = false;
            }


            foreach (var v in Animators)
            {
                if (v.Comp != null)
                    v.Comp.enabled = false;
            }

            foreach (var v in Projectors)
            {
                if (v.Comp != null && v.Comp.enabled)
                {
                    v.Comp.enabled = false;
                    _CurActiveProjectorCount--;
                    if (_CurActiveProjectorCount < 0)
                        _CurActiveProjectorCount = 0;
                }
            }

            foreach (var v in Renderers)
            {
                if (v.Comp != null)
                    v.Comp.enabled = false;
            }

            foreach (var v in TrailRenderers)
            {
                if (v.Comp != null)
                {
                    v.Comp.Clear();
                    v.Comp.enabled = false;
                }
            }

            foreach (var v in ReusableFxCompnents)
            {
                if (v.Comp != null)
                    v.Comp.SetActive(false);
            }
        }
    }

    public void SetScale(float scale)
    {
        foreach (var v in ScaleCurves)
            v.SetScaleFactor(scale);

        if (_ProjectorStartScale != null)
        {
            for (int i = 0; i < Projectors.Length; i++)
            {
                var v = Projectors[i];
                if (v.Comp != null)
                    v.Comp.orthographicSize = scale * _ProjectorStartScale[i];
            }
        }
    }

    public bool IsValid()
    {
        return ParticleSystems.Length > 0
               || Renderers.Length > 0
               || TrailRenderers.Length > 0
               || Projectors.Length > 0
               || ReusableFxCompnents.Length > 0;
    }
}

public class FxComponentInfo : MonoBehaviour
{
    public ComponentInfo[] FxComponentInfos;

    public void SetPrefabName(string name)
    {
        foreach (var v in FxComponentInfos)
        {
            if (v != null)
                v.PrefabName = name;
        }
    }

    public void TryEnable(EFxLODLevel lv, bool enable, float curLastTime)
    {
        if (!enable && lv == EFxLODLevel.All)
        {
            foreach (var v in FxComponentInfos)
                v.TryEnable(false, 0);
        }
        else if (lv != EFxLODLevel.All)
        {
            if(FxComponentInfos.Length == 1 && FxComponentInfos[0] != null)
                FxComponentInfos[0].TryEnable(enable, curLastTime);
            else if (FxComponentInfos.Length == 3)
            {
                for (int i = (int)lv; i >= 0 ; i--)
                {
                    // 如果当前LOD等级无有效资源，则降一级查找；直至找到有效资源
                    if (FxComponentInfos[i].IsValid())
                    {
                        FxComponentInfos[i].TryEnable(enable, curLastTime);
                        break;
                    }
                }
            }
        }
    }

    public void SetScale(EFxLODLevel lv, float scale)
    {
        transform.localScale = new Vector3(scale, scale, scale);

        if (FxComponentInfos.Length == 1 && FxComponentInfos[0] != null)
            FxComponentInfos[0].SetScale(scale);

        if (FxComponentInfos.Length == 3)
        {
            for (int i = (int)lv; i >= 0; i--)
            {
                // 如果当前LOD等级无有效资源，则降一级查找；直至找到有效资源
                if (FxComponentInfos[i] != null)
                {
                    FxComponentInfos[i].SetScale(scale);
                    return;
                }
            }
        }
    }

    public void ChangePlaySpeed(EFxLODLevel lv, float psSpeed, float animatorSpeed)
    {
        ComponentInfo curInfo = null;
        if (FxComponentInfos.Length == 1 && FxComponentInfos[0] != null)
        {
            curInfo = FxComponentInfos[0];
        }
        else if (FxComponentInfos.Length == 3)
        {
            for (int i = (int)lv; i >= 0; i--)
            {
                // 如果当前LOD等级无有效资源，则降一级查找；直至找到有效资源
                if (FxComponentInfos[i] != null)
                {
                    curInfo = FxComponentInfos[i];
                    break;
                }
            }
        }

        if (curInfo != null)
        {
            foreach (var v in curInfo.ParticleSystems)
            {
                if (v.Comp != null)
                {
                    if (psSpeed < Util.FloatZero)
                    {
                        v.Comp.Pause();
                    }
                    else
                    {
                        var mainModule = v.Comp.main;
                        mainModule.simulationSpeed = psSpeed;
                        if (!v.Comp.isPlaying)
                            v.Comp.Play();
                    }
                }
            }

            foreach (var v in curInfo.Animators)
            {
                if (v.Comp != null)
                    v.Comp.speed = animatorSpeed;
            }
        }
    }
}
