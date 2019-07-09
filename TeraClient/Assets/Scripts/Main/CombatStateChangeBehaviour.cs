using System;
using UnityEngine;
using GameLogic;

namespace EntityComponent
{
    public class CombatStateChangeBehaviour : MonoBehaviour, IRecyclable
    {
        static readonly Vector3 WeaponSmallSize = new Vector3(0.7f, 0.7f, 0.7f);
        private static string LeftHandKeywords = "_L_";
        private static string RightHandKeywords = "_R_";
        private static string BackKeywords = "_B_";

        private AnimationUnit _AnimationUnitComp = null;
        private bool _HasCombatAnimation = false;
        private HangPointHolder _HangPointHolderComp = null;
        private bool _Change2Combat = false;
        private bool _IsChangingNow = false;
        private float _AnimationLength = 0;
        private Transform _LeftHandWeaponTrans = null;
        private Transform _RightHandWeaponTrans = null;

        private bool _WeaponStateSwtichable = false;
        //private string _BackWeaponName = "";

        void Awake()
        {
            _AnimationUnitComp = gameObject.GetComponent<AnimationUnit>();
            _HasCombatAnimation = (_AnimationUnitComp != null && _AnimationUnitComp.HasAnimation(AnimationUnit.BATTLE_STAND));
            _HangPointHolderComp = gameObject.GetComponent<HangPointHolder>();
            _AnimationLength = _AnimationUnitComp != null ? _AnimationUnitComp.GetAniLength(AnimationUnit.UNLOAD_WEAPON) : 0f;
        }

        // 1 播放动作 2 切换挂点 3 顺切
        public void ChangeState(bool changeImmediatelly, bool is2Combat, float scaleChangeTime, float hangPointChangeTime)
        {
            if (null == _AnimationUnitComp || !_HasCombatAnimation) return;

            // 在现在的逻辑用，半身动画只用在技能中，故以下情形不存在
            //正在播放半身动作屏蔽下 收武器
            //if (_AnimationUnitComp.IsPlayingPartialAnimation && !is2Combat)
            //    return;
            // 切换中，重复调用直接返回
            if (_IsChangingNow && _Change2Combat == is2Combat && !changeImmediatelly) return;

            _AnimationUnitComp.StopAnimation(AnimationUnit.UNLOAD_WEAPON, 1);
            CancelInvokes();

            if (is2Combat)
            {
                if (_HasCombatAnimation)
                {
                    if (_AnimationUnitComp.IsPlaying(AnimationUnit.NORMAL_STAND, AnimationUnit.NORMAL_STAND_CLONE))
                        _AnimationUnitComp.PlayAnimation(AnimationUnit.BATTLE_STAND, 0.1f, false, 0, false);
                    else if (_AnimationUnitComp.IsPlaying(AnimationUnit.NORMAL_RUN, AnimationUnit.NORMAL_RUN_CLONE))
                        _AnimationUnitComp.PlayAnimation(AnimationUnit.BATTLE_RUN, 0.1f, false, 0, false, _AnimationUnitComp.FixedSpeed);
                }

                if (changeImmediatelly)
                {
                    GetWeapons();
                    _Change2Combat = is2Combat;
                    ChangeWeaponHangPoint();
                    ChangeWeaponSize();
                }
                else
                {
                    _Change2Combat = is2Combat;
                    _IsChangingNow = true;
                    _AnimationUnitComp.PlayUnloadWeaponAni(true);
                    StartInvokes(scaleChangeTime, hangPointChangeTime, _AnimationLength);
                }
            }
            else
            {
                if (_HasCombatAnimation)
                {
                    if (_AnimationUnitComp.IsPlaying(AnimationUnit.BATTLE_STAND, AnimationUnit.BATTLE_STAND_CLONE))
                        _AnimationUnitComp.PlayAnimation(AnimationUnit.NORMAL_STAND, 0.1f, false, 0, false);
                    else if (_AnimationUnitComp.IsPlaying(AnimationUnit.BATTLE_RUN, AnimationUnit.BATTLE_RUN_CLONE))
                        _AnimationUnitComp.PlayAnimation(AnimationUnit.NORMAL_RUN, 0.1f, false, 0, false, _AnimationUnitComp.FixedSpeed);
                }

                if (changeImmediatelly)
                {
                    _Change2Combat = is2Combat;
                    ChangeWeaponHangPoint();
                    ChangeWeaponSize();
                }
                else
                {
                    _Change2Combat = is2Combat;
                    _AnimationUnitComp.PlayUnloadWeaponAni(false);
                    StartInvokes(scaleChangeTime, hangPointChangeTime, _AnimationLength);
                }
            }
        }

        private void GetWeapons()
        {
            if(null == _HangPointHolderComp)
                _HangPointHolderComp = gameObject.GetComponent<HangPointHolder>();
            if (null == _HangPointHolderComp)
                return;
            var back1 = _HangPointHolderComp.GetHangPoint(HangPointHolder.HangPointType.WeaponBack1);
            var back2 = _HangPointHolderComp.GetHangPoint(HangPointHolder.HangPointType.WeaponBack2);
            var leftHand = _HangPointHolderComp.GetHangPoint(HangPointHolder.HangPointType.WeaponLeft);
            var rightHand = _HangPointHolderComp.GetHangPoint(HangPointHolder.HangPointType.WeaponRight);
            if (back1 != null && back1.transform.childCount > 0)
                _LeftHandWeaponTrans = back1.transform.GetChild(0);
            else if (leftHand != null && leftHand.transform.childCount > 0)
                _LeftHandWeaponTrans = leftHand.transform.GetChild(0);

            if (back2 != null && back2.transform.childCount > 0)
                _RightHandWeaponTrans = back2.transform.GetChild(0);
            else if (rightHand != null && rightHand.transform.childCount > 0)
                _RightHandWeaponTrans = rightHand.transform.GetChild(0);
        }

        private void StartInvokes(float time1, float time2, float time3)
        {
            Invoke("ChangeWeaponSize", time1);
            Invoke("ChangeWeaponHangPoint", time2);
            Invoke("ResetChangingFlag", time3);
        }

        private void CancelInvokes()
        {
            if (IsInvoking("ChangeWeaponSize"))
                CancelInvoke("ChangeWeaponSize");
            if (IsInvoking("ChangeWeaponHangPoint"))
                CancelInvoke("ChangeWeaponHangPoint");
            if (IsInvoking("ResetChangingFlag"))
                CancelInvoke("ResetChangingFlag");
        }

        private void ChangeWeaponHangPoint()
        {
            //Debug.LogWarningFormat("ChangeWeaponHangPoint {0}", Time.time);
            GetWeapons();
            if (_LeftHandWeaponTrans != null)
            {
                var hangPointId = !_Change2Combat ? HangPointHolder.HangPointType.WeaponBack1 : HangPointHolder.HangPointType.WeaponLeft;
                var dstHangPoint = _HangPointHolderComp.GetHangPoint(hangPointId);
                if (null != dstHangPoint)
                {
                    _LeftHandWeaponTrans.parent = dstHangPoint.transform;
                    _LeftHandWeaponTrans.localPosition = Vector3.zero;
                    _LeftHandWeaponTrans.localRotation = Quaternion.identity;
                }
            }

            if (_RightHandWeaponTrans != null)
            {
                var hangPointId = !_Change2Combat ? HangPointHolder.HangPointType.WeaponBack2 : HangPointHolder.HangPointType.WeaponRight;
                var dstHangPoint = _HangPointHolderComp.GetHangPoint(hangPointId);
                if (null != dstHangPoint)
                {
                    _RightHandWeaponTrans.parent = dstHangPoint.transform;
                    _RightHandWeaponTrans.localPosition = Vector3.zero;
                    _RightHandWeaponTrans.localRotation = Quaternion.identity;
                }
            }
            ChangeWeaponState();
            ChangeWeaponSize();
        }

        private void ChangeWeaponSize()
        {
            GetWeapons();
            if (_LeftHandWeaponTrans != null)
                _LeftHandWeaponTrans.localScale = !_Change2Combat ? WeaponSmallSize : Vector3.one;

            if (_RightHandWeaponTrans != null)
                _RightHandWeaponTrans.localScale = !_Change2Combat ? WeaponSmallSize : Vector3.one;
        }

        public void EnableWeaponStateSwtichable(bool enable)
        {
            _WeaponStateSwtichable = enable;
            ChangeWeaponState();
        }

        private void ChangeWeaponState()
        {
            if (!_WeaponStateSwtichable) return;

            GetWeapons();

            if (_LeftHandWeaponTrans != null)
            {
                var keywords = _Change2Combat ? LeftHandKeywords : BackKeywords;
                for (int i = 0; i < _LeftHandWeaponTrans.childCount; i++)
                {
                    var child = _LeftHandWeaponTrans.GetChild(i);
                    var active = child.gameObject.name.Contains(keywords);
                    if(active != child.gameObject.activeSelf)
                        child.gameObject.SetActive(active);
                }
            }

            if (_RightHandWeaponTrans != null)
            {
                var keywords = _Change2Combat ? RightHandKeywords : BackKeywords;
                for (int i = 0; i < _RightHandWeaponTrans.childCount; i++)
                {
                    var child = _RightHandWeaponTrans.GetChild(i);
                    var active = child.gameObject.name.Contains(keywords);
                    if (active != child.gameObject.activeSelf)
                        child.gameObject.SetActive(active);
                }
            }
        }

        private void ResetChangingFlag()
        {
            _IsChangingNow = false;
        }

        [NoToLua]
        public void OnRecycle()
        {
            _WeaponStateSwtichable = false;
            CancelInvokes();
        }
    }
}

