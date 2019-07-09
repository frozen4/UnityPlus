using UnityEngine;
using LuaInterface;

public abstract class CMotor : MonoBehaviour
{ 
    protected Vector3 _StartPos;
	protected Transform _DestTrans;
    protected Vector3 _DestPos;
    protected float _HorzSpeed = 3.0f;
    protected float _Tolerance = 0.2f;
    protected LuaFunction _CallbackFun = null;

    protected bool _IsArrived = false;
    protected float _LifeTime = -1;

    protected abstract bool OnMove(float dt);
	protected virtual void OnStart()
    {
        _LifeTime = Util.DistanceH(_StartPos, _DestPos) / _HorzSpeed;
        _IsArrived = false;
    }
    protected Vector3 GetDestPos()
	{
		if (_DestTrans == null)
			return _DestPos;
		_DestPos = _DestTrans.position;
		return _DestPos;
	}
	
	void Update() 
    {
        if (_IsArrived) return;

        float delta = Time.deltaTime;
		_LifeTime -= delta;

		bool ret = OnMove(delta);
        if (ret) return;

        bool time_out = _LifeTime < 0.0f;
        _IsArrived = (_DestTrans != null || time_out);
        if (_IsArrived)
        {
            if (_CallbackFun != null)
                _CallbackFun.Call(gameObject, time_out);            
        }
	}

    public void Fly(Vector3 from, GameObject destGo, float speed, float tolerance, LuaFunction cb)
	{
		_StartPos = from;
		_HorzSpeed = speed > 0.001f ? speed : 0.001f;

        if (_CallbackFun != null)
            _CallbackFun.Release();
        _CallbackFun = cb;

		_Tolerance = tolerance;

        _DestTrans = null;
        if(destGo != null)
            _DestTrans = destGo.transform;

        if (_DestTrans != null)
        {
            _DestPos = _DestTrans.position;
        }
        else
        {
            _DestPos = Vector3.zero;
            Common.HobaDebuger.LogWarningFormat("{0}'s target is null when call fly", gameObject.name);
        }

        OnStart();
    }

    public void Fly(Vector3 from, Vector3 destPos, float speed, float tolerance, LuaFunction cb)
	{
		_StartPos = from;
        _HorzSpeed = speed > 0.001f ? speed : 0.001f;

        if (_CallbackFun != null)
            _CallbackFun.Release();
        _CallbackFun = cb;
		_Tolerance = tolerance;
        _DestTrans = null;
		_DestPos = destPos;

        OnStart();
    }

    void OnDestroy()
    {
        if (_CallbackFun != null)
        {
            _CallbackFun.Release();
            _CallbackFun = null;
        }

        _DestTrans = null;
    }
}
