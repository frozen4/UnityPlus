#define SPRING_VERSION

using UnityEngine;
using System.Collections;

public class CameraFollowTarget : MonoBehaviour 
{
    public bool _UsingSceneSetting = false;
    public Transform _Target;

    public Vector3 _OffsetDir;
    public float _NormalDistance;

    private Vector3 _Offset;
#if SPRING_VERSION
    private Vector3 _WantedPos;

    private bool _InNormalState = true;
    private float _WantedDistance = 0f;
    private float _FadeInSpeed = 0f;
    private float _FadeOutSpeed = 0f;
    private float _FadeInTime = 0f;
    private float _StayTime = 0f;
    private float _FadeOutTime = 0f;

    private Vector3 _Velocity = Vector3.zero;
#else
    public float HeightDamping = 0.5f;
    private float _RefHigh = 0f;
#endif


    void Start () 
    {
        if (_UsingSceneSetting)
            _OffsetDir = transform.position - _Target.position;

        if (!Util.IsValidDir(ref _OffsetDir))
            _OffsetDir = Vector3.zero;
	}

    void OnEnable()
    {
        if (_Target != null)
            transform.position = _Target.position + _OffsetDir * _NormalDistance;
    }

    void LateUpdate()
    {
        if (!_Target) return;
#if SPRING_VERSION
        if (_InNormalState)
        {
            _Offset = _OffsetDir * _NormalDistance;
            _WantedPos = _Target.position + _Offset;
            transform.position = Vector3.SmoothDamp(transform.position, _WantedPos, ref _Velocity, 0.2f); 
        }
        else
        {
            float now = Time.time;
            if (now < _FadeInTime)
                transform.position -= (Time.deltaTime * _FadeInSpeed * _OffsetDir);
            else if(now < _StayTime)
                transform.position = _Target.position + _OffsetDir * _WantedDistance;
            else if (now < _FadeOutTime)
                transform.position += (Time.deltaTime * _FadeOutSpeed * _OffsetDir);
            else
            {
                transform.position = _Target.position + _OffsetDir * _NormalDistance;
                _InNormalState = true;
            }
        }
#else
        _Offset = _OffsetDir * _NormalDistance;
        float wantedHeight = _Target.position.y + _Offset.y;
        float currentHeight = transform.position.y;

        currentHeight = Mathf.SmoothDamp(currentHeight, wantedHeight, ref _RefHigh, HeightDamping);

        Vector3 position = _Target.position + _Offset;
        position.y = currentHeight;
        transform.position = position;
#endif
    }

    public void SetParams(Transform target, Vector3 offset)
    {
        _Target = target;

        _OffsetDir = offset;
        if (!Util.IsValidDir(ref _OffsetDir, ref _NormalDistance))
            _OffsetDir = Vector3.zero;

        transform.position = _Target.position + offset;
    }

    public void ChangeDistance(float dis_factor, float fadein_time, float stay_time, float fadeout_time)
    {
#if SPRING_VERSION
        if (dis_factor < 1f)
            dis_factor = 1;
        _WantedDistance = _NormalDistance * dis_factor / 100;
        if (fadein_time <= 0.01f) fadein_time = Time.deltaTime;
        _FadeInSpeed = (_NormalDistance - _WantedDistance) / fadein_time;
        if (fadeout_time <= 0.01f) fadeout_time = Time.deltaTime;
        _FadeOutSpeed = (_NormalDistance - _WantedDistance) / fadeout_time;
        _FadeInTime = Time.time + fadein_time;
        _StayTime = _FadeInTime + stay_time;
        _FadeOutTime = _StayTime + fadeout_time;
        _InNormalState = false;
#else
#endif
    }

}
