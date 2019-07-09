//using System.Collections.Generic;
using UnityEngine;
using System;
using EntityComponent;

public class AdsorbBehavior : Behavior
{
    struct AdsorbInfo:IEquatable<AdsorbInfo>
    {
        public int OriginId;
        public float Speed;
        public Vector3 Position;

        public AdsorbInfo(int origin, float speed, Vector3 pos)
        {
            OriginId = origin;
            Speed = speed;
            Position = pos;
        }
        public bool Equals(AdsorbInfo other)
        {
            //throw new NotImplementedException();
            if (!(OriginId.Equals(other.OriginId)))
                return false;
            if (!(Speed.Equals(other.Speed)))
                return false;
            if (!(Position.Equals(other.Position)))
                return false;
            else
                return true;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is AdsorbInfo))
                return false;
            return Equals((AdsorbInfo)obj);

        }
        public override int GetHashCode()
        {
            return OriginId.GetHashCode() ^ Speed.GetHashCode() ^ Position.GetHashCode();
        }
    };

    /*
     * 由于多个吸附叠加运算容易导致不同步现象，所以简化为同时可以存在多个吸附操作，但是只有一个生效。
     * 服务器选择速度快距离最近的的吸附通知客户端，吸附结束或者有新的吸附时进行再次选择并通知客户端，直达所有的吸
     * 附操作执行完毕通知客户端结束
     */
    //private readonly List<AdsorbInfo> _AdsorbInfoList = new List<AdsorbInfo>();
    private AdsorbInfo _AdsorbInfo;

    public Vector3 Velocity
    {
        get
        {
            var adsorbDir = _AdsorbInfo.Position - _Owner.position;
            adsorbDir.y = 0;
            adsorbDir.Normalize();
            return adsorbDir * _AdsorbInfo.Speed;
        }
    }

    public AdsorbBehavior()
        : base(BehaviorType.Adsorb)
    {
    }

    public void AddAdsorbData(int origin, float speed, Vector3 position)
    {
        //var info = new AdsorbInfo(origin, speed, position);
        //_AdsorbInfoList.Add(info);
        _AdsorbInfo.OriginId = origin;
        _AdsorbInfo.Speed = speed;
        _AdsorbInfo.Position = position;

        // NPC/Monster在移动中进入吸附 不会主动发送StopMove协议；需要客户端处理
        if (_ObjectComponent == null)
            _ObjectComponent = _Owner.GetComponent<ObjectBehaviour>();
        if (_ObjectComponent != null
            && _OwnerType != ObjectBehaviour.OBJ_TYPE.HOSTPLAYER 
            && _OwnerType != ObjectBehaviour.OBJ_TYPE.ELSEPLAYER
            && _ObjectComponent.HasBehavior(BehaviorType.Move))
        {
            _ObjectComponent.RemoveBehavior(BehaviorType.Move);
        }
    }

    public override bool Tick(float dt)
    {
        if (_ObjectComponent == null)
            _ObjectComponent = _Owner.GetComponent<ObjectBehaviour>();

        // 其他玩家，如果吸附中移动，以移动位置点为准；不移动时，吸附效果为准。
        if (_OwnerType == ObjectBehaviour.OBJ_TYPE.ELSEPLAYER && _ObjectComponent.HasBehavior(BehaviorType.Move))
            return false;

        var adsorbDir = _AdsorbInfo.Position - _Owner.position;
        adsorbDir.y = 0;
        adsorbDir = adsorbDir.normalized;

        var moveStep = adsorbDir * _AdsorbInfo.Speed * dt;

        Vector3 newPos;
        if (Util.DistanceH(_AdsorbInfo.Position, _Owner.position) < Util.MagnitudeH(moveStep))
            newPos = _AdsorbInfo.Position;
        else
            newPos = _Owner.position + moveStep;

        newPos.y = CUnityUtil.GetMapHeight(newPos) + _ObjectComponent.OffsetY;
        if (IsValidPositionStrict(newPos) && Util.DistanceH(newPos, _Owner.position) > Util.FloatZero)
        {
            //Debug.LogWarningFormat("{0} {1} Adsorb", Time.frameCount, _Owner.position);
            //Debug.LogWarningFormat("{0} {1} Adsorb", Time.frameCount, newPos);
            _Owner.position = newPos;
        }

        return false;
    }
}
