using UnityEngine;
using System;
using System.Collections.Generic;
using GameLogic;

namespace EntityComponent
{
    public partial class ObjectBehaviour : MonoBehaviour, IRecyclable, ITickLogic
    {
        public static Action<ObjectBehaviour> OnRecycleCallback = null;

        public bool IgnoreTimer = false;
        public bool IgnoreCollisionDetecr = false;
        public bool IgnoreBehavior = false;

        public enum OBJ_TYPE
        {
            NONE = -1,         // 未知
            HOSTPLAYER = 0,    // 主角
            ELSEPLAYER,        // 其他玩家
            NPC,               // NPC
            MONSTER,           // 怪
            SUBOBJECT,         // 子物体
            LOOT,              // 掉落物
        }

        public OBJ_TYPE ObjType = OBJ_TYPE.NONE;
        public bool NeedSyncPos = false;
        public bool UsingGroundNormal = false;          //运用地面法向，在有坐骑时使用
        public float OffsetY = 0.0f;       //距离地面高度，某些entity需要不贴地
        public CPhysicsEventTransfer PhysicsHandler;

        private readonly LinkedList<Behavior> _ActiveBehaviorList = new LinkedList<Behavior>();
        private readonly LinkedList<Behavior> _InactiveBehaviorList = new LinkedList<Behavior>();
        private readonly List<Behavior> _ActiveTurnBehaviorList = new List<Behavior>();

        // 避免重复碰撞
        private readonly List<int> _Colliders = new List<int>();
        private int _LuaObjectRef = -2;

        private CTimerList _TimerList = null;

        private int _32BitID = 0;
        private float _Radius = 0;
        private Transform _SubobjectCollideTrans = null;

        public float Radius { get { return _Radius; } }

        public void Tick(float dt)
        {
            if (CGameSession.Instance().IsProcessingPaused)
                return;

            if (!IgnoreTimer)
            {
                if (_TimerList != null)
                    _TimerList.Tick(false);
            }

            if (!IgnoreCollisionDetecr)
                UpdateCollision();

            if (!IgnoreBehavior)
                UpdateBehaviors();
        }

        public int GetTimerCount()
        {
            if (_TimerList != null)
                return _TimerList.GetTimerCount();
            return 0;
        }

        public int ID32Bit
        {
            get { return _32BitID; }
        }

        public void SetUseGroundNormal(bool bNormal)
        {
            UsingGroundNormal = bNormal;

            if (!UsingGroundNormal && (ObjType == OBJ_TYPE.HOSTPLAYER || ObjType == OBJ_TYPE.ELSEPLAYER))
            {
                transform.rotation = CMapUtil.GetUpNormalRotation(transform.forward);
            }
        }

        public bool SetYOffset(float yOffset)
        {
            OffsetY = yOffset;

            return _ActiveBehaviorList.Count > 0;
        }

        public Behavior AddBehavior(BehaviorType type)
        {
            LinkedListNode<Behavior> curNode = _ActiveBehaviorList.First;
            while (curNode != null)
            {
                Behavior b = curNode.Value;
                if (b.Type == type)
                {
                    if (b.IsInCallback)
                        b.ReActive = true;

                    return b;
                }

                curNode = curNode.Next;
            }

            curNode = _InactiveBehaviorList.First;
            while (curNode != null)
            {
                Behavior b = curNode.Value;
                if (b.Type == type)
                {
                    _ActiveBehaviorList.AddLast(b);

                    if (b.Type == BehaviorType.Turn)
                        _ActiveTurnBehaviorList.Add(b);
                    _InactiveBehaviorList.Remove(b);
                    b.IsInCallback = false;
                    return b;
                }
                curNode = curNode.Next;
            }

            Behavior newb = BehaviorFactory.CreateBehavior(type);
            newb.Init(gameObject.transform, ObjType);
            _ActiveBehaviorList.AddLast(newb);

            if (newb.Type == BehaviorType.Turn)
                _ActiveTurnBehaviorList.Add(newb);

            return newb;
        }

        // 在Behavior List中，每种类型的Behavior，最多只有一个
        public void RemoveBehavior(BehaviorType type)
        {
            LinkedListNode<Behavior> cur_node = _ActiveBehaviorList.First;
            while (cur_node != null)
            {
                Behavior b = cur_node.Value;
                if (b.Type == type)
                {
                    if (!b.IsInCallback)
                    {
                        if (b.OnFinishCallbackRef != null)
                        {
                            b.OnFinishCallbackRef.Release();
                            b.OnFinishCallbackRef = null;
                        }
                        b.OnRemove();
                        _InactiveBehaviorList.AddLast(b);
                        _ActiveBehaviorList.Remove(b);

                        if (b.Type == BehaviorType.Turn)
                            _ActiveTurnBehaviorList.Remove(b);
                    }
                    break;
                }
                else
                {
                    cur_node = cur_node.Next;
                }
            }

        }

        public bool HasBehavior(BehaviorType type)
        {
            LinkedListNode<Behavior> cur_node = _ActiveBehaviorList.First;
            while (cur_node != null)
            {
                Behavior b = cur_node.Value;
                if (b.Type == type)
                    return true;

                cur_node = cur_node.Next;
            }

            return false;
        }

        public bool HasTurnBehavior()
        {
            return _ActiveTurnBehaviorList.Count > 0;
        }

        public Behavior GetActiveBehavior(BehaviorType behaviorType)
        {
            Behavior behavior = null;
            LinkedListNode<Behavior> cur_node = _ActiveBehaviorList.First;
            while (cur_node != null)
            {
                Behavior b = cur_node.Value;
                if (b.Type == behaviorType)
                {
                    behavior = b;
                    break;
                }
                cur_node = cur_node.Next;
            }
            return behavior;
        }

        //获取主角当前移动速度
        public float GetHostPlayerMoveSpeed()
        {
            var adsorb = GetActiveBehavior(BehaviorType.Follow) as AdsorbBehavior;
            if (adsorb == null) // 吸附可以与JoyStickMove & Move同时存在
            {
                var move = GetActiveBehavior(BehaviorType.Move) as MoveBehavior;
                if (move != null)
                    return move.MoveSpeed;

                var joystick = GetActiveBehavior(BehaviorType.JoyStickMove) as JoyStickBehavior;
                if (joystick != null)
                    return joystick.MoveSpeed;

                var follow = GetActiveBehavior(BehaviorType.Follow) as FollowBehavior;
                if (follow != null)
                    return follow.MoveSpeed;
            }
            else
            {
                var velocity = adsorb.Velocity;

                var move = GetActiveBehavior(BehaviorType.Move) as MoveBehavior;
                if (move != null)
                    velocity += move.Velocity;

                var joystick = GetActiveBehavior(BehaviorType.JoyStickMove) as JoyStickBehavior;
                if (joystick != null)
                    velocity += joystick.Velocity;

                return velocity.magnitude;
            }
            return 0f;
        }

        protected void UpdateBehaviors()
        {
            if (ObjType == OBJ_TYPE.SUBOBJECT) return;

            float dt = Time.deltaTime;
            LinkedListNode<Behavior> curNode = _ActiveBehaviorList.First;
            while (curNode != null)
            {
                Behavior b = curNode.Value;
                if (b.Tick(dt))
                {
                    if (!b.ReActive)
                    {
                        LinkedListNode<Behavior> tempNode = curNode.Next;
                        RemoveBehavior(b.Type);
                        curNode = tempNode;
                    }
                    else
                    {
                        b.ReActive = false;
                        curNode = curNode.Next;
                    }
                }
                else
                {
                    curNode = curNode.Next;
                }
            }

            if (UsingGroundNormal && (ObjType == OBJ_TYPE.HOSTPLAYER || ObjType == OBJ_TYPE.ELSEPLAYER))
            {
                var destRot = CMapUtil.GetMapNormalRotation(transform.position, transform.forward, 0.75f);

                // 非匀速Lerp无限接近目标值
                transform.rotation = Quaternion.Slerp(transform.rotation, destRot, Behavior.RotationLerpFactor);
            }
        }

        public void ClearAllBehavior()
        {
            LinkedListNode<Behavior> curNode = _ActiveBehaviorList.First;
            while (curNode != null)
            {
                Behavior b = curNode.Value;
                if (b.OnFinishCallbackRef != null)
                {
                    b.OnFinishCallbackRef.Release();
                    b.OnFinishCallbackRef = null;
                }
                curNode = curNode.Next;
            }

            curNode = _InactiveBehaviorList.First;
            while (curNode != null)
            {
                Behavior b = curNode.Value;
                if (b.OnFinishCallbackRef != null)
                {
                    b.OnFinishCallbackRef.Release();
                    b.OnFinishCallbackRef = null;
                }
                curNode = curNode.Next;
            }

            _ActiveBehaviorList.Clear();
            _InactiveBehaviorList.Clear();
            _ActiveTurnBehaviorList.Clear();
        }

        void UpdateCollision()
        {
            if (ObjType != OBJ_TYPE.SUBOBJECT || _SubobjectCollideTrans == null || _Radius < 0.001f) return;

            Vector3 center = _SubobjectCollideTrans.position;
            Collider[] colliders = Physics.OverlapSphere(center, _Radius, CUnityUtil.LayerMaskObstacle);
            if (colliders == null) return;

            int i = 0;
            while (i < colliders.Length)
            {
                GameObject go = colliders[i].gameObject;
                if (go == null) continue;

                if (go.layer == CUnityUtil.Layer_Player || go.layer == CUnityUtil.Layer_NPC || go.layer == CUnityUtil.Layer_HostPlayer)
                {
                    Transform trans = go.transform.parent;
                    if(null != trans)
                    {
                        ObjectBehaviour ob = trans.GetComponent<ObjectBehaviour>();
                        if (ob != null && !_Colliders.Contains(ob._32BitID))
                        {
                            OnCollideWithOther(ob._32BitID, CollideEntityType.All);
                            _Colliders.Add(ob._32BitID);
                        }
                    }

                }
                else if (go.layer == CUnityUtil.Layer_Blockable)
                {
                    if (!_Colliders.Contains(0))
                    {
                        OnCollideWithOther(0, CollideEntityType.All);
                        _Colliders.Add(0);
                    }
                }

                i++;
            }
        }

        void UnrefLuaObject()
        {
            if (_LuaObjectRef != -2)
            {
                LuaScriptMgr.Instance.SafeUnRef(ref _LuaObjectRef);
                _LuaObjectRef = -2;
            }
        }

        public void SetInfo(int obj_ref, int id, float r)
        {
            UnrefLuaObject();
            _LuaObjectRef = obj_ref;
            _32BitID = id;
            _Radius = r;

            if (ObjType == OBJ_TYPE.HOSTPLAYER)
                Main.HostPlayerRadius = _Radius;

            if (transform.childCount > 0)
                _SubobjectCollideTrans = transform.GetChild(0);
        }

        public int AddTimer(float ttl, bool bOnce, int cb, string debugInfo)
        {
            if (_TimerList == null)
                _TimerList = new CTimerList();

            return _TimerList.AddTimer(ttl, bOnce, cb, debugInfo);
        }

        public void RemoveTimer(int id)
        {
            if (_TimerList != null && id > 0)
                _TimerList.RemoveTimer(id);
        }

        public void ResetTimer(int id)
        {
            if (_TimerList != null && id > 0)
                _TimerList.ResetTimer(id);
        }

        private void Reset()
        {
            try
            {
                _Colliders.Clear();
                UnrefLuaObject();
                ClearAllBehavior();

                if (_TimerList != null)
                {
                    _TimerList.Clear();
                    _TimerList = null;
                }
                _SubobjectCollideTrans = null;

                if (ObjType == OBJ_TYPE.HOSTPLAYER)
                    Main.HostPlayerRadius = 0.1f;

                OffsetY = 0;
                UsingGroundNormal = false;

                PhysicsHandler = null;
                _32BitID = 0;
                _Radius = 0;
                ObjType = OBJ_TYPE.NONE;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void OnRecycle()
        {
            if (OnRecycleCallback != null)
                OnRecycleCallback(this);
            Reset();
        }

        void OnDestroy()
        {
            Reset();
        }
    }
}



