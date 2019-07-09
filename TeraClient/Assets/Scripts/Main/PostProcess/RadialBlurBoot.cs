using UnityEngine;

public class RadialBlurBoot : MonoBehaviour
{
    Camera _MainCamera;
    PostProcessChain _PostProcess;

    float _Duration, _FadeIn, _FadeOut, _Elapse;
    float _Intensity;
    float _Radius;
    bool _OnActive = false;

    private void Awake()
    {
        _MainCamera = Main.Main3DCamera;
        _PostProcess = Main.MainPostProcessChain;
    }

    void OnDisable()
    {
        StopEffect();       
    }

    void OnDestroy()
    {
        StopEffect();
    }

    void Update()
    {
        if (_MainCamera != null && _PostProcess != null)
        {
            if (_Duration > 0 && _Elapse < _Duration)
            {
                float intensity = _Intensity;

                float t = _Elapse / _Duration;
                if (t < _FadeIn)
                {
                    intensity *= Mathf.Clamp01(t / _FadeIn);
                }
                else if (t > _FadeOut)
                {
                    intensity *= Mathf.Clamp01(((1 - t) / (1 - _FadeOut)));
                }

                Vector3 spos = _MainCamera.WorldToScreenPoint(transform.position);
                float u = spos.x / Screen.width;
                float v = spos.y / Screen.height;
                _PostProcess.radial_blur_pos = new Vector2(u, v);

                _PostProcess.radial_blur_paramters.x = intensity;
                _PostProcess.radial_blur_paramters.y = _Radius;

                _Elapse += Time.deltaTime;
            }
            else
            {
                StopEffect();
            }
        }
        else
        {
            StopEffect();
        }
    }

    public void StartEffect(float fade_in, float duration, float fade_out, float intensity, float radius)
    {

        if (null == _MainCamera || null == _PostProcess)
            return;
            
        _Elapse = 0;

        _FadeIn = fade_in;
        _Duration = duration;
        _FadeOut = fade_out;

        _Intensity = intensity;
        _Radius = radius;

        enabled = true;
        _PostProcess.EnableRadialBlur = true;
        _OnActive = true;

    }

    public void StopEffect()
    {
        if (!_OnActive)
            return;
        enabled = false;
        if(null != _PostProcess)
        {
            _PostProcess.EnableRadialBlur = false;
        }
        _OnActive = false;
    }
}
