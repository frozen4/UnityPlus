#if SERVER_USE
using System;
using System.IO;
using System.Collections.Generic;
using Common;
using UnityEngine;
using LuaInterface;

using T = System.Int32;

//T: ID

// ReSharper disable once CheckNamespace
namespace Server.Game
{
    //public class NavCrowdMapInstance
    public class NavCrowdMapInstance
    {
        private static Common.LogHelp _log = LogHelp.getLogger(typeof(NavCrowdMapInstance));
        static int _NavCrowdId = 0;
        public readonly static  int NavCrowdMaxNum = 32;
        public readonly static float DefaultAgentRadius = 0.5f;

        NavMeshManager _NavMeshManager;
        public NavCrowdMapInstance(NavMeshManager navMeshManager)
        {
            _NavMeshManager = navMeshManager;
            Debug.Assert(_NavMeshManager.IsInited);
        }

        //
        public enum NavCrowdType
        {
            GENERATOR,
            CREATURE,
        }

        // NavCrowdId生成
        /// <summary>
        /// 保证Id的唯一性
        /// </summary>
        /// <param name="type"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public static int GenerateNavCrowd()
        {
            return ++_NavCrowdId;
        }

        //记得释放
        public void Release()
        {
            ClearAllCrowds();
        }

        private class NavCrowdEntry
        {
            public IntPtr navCrowd;
            public Dictionary<int, int> crowdObstacleID2IndexMap;

            public NavCrowdEntry()
            {
                navCrowd = IntPtr.Zero;
                crowdObstacleID2IndexMap = new Dictionary<int, int>();
            }
        }

        //crowd map
        private Dictionary<T, NavCrowdEntry> _NavCrowdMap = new Dictionary<T, NavCrowdEntry>();

        public void Tick(float dt)      //dt: ms
        {
            if (!_NavMeshManager.IsInited) return;

            Dictionary<T, NavCrowdEntry>.Enumerator itor = _NavCrowdMap.GetEnumerator();
            while (itor.MoveNext())
            {
                IntPtr crowd = itor.Current.Value.navCrowd;
                CrowdUpdateTick(crowd, (int)dt);
            }
        }

        float[] _Float3_Pos100 = new float[3];
        float[] _Float3_Target100 = new float[3];
 
        public bool UpdateOneAgent(T crowdId, int idx, int deltaTime, Vector3 pos, Vector3 targetpos, float maxSpeed, bool bIgnoreCollision, bool bKeepSpeed)      //deltaTime: ms
        {
            NavCrowdEntry entry = GetCrowdEntry(crowdId);
            if (entry == null)
            {
                logOne("1GetCrowdEntry is null, CrowdId = {0}", crowdId);
                return false;
            }

            //float[] fpos = new float[3];
            _Float3_Pos100[0] = pos.x;
            _Float3_Pos100[1] = pos.y;
            _Float3_Pos100[2] = pos.z;

            _Float3_Target100[0] = targetpos.x;
            _Float3_Target100[1] = targetpos.y;
            _Float3_Target100[2] = targetpos.z;

            return LuaDLL.NM_crowdUpdateOneAgent(entry.navCrowd, idx, deltaTime, _Float3_Pos100, _Float3_Target100, maxSpeed, bIgnoreCollision, bKeepSpeed);
        }

        private void CrowdUpdateTick(IntPtr pNavCrowd, int deltaTime)
        {
            if (_NavMeshManager.IsInited)
                LuaDLL.NM_crowdUpdateTick(pNavCrowd, deltaTime);
        }

        private NavCrowdEntry GetCrowdEntry(T crowdId)
        {
            NavCrowdEntry entry;
            if (_NavCrowdMap.TryGetValue(crowdId, out entry))
            {
                return entry;
            }
            else
            {
                return null;
            }
        }

        public bool CreateCrowd(T crowdId, int nMaxAgents, float fAgentRadius, float fHeightExt = 256.0f)           //fHeightExt 无视高度差的最大值
        {
            if (_NavCrowdMap.ContainsKey(crowdId))
            {
                logOne("CreateCrowd duplicate crowdId, CrowdId = {0}", crowdId);
                return false;
            }

            Common.SCounters.Instance.Increase( EnumCountType.CreateCrowd);
            IntPtr crowd = LuaDLL.NM_CreateCrowd(_NavMeshManager.NavQuery, nMaxAgents, fAgentRadius, fHeightExt);
            if (crowd == IntPtr.Zero)
            {
                _log.warnFormat("CreateCrowd LuaDLL.NM_CreateCrowd failed, CrowdId = {0}, nMaxAgents = {1}, fAgentRaius = {2}", crowdId, nMaxAgents, fAgentRadius);
                return false;
            }

            NavCrowdEntry entry = new NavCrowdEntry() { navCrowd = crowd };
            _NavCrowdMap.Add(crowdId, entry);

            return true;
        }

        public bool RemoveCrowd(T crowdId)
        {
            NavCrowdEntry entry = GetCrowdEntry(crowdId);
            if (entry == null)
            {
                logOne("2GetCrowdEntry is null, CrowdId = {0}", crowdId);
                return false;
            }
            Common.SCounters.Instance.Increase(EnumCountType.ClearCrowd);
            LuaDLL.NM_ClearCrowd(entry.navCrowd);
            _NavCrowdMap.Remove(crowdId);
            
            return true;
        }

        float[] _Float3_Pos200 = new float[3];
        public int CrowdAddAgent(T crowdId, Vector3 pos, ref SCrowdAgentParams param)
        {
            NavCrowdEntry entry = GetCrowdEntry(crowdId);
            if (entry == null)
            {
                logOne("3GetCrowdEntry is null, CrowdId = {0}", crowdId);
                return -1;
            }

            //float[] fpos = new float[3];
            _Float3_Pos200[0] = pos.x;
            _Float3_Pos200[1] = pos.y;
            _Float3_Pos200[2] = pos.z;
            Common.SCounters.Instance.Increase( EnumCountType.crowdAddAgent);
            return LuaDLL.NM_crowdAddAgent(entry.navCrowd, _Float3_Pos200, ref param);
        }

        public void CrowdRemoveAgent(T crowdId, int idx)
        {
            NavCrowdEntry entry = GetCrowdEntry(crowdId);
            if (entry == null)
            {
                logOne("4GetCrowdEntry is null, CrowdId = {0}", crowdId);
                return;
            }

            SCounters.Instance.Increase(EnumCountType.crowdRemoveAgent);
            LuaDLL.NM_crowdRemoveAgent(entry.navCrowd, idx);
        }

        public void ClearAllCrowds()
        {
            Dictionary<T, NavCrowdEntry>.Enumerator itor = _NavCrowdMap.GetEnumerator();
            while (itor.MoveNext())
            {
                T crowdId = itor.Current.Key;
                NavCrowdEntry entry = itor.Current.Value;
                Common.SCounters.Instance.Increase(EnumCountType.ClearCrowd);
                LuaDLL.NM_ClearCrowd(entry.navCrowd);
            }
            _NavCrowdMap.Clear();
        }

        public bool CrowdIsValid(T crowdId)
        {
            NavCrowdEntry entry = GetCrowdEntry(crowdId);
            if (entry == null)
            {
                logOne("9GetCrowdEntry is null, CrowdId = {0}", crowdId);
                return false;
            }

            return LuaDLL.NM_crowdIsValid(entry.navCrowd);
        }

        public int CrowdGetMaxAgentCount(T crowdId)
        {
            NavCrowdEntry entry = GetCrowdEntry(crowdId);
            if (entry == null)
            {
                logOne("10GetCrowdEntry is null, CrowdId = {0}", crowdId);
                return 0;
            }

            return LuaDLL.NM_crowdGetMaxAgentCount(entry.navCrowd);
        }

        public int CrowdGetActivityAgentCount(T crowdId)
        {
            NavCrowdEntry entry = GetCrowdEntry(crowdId);
            if (entry == null)
            {
                logOne("11GetCrowdEntry is null, CrowdId = {0}", crowdId);
                return 0;
            }

            return LuaDLL.NM_crowdGetActiveAgentCount(entry.navCrowd);
        }

        float[] _Float3_Pos300 = new float[3];
        float[] _Float3_Target300 = new float[3];
        float[] _Float3_Vel300 = new float[3];
        public bool CrowdGetAgentInfo(T crowdId, int idx, out Vector3 pos, out Vector3 targetPos, out Vector3 vel)
        {
            NavCrowdEntry entry = GetCrowdEntry(crowdId);
            if (entry == null)
            {
                logOne("11GetCrowdEntry is null, CrowdId = {0}", crowdId);
                pos = Vector3.zero;
                targetPos = Vector3.zero;
                vel = Vector3.zero;
                return false;
            }

            bool ret = LuaDLL.NM_crowdGetAgentInfo(entry.navCrowd, idx, _Float3_Pos300, _Float3_Target300, _Float3_Vel300);
            pos = new Vector3(_Float3_Pos300[0], _Float3_Pos300[1], _Float3_Pos300[2]);
            targetPos = new Vector3(_Float3_Target300[0], _Float3_Target300[1], _Float3_Target300[2]);
            vel = new Vector3(_Float3_Vel300[0], _Float3_Vel300[1], _Float3_Vel300[2]);
            return ret;
        }

        float[] _Float3_Pos400 = new float[3];
        public bool CrowdSetAgentPos(T crowdId, int idx, Vector3 pos)
        {
            NavCrowdEntry entry = GetCrowdEntry(crowdId);
            if (entry == null)
            {
                logOne("12GetCrowdEntry is null, CrowdId = {0}", crowdId);
                return false;
            }

            //float[] fpos = new float[3];
            _Float3_Pos400[0] = pos.x;
            _Float3_Pos400[1] = pos.y;
            _Float3_Pos400[2] = pos.z;
            return LuaDLL.NM_crowdSetAgentPos(entry.navCrowd, idx, _Float3_Pos400);
        }

        public bool CrowdGetAgentPos(T crowdId, int idx, out Vector3 pos)
        {
            NavCrowdEntry entry = GetCrowdEntry(crowdId);
            if (entry == null)
            {
                logOne("13GetCrowdEntry is null, CrowdId = {0}", crowdId);
                pos = Vector3.zero;
                return false;
            }

            Vector3 targetPos;
            Vector3 vel;
            bool ret = CrowdGetAgentInfo(entry.navCrowd, idx, out pos, out targetPos, out vel);
            return ret;
        }

        float[] _Float3_Pos500 = new float[3];
        float[] _Float3_Target500 = new float[3];
        float[] _Float3_Vel500 = new float[3];
        private bool CrowdGetAgentInfo(IntPtr _navCrowd, int idx, out Vector3 pos, out Vector3 targetPos, out Vector3 vel)
        {
            Debug.Assert(_navCrowd != IntPtr.Zero);

            bool ret = LuaDLL.NM_crowdGetAgentInfo(_navCrowd, idx, _Float3_Pos500, _Float3_Target500, _Float3_Vel500);
            pos = new Vector3(_Float3_Pos500[0], _Float3_Pos500[1], _Float3_Pos500[2]);
            targetPos = new Vector3(_Float3_Target500[0], _Float3_Target500[1], _Float3_Target500[2]);
            vel = new Vector3(_Float3_Vel500[0], _Float3_Vel500[1], _Float3_Vel500[2]);
            return ret;
        }

        public bool CrowdSetAgentSpeed(T crowdId, int idx, float speed)
        {
            NavCrowdEntry entry = GetCrowdEntry(crowdId);
            if (entry == null)
            {
                logOne("41GetCrowdEntry is null, CrowdId = {0}", crowdId);
                return false;
            }

            // 如果调用频繁，考虑将new放在外面
            SCrowdAgentParams param = new SCrowdAgentParams();
            if (!LuaDLL.NM_crowdGetAgentParam(entry.navCrowd, idx, out param))
                return false;
            param.maxSpeed = speed;
            return LuaDLL.NM_crowdUpdateAgentParam(entry.navCrowd, idx, ref param);
        }

        public bool CrowdGetAgentSpeed(T crowdId, int idx, out float speed)
        {
            NavCrowdEntry entry = GetCrowdEntry(crowdId);
            if (entry == null)
            {
                logOne("15GetCrowdEntry is null, CrowdId = {0}", crowdId);
                speed = 0.0f;
                return false;
            }

            // 如果调用频繁，考虑将new放在外面
            SCrowdAgentParams param = new SCrowdAgentParams();
            if (!LuaDLL.NM_crowdGetAgentParam(entry.navCrowd, idx, out param))
            {
                speed = 0.0f;
                return false;
            }
            speed = param.maxSpeed;
            return true;
        }

        float[] _Float3_Pos600 = new float[3];
        public bool CrowdSetMoveTarget(T crowdId, int idx, Vector3 pos)
        {
            NavCrowdEntry entry = GetCrowdEntry(crowdId);
            if (entry == null)
            {
                logOne("16GetCrowdEntry is null, CrowdId = {0}", crowdId);
                return false;
            }

            //float[] fpos = new float[3];
            _Float3_Pos600[0] = pos.x;
            _Float3_Pos600[1] = pos.y;
            _Float3_Pos600[2] = pos.z;
            return LuaDLL.NM_crowdSetMoveTarget(entry.navCrowd, idx, _Float3_Pos600);
        }

        public bool CrowdCancelMove(T crowdId, int idx)
        {
            NavCrowdEntry entry = GetCrowdEntry(crowdId);
            if (entry == null)
            {
                logOne("17GetCrowdEntry is null, CrowdId = {0}", crowdId);
                return false;
            }

            return LuaDLL.NM_crowdCancelMove(entry.navCrowd, idx);
        }

        float[] _Float3_Pos700 = new float[3];
        public void CrowdSetAllMoveTarget(T crowdId, Vector3 pos)
        {
            NavCrowdEntry entry = GetCrowdEntry(crowdId);
            if (entry == null)
            {
                logOne("18GetCrowdEntry is null, CrowdId = {0}", crowdId);
                return;
            }

            //float[] fpos = new float[3];
            _Float3_Pos700[0] = pos.x;
            _Float3_Pos700[1] = pos.y;
            _Float3_Pos700[2] = pos.z;
            LuaDLL.NM_crowdSetAllMoveTarget(entry.navCrowd, _Float3_Pos700);
        }

        public void CrowdCancelAllMove(T crowdId)
        {
            NavCrowdEntry entry = GetCrowdEntry(crowdId);
            if (entry == null)
            {
                logOne("19GetCrowdEntry is null, CrowdId = {0}", crowdId);
                return;
            }

            LuaDLL.NM_crowdCancelAllMove(entry.navCrowd);
        }
        private void logOne(string log, int param)
        {
            _log.warnFormat(log, param);
        }
    }
}
#endif
