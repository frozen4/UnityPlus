using UnityEngine;
using System.Collections.Generic;

public class FxYujingProgressEx : ReusableFx
{
    public enum YujingType
    {
        Ring,     // 实心环形
        Fan,      // 扇形
        Juxing,   // Rect 
        Hollow,   // 带空洞的环形
    }

    public bool DoNotUseProjector = false;
    public GameObject ScaledInnerObject = null;

    public Projector GameObjProgress = null;
    public List<Projector> ProjectorList = new List<Projector>();
    public Vector3 Scale = Vector3.one;             //x: 宽，z: 长
    public YujingType Type = YujingType.Ring;

    private float _StartTime = -1;
    private float _Duration = 0.0f;

    private Material _InnerObjectMaterial = null;
    private MeshRenderer _InnerObjectRenderer = null;
    private Material _OrignalInnerMaterial = null;
    private float _MainScaleInterval = 0f;
    private float _StartMainScaleValue = 0f;

    private void Awake()
    {
        if(ScaledInnerObject != null && DoNotUseProjector)
        {
            if (Type == YujingType.Hollow)
            {
                var render = ScaledInnerObject.GetComponent<MeshRenderer>();
                _InnerObjectRenderer = render;
                _OrignalInnerMaterial = render != null ? render.material : null;
            }

            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                child.localScale = Vector3.one;
            }
        }
    }

    public void SetData(Vector3 scale, float startTime, float duration, float outRadius, float innerRadius, bool doNotUseProjector)
    {
        if (DoNotUseProjector != doNotUseProjector)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Common.HobaDebuger.LogWarningFormat("预警特效使用参数和资源配置参数不匹配,{0}", gameObject.name);
#endif
        }

        Scale = scale;
        _StartTime = startTime;
        _Duration = duration;

        if (Type == YujingType.Hollow)
        {
            if (!DoNotUseProjector)
            {
                if (null != GameObjProgress )
                {
                    GameObjProgress.orthographicSize = outRadius;

                    var innerParam = innerRadius / outRadius * 0.5f;
                    GameObjProgress.material.SetFloat(ShaderIDs.InnerRadius, innerParam);

                    var startScale = Mathf.Cos(1 - (2 * innerParam - 0.1f + innerParam / 5));
                    GameObjProgress.material.SetFloat(ShaderIDs.MainScaler, startScale);
                    _StartMainScaleValue = startScale;
                    _MainScaleInterval = 1 - startScale;
                }
            }
            else
            {
                if (_InnerObjectRenderer != null && _OrignalInnerMaterial != null)
                {
                    _InnerObjectMaterial = MaterialPool.Instance.Get(_OrignalInnerMaterial);
                    _InnerObjectRenderer.sharedMaterial = _InnerObjectMaterial;

                    // 前提：特效制作按照标准1X1尺寸制作
                    // outRadius控制整体大小，通过localScale来设置
                    var size = scale.x * 2;
                    transform.localScale = new Vector3(size, size, size);
                    // 内径参数 = innerRadius / outRadius * 0.5f 
                    var innerParam = innerRadius / outRadius * 0.5f;
                    _InnerObjectMaterial.SetFloat(ShaderIDs.InnerRadius, innerParam);
                    var startScale = Mathf.Cos(1 - (2 * innerParam - 0.1f + innerParam / 5));
                    _InnerObjectMaterial.SetFloat(ShaderIDs.MainScaler, startScale);
                    _StartMainScaleValue = startScale;
                    _MainScaleInterval = 1 - startScale;
                }
            }
        }
        else if (Type == YujingType.Ring)
        {
            if (DoNotUseProjector)
            {
                var size = outRadius * 2;
                transform.localScale = new Vector3(size, size, size);
            }
        }
        else if (Type == YujingType.Fan)
        {
            if (DoNotUseProjector)
                transform.localScale = new Vector3(Scale.x, Scale.x, Scale.x);
        }
        else if (Type == YujingType.Juxing)
        {
            if (DoNotUseProjector)
                transform.localScale = Scale;
        }
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);

        if (!active && _InnerObjectMaterial != null && DoNotUseProjector)
        {
            _InnerObjectRenderer.sharedMaterial = _OrignalInnerMaterial;
            MaterialPool.Instance.Recycle(_InnerObjectMaterial);
            _InnerObjectMaterial = null;
        }
    }

    public override void Tick(float dt)
    {
        if(!enabled) return;

        if (_StartTime < 0f || _Duration <= 0.001f)
            return;

        if (Time.time > _StartTime + _Duration)
            return;

        var progress = (Time.time - _StartTime) / _Duration;
        progress = Mathf.Clamp(progress, 0.0f, 1.0f);

        if (DoNotUseProjector)
        {
            if (ScaledInnerObject == null) return;
            switch (Type)
            {
                case YujingType.Ring:
                    ScaledInnerObject.transform.localScale = new Vector3(progress, progress, progress);
                    break;
                case YujingType.Fan:
                    ScaledInnerObject.transform.localScale = new Vector3(progress, progress, progress);
                    break;
                case YujingType.Juxing:
                    ScaledInnerObject.transform.localScale = new Vector3(1, progress,1);  // 美术资源做了旋转，所以调整Y变量
                    break;
                case YujingType.Hollow:
                    var scale = _StartMainScaleValue + _MainScaleInterval * progress;
                    _InnerObjectMaterial.SetFloat(ShaderIDs.MainScaler, scale);
                    break;
            }
        }
        else
        {
            if (GameObjProgress == null) return;

            float width = Scale.x;
            switch (Type)
            {
                case YujingType.Ring:
                    GameObjProgress.orthographicSize = progress * width;
                    foreach (var v in ProjectorList) v.orthographicSize = width;
                    break;
                case YujingType.Fan:
                    GameObjProgress.orthographicSize = progress * width;
                    foreach (var v in ProjectorList) v.orthographicSize = width;
                    break;
                case YujingType.Juxing:
                    float length = width > 0 ? Scale.z / width : Scale.z;
                    GameObjProgress.orthographicSize = width;
                    GameObjProgress.aspectRatio = progress * length;
                    foreach (var v in ProjectorList)
                    {
                        v.orthographicSize = width;
                        v.aspectRatio = length;
                    }
                    break;
                case YujingType.Hollow:
                    var scale = _StartMainScaleValue + _MainScaleInterval * progress;
                    GameObjProgress.material.SetFloat(ShaderIDs.MainScaler, scale);
                    break;
            }
        }
    }

    void Update()
    {
        Tick(Time.deltaTime);
    }
}

