using UnityEngine;
using Common;

public class CFxFollowTarget : MonoBehaviour
{
    [NoToLua]
    public GameObject FollowTarget;
    [NoToLua]
    public float Distance;
    private float _TargetId = 0;
    private float _Radius = 0;
	private Vector3 _Offset = Vector3.zero;
    private CFxOne _FxOneComp = null;

    public void Apply(bool show, GameObject target, float distance, float targetId)
    {
        if (show)
        {
            if(target == null) return;

            FollowTarget = target;
            Distance = distance;
            _TargetId = targetId;
            var capsuleCollider = target.GetComponentInChildren<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                var height = capsuleCollider.height / 2;
                _Radius = capsuleCollider.radius;
                _Offset.y = height;
            }
            else
            {
                HobaDebuger.LogWarningFormat("EffectiveModel does not have CapsuleCollider: {0}", target.gameObject.name);
                var height = 1;
                _Offset.y = height;
            }            
            if (_FxOneComp != null) _FxOneComp.Play(-1);
        }
        else
        {
            if (_FxOneComp != null) _FxOneComp.Stop();
            FollowTarget = null;
            Distance = 0;
        }
    }

    void OnEnable()
    {
        if (_FxOneComp == null)
            _FxOneComp = gameObject.GetComponent<CFxOne>();
    }

    void Update()
    { 
        var mainCamera = CCamCtrlMan.Instance.MainCamera;
        if (mainCamera == null || !mainCamera.enabled) return;

		if (FollowTarget == null) return;
		if (Util.IsZero(Distance)) return;
        // 新手boss锁定框Y,Z和距离特殊处理。
        if (_TargetId == 40000)
        {
            var capsuleCollider = FollowTarget.GetComponentInChildren<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                var radius = capsuleCollider.radius;
                if (_Radius < radius) 
                {
                    _Offset.y = 3;     
                    _Offset.z = -5;           
                    Distance = 35;
                }
                else
                {
                    _Offset.y = 5;    
                    _Offset.z = 0;             
                    Distance = 25;
                }
            }
        }

		Vector3 position = FollowTarget.transform.position;
		Vector3 dir = (mainCamera.transform.position - position).normalized;   
		Vector3 cam_dist =  (dir * Distance) + position;
		transform.localPosition = cam_dist + _Offset;
    }
}