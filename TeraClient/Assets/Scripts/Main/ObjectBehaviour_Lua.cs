using UnityEngine;
using LuaInterface;
using System;
using Common;
using GameLogic;

namespace EntityComponent
{
    public partial class ObjectBehaviour : MonoBehaviour, IRecyclable
    {
        private static float SyncIntervalTime = 0.2f;
        private static float LastSyncTimestamp = 0f;

        public void OnClick()
        {
            IntPtr L = LuaScriptMgr.Instance.GetL();
            if (L != IntPtr.Zero)
            {
                //LuaDLL.lua_stackdump(L);
                var oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, _LuaObjectRef); // obj
                LuaDLL.lua_getfield(L, -1, "OnClick");                              // obj, OnClick
                LuaDLL.lua_pushvalue(L, -2);                                        // obj, OnClick, obj
                if (!LuaScriptMgr.Instance.GetLuaState().PCall(1, 0)) // obj
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
                LuaDLL.lua_settop(L, oldTop);
                //LuaDLL.lua_stackdump(L);
            }
        }

        public enum CollideEntityType
        {
            All = 0,
            OnlyTarget = 1,
            Enemy = 2,
        }
        public bool OnCollideWithOther(int colliderId, CollideEntityType collideEntityType)
        {
            bool result = false;
            IntPtr L = LuaScriptMgr.Instance.GetL();
            if (L != IntPtr.Zero)
            {
                var oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, _LuaObjectRef);   // obj
                LuaDLL.lua_getfield(L, -1, "OnCollideWithOther");   // obj, OnCollideWithOther 
                LuaDLL.lua_pushvalue(L, -2);                        // obj, OnCollideWithOther, obj 
                LuaDLL.lua_pushinteger(L, colliderId);             // obj, OnCollideWithOther, obj, collider_id             
                LuaDLL.lua_pushinteger(L, (int)collideEntityType);                         
                if (LuaScriptMgr.Instance.GetLuaState().PCall(3, 1)) // obj, ret
                    result = LuaDLL.lua_toboolean(L, -1);  // obj, ret
                else
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));

                LuaDLL.lua_settop(L, oldTop);
            }

            return result;
        }

        public bool OnCollidingHuge(int collider_id)
        {
            bool result = false;
            IntPtr L = LuaScriptMgr.Instance.GetL();
            if (L != IntPtr.Zero)
            {
                //LuaDLL.lua_stackdump(L);
                var oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, _LuaObjectRef);
                LuaDLL.lua_getfield(L, -1, "OnCollidingHuge");
                LuaDLL.lua_pushvalue(L, -2);
                LuaDLL.lua_pushinteger(L, collider_id);

                if (LuaScriptMgr.Instance.GetLuaState().PCall(2, 1)) // obj, ret
                {
                    result = LuaDLL.lua_toboolean(L, -1);  // obj, ret
                }
                else
                {
                    HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
                LuaDLL.lua_settop(L, oldTop);
            }
            return result;
        }

        public void OnPhysicsEventEnter(GameObject other, Vector3 pos)
        {
            CWeaponOwnerInfo woi = other.GetComponent<CWeaponOwnerInfo>();
            if (woi == null) return;

            IntPtr L = LuaScriptMgr.Instance.GetL();
            if (L != IntPtr.Zero)
            {
                var oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, _LuaObjectRef);   // 把 lua object 压栈
                LuaDLL.lua_getfield(L, -1, "OnPhysicsTriggerEvent");                  // 获得将调用的函数，压栈
                LuaDLL.lua_pushvalue(L, -2);                                          // 将lua object作为第1个参数压入栈
                LuaDLL.lua_pushinteger(L, woi.Owner32BitID);                          // 将武器所属角色的ID作为第2个参数压入栈
                LuaScriptMgr.Push(L, pos);                                            // 近似碰撞位置
                LuaScriptMgr.Instance.GetLuaState().Call(3); // obj, owner_id, pos
                LuaDLL.lua_pop(L, 1);
                LuaDLL.lua_settop(L, oldTop);
            }
        }

        public void SyncHostMoveInfo(Vector3 moveDir, bool isStop, bool forceSync, Vector3 destPos)
        {
            if(ObjType != ObjectBehaviour.OBJ_TYPE.HOSTPLAYER) return;
            if (!NeedSyncPos) return;

            var interTime = Time.time - LastSyncTimestamp;
            bool doNeed2RecordStamp = (interTime >= SyncIntervalTime || forceSync);
            if (doNeed2RecordStamp)
            {
                int mapId = ScenesManager.Instance.GetCurrentMapID();
                if (mapId != 0)             //mapId为0时处于切图状态，不发送move协议
                {
                    int commandTick = 1;
                    int entityId = _32BitID;
                    int intervalTime = (int)(interTime * 1000);
                    double timestamp = EntryPoint.GetServerTime();
                    Vector3 destPosition = destPos;

                    #region SendProtocol_RoleMove
                    IntPtr L = LuaScriptMgr.Instance.GetL();
                    if (L != IntPtr.Zero)
                    {
                        int oldTop = LuaDLL.lua_gettop(L);
                        LuaDLL.lua_getglobal(L, "SendProtocol_RoleMove");
                        if (!LuaDLL.lua_isnil(L, -1))
                        {
                            LuaScriptMgr.Push(L, commandTick);
                            LuaScriptMgr.Push(L, entityId);
                            LuaScriptMgr.Push(L, intervalTime);
                            LuaScriptMgr.Push(L, timestamp);
                            LuaScriptMgr.Push(L, mapId);
                            LuaScriptMgr.Push(L, isStop);
                            LuaScriptMgr.Push(L, true);

                            Vector3 position = transform.position;
                            Vector3 forward = transform.forward;

                            LuaScriptMgr.Push(L, position.x);
                            LuaScriptMgr.Push(L, position.z);

                            LuaScriptMgr.Push(L, forward.x);
                            LuaScriptMgr.Push(L, forward.z);

                            LuaScriptMgr.Push(L, moveDir.x);
                            LuaScriptMgr.Push(L, moveDir.z);

                            LuaScriptMgr.Push(L, destPosition.x);
                            LuaScriptMgr.Push(L, destPosition.z);

                            if (LuaDLL.lua_pcall(L, 15, 0, 0) != 0)
                            {
                                HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                            }
                        }
                        LuaDLL.lua_settop(L, oldTop);
                    }
                }
                #endregion // SendProtocol_RoleMove

                LastSyncTimestamp = Time.time;
            }
        }

        public void SyncHostDashCollideInfo(int colliderId, bool restartDash)
        {
            IntPtr L = LuaScriptMgr.Instance.GetL();
            if (L != IntPtr.Zero)
            {
                int oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_getglobal(L, "SendProtocol_EntityCollide");
                if (!LuaDLL.lua_isnil(L, -1))
                {
                    LuaScriptMgr.Push(L, colliderId);
                    int moveType = restartDash ? 1 : 0;
                    LuaScriptMgr.Push(L, moveType);

                    if (!LuaScriptMgr.Instance.GetLuaState().PCall(2, 0)) // obj, ret
                        HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                }
                LuaDLL.lua_settop(L, oldTop);
            }
        }

        public void SyncHostDashInfo(Vector3 curPos, Vector3 curDir, Vector3 destPos)
        {
            IntPtr L = LuaScriptMgr.Instance.GetL();
            if (L != IntPtr.Zero)
            {
                int oldTop = LuaDLL.lua_gettop(L);
                LuaDLL.lua_getglobal(L, "SendProtocol_SkillMoveTurn");
                if (!LuaDLL.lua_isnil(L, -1))
                {
                    LuaScriptMgr.Push(L, curPos.x);
                    LuaScriptMgr.Push(L, curPos.z);

                    LuaScriptMgr.Push(L, curDir.x);
                    LuaScriptMgr.Push(L, curDir.z);

                    LuaScriptMgr.Push(L, destPos.x);
                    LuaScriptMgr.Push(L, destPos.z);

                    if (LuaDLL.lua_pcall(L, 6, 0, 0) != 0)
                    {
                        HobaDebuger.LogLuaError(LuaDLL.lua_tostring(L, -1));
                    }
                }
                LuaDLL.lua_settop(L, oldTop);
            }
        }
    }
}



