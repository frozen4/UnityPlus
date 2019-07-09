using UnityEngine;
using Common;
using System.Collections.Generic;
using System.IO;
using System;
using GameLogic;

namespace EntityComponent
{
    public class AnimationUnit : MonoBehaviour, IRecyclable
    {
        /* -----------------------------------------------
         *  功能描述：
         *      - 对游戏中常用动画接口的封装，考虑Animation与Animator的无缝切换
         *      - 减少函数托管与非托管之间的交互次数
         *  
         *  注意事项：
         *      (1) 拔剑收剑动画 UnloadWeaponState：
         *      - 放在layer 1层
         *      - 处于站立时，播全身；非站立状态下，播上半身
         *      (2) 受伤动画
         *      - 放在layer 1层
         *      - 当前只有 NORMAL_HURT，ADDITIVE_HURT已经废弃
         *      - 技能中不会播放受伤
         *      (3) 播放半身动画
         *      - 放在layer 1层
         *      - 仅在技能中使用，不会与收剑拔剑同时存在，也不会与Hurt同时存在
         *      - 需要主动播放 主动停止 （使用场景有限！！！无异常处理逻辑 此处可能会存在bug）
         *      (4) 播放其他动画
         *      - 放在layer 0层
         ------------------------------------------------ */
        #region 常量区
        [NoToLua]
        public const string UNLOAD_WEAPON = "battle_end_c";
        [NoToLua]
        public const string UNLOAD_WEAPON_CLONE = "battle_end_c_clone";

        [NoToLua]
        public const string NORMAL_STAND = "stand_c";
        [NoToLua]
        public const string NORMAL_STAND_CLONE = "stand_c_clone";

        [NoToLua]
        public const string BATTLE_STAND = "stand_battle_c";
        [NoToLua]
        public const string BATTLE_STAND_CLONE = "stand_battle_c_clone";

        [NoToLua]
        public const string BATTLE_RUN = "run_battle_c";
        [NoToLua]
        public const string BATTLE_RUN_CLONE = "run_battle_c_clone";

        [NoToLua]
        public const string NORMAL_RUN = "run_c";
        [NoToLua]
        public const string NORMAL_RUN_CLONE = "run_c_clone";

        private const string NORMAL_HURT = "hurt_front_c";
        private const string NORMAL_HURT_CLONE = "hurt_front_c_clone";
        //private const string ADDITIVE_HURT = "hurt_mix_c";
        private const string NORMAL_DIE = "die_c";
        private const string NORMAL_DIE_CLONE = "die_c_clone";

        private const string FRONT_NAME = "Assets/Characters/";
        #endregion

        #region 基础成员变量
        private Animator _AnimatorComponent = null;
        private Animation _AnimationComponent = null;
        private AnimationInfo _AnimationInfo = null;
        private Dictionary<string, int> _AnimationNameMap = null;
        private HangPointHolder _HangPointHolder = null;
        private Transform _WaistTrans = null;
        //现在Animation的Layer就用到了两层
        private string[] _CurPlayingAnimation = new string[2] {"", ""};
        private string _ToRecordAnimation = "";  // 记录Queued操作信息

        // 加速或减速播放动画时的动画修正速度
        [NoToLua]
        public float FixedSpeed = 1f;  
        #endregion

        #region 拔剑收剑相关
        // 收剑动画插值参数
        private const float UNLOAD_WEAPON_PARAM_TIME = 0.3f;
        private AnimationState _UnloadWeaponState = null;
        private bool _IsUWAnimationInLerp = false;
        #endregion

        #region 半身动画相关
        //当前正在进行的技能半身动画
        private string _CurPartialSkillAniName = "";
        #endregion

        #region 动画顿帧相关
        //private float _BluntEndTime = -1;
        private AnimationState _BluntAniState = null;
        private float _CorrectBluntSpeed = 0;
        #endregion


        void Awake()
        {
            _AnimatorComponent = GetComponent<Animator>();
            _AnimationComponent = GetComponent<Animation>();
            _AnimationInfo = GetComponent<AnimationInfo>();
            _HangPointHolder = GetComponent<HangPointHolder>();

            if (_AnimationComponent != null)
            {
                _UnloadWeaponState = GetAnimationStateByName(UNLOAD_WEAPON);
                if (null != _HangPointHolder && null != _HangPointHolder.HangPoint_WaistTrans)
                {
                    _WaistTrans = _HangPointHolder.HangPoint_WaistTrans.transform;
                }
                else
                {
                    _WaistTrans = transform.Find("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Spine2");
                    if (_WaistTrans == null)
                        _WaistTrans = transform.Find("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1");

                }

                if (_UnloadWeaponState != null && _WaistTrans != null)
                {
                    _UnloadWeaponState.wrapMode = WrapMode.Once;
                    _UnloadWeaponState.layer = 1;
                    _UnloadWeaponState.weight = 1f;
                    _UnloadWeaponState.enabled = false;
                    _UnloadWeaponState.AddMixingTransform(_WaistTrans);
                }
                _AnimationComponent.cullingType = AnimationCullingType.AlwaysAnimate;
            }
            else if (null != _AnimatorComponent)
            {
            }
        }

        public bool IsPlaying(string aniName, string cloneAniName = null)
        {
            // 正在播放拷贝动画也算正在播放
           
            // 优化
            //return ((_CurPlayingAnimation[0] == aniName || _CurPlayingAnimation[0] == clone_aniName || _CurPlayingAnimation[1] == aniName)
            //    && (_AnimationComponent.IsPlaying(aniName) || _AnimationComponent.IsPlaying(clone_aniName)));

            bool isAnim = (_CurPlayingAnimation[0] == aniName || _CurPlayingAnimation[1] == aniName);
            bool isPlayingAnim = _AnimationComponent.IsPlaying(aniName);

            if (isAnim && isPlayingAnim)
                return true;

            string clone_aniName = cloneAniName != null ? cloneAniName : GetCloneAnimationName(aniName);
            bool isAnimClone = _CurPlayingAnimation[0] == clone_aniName;
            bool isPlayingAnimClone = _AnimationComponent.IsPlaying(clone_aniName);

            if (isAnim && isPlayingAnimClone)
                return true;
            if (isAnimClone && (isPlayingAnim || isPlayingAnimClone))
                return true;
            return false;

//             if (_AnimationComponent != null)
//                 return _AnimationComponent.IsPlaying(aniName);
//             else if (_AnimatorComponent != null) { }
// 
//             return false;
        }

        public bool IsRealPlaying(string aniName)
        {
            return ((_CurPlayingAnimation[0] == aniName || _CurPlayingAnimation[1] == aniName)
                && (_AnimationComponent.IsPlaying(aniName)));
        }

        public bool HasAnimation(string aniName)
        {
            if (_AnimationComponent != null)
            {
                AnimationState state = GetAnimationStateByName(aniName);
                return state != null;
            }
            else if (_AnimatorComponent != null)
            {
            }

            return false;
        }

        public void PlayAnimation(string aniname, float fade_time, bool is_queued, float life_time, bool clampForever, float fixedSpeed = 1)
        {
            if (_AnimationComponent != null)
            {
                var curAniState = GetAnimationStateByName(aniname);
                if (curAniState == null) return;

                MixAnimationAtLayerOne(aniname);
                
                ResetAnimationState(aniname, life_time, clampForever, fixedSpeed);

                string real_aniname = aniname; //真正播放的动画名称
                //融合时间超出 动作时间不融
                bool can_cross = IsCurAniCanCross(0, fade_time, aniname);
                if (fade_time <= 0 || !can_cross)
                {
                    if (!is_queued)
                    {
                        // 此处为什么要Rewind，这是一个没搞明白的问题  - added by lijian
                        _AnimationComponent.Rewind(aniname);
                        _AnimationComponent.Play(aniname, PlayMode.StopSameLayer);
                    }
                    else
                    {
                        _AnimationComponent.PlayQueued(aniname);
                    }
                }
                else
                {
                    if (!is_queued)
                    {
                        if (IsPlaying(aniname))
                        {
                            // 同动画的融合，检查是否有拷贝动画可以播放
                            if (!IsRealPlaying(aniname))
                            {
                                //正在播放拷贝，融合时间作特殊处理
                                _AnimationComponent.CrossFade(aniname, 0.2f);
                            }
                            else
                            {
                                string clone_aniname = GetCloneAnimationName(aniname);
                                var cloneAniState = GetAnimationStateByName(clone_aniname);
                                if (cloneAniState != null)
                                {
                                    // 融合时间作特殊处理
                                    ResetAnimationState(clone_aniname, life_time, clampForever, fixedSpeed);
                                    real_aniname = clone_aniname;
                                    _AnimationComponent.CrossFade(clone_aniname, 0.2f);
                                }
                            }
                        }
                        else
                            _AnimationComponent.CrossFade(aniname, fade_time);
                    }
                    else
                    {
                        _AnimationComponent.CrossFadeQueued(aniname, fade_time);
                    }
                }
                
                // 记录layer 0正在播的动画
                if (!is_queued)
                {
                    if (IsInvoking("RecordLayer0Animation"))
                        CancelInvoke("RecordLayer0Animation");
                    _ToRecordAnimation = "";
                    _CurPlayingAnimation[0] = real_aniname;
                }
                else
                {
                    var curAni = _CurPlayingAnimation[0];
                    _ToRecordAnimation = real_aniname;
                    var cas = GetAnimationStateByName(curAni, false);
                    if (cas != null)
                    {
                        var length = cas.length - cas.time;
                        if (fade_time > 0) length -= fade_time;
                        Invoke("RecordLayer0Animation", length);
                    }
                    else
                    {
                        _ToRecordAnimation = "";
                        _CurPlayingAnimation[0] = real_aniname;
                        HobaDebuger.LogWarningFormat("CurPlayingAnimation {0} is null when QueuedAnimation", curAni);
                    }
                }
            }
            else if (_AnimatorComponent != null)
            {
            }
        }

        public void StopAnimation(string aniname, int layer)
        {
            if (layer != 0 && layer != 1) return;

            if (_AnimationComponent != null)
            {
                if (_AnimationComponent.IsPlaying(aniname))
                {
                    _AnimationComponent.Stop(aniname);
                    _CurPlayingAnimation[layer] = "";
                }
            }
            else if (_AnimatorComponent != null)
            {

            }
        }

        [NoToLua]
        public void Pause(string aniName, float pauseTime)
        {
            if (_AnimationComponent != null)
            {
                _AnimationComponent.enabled = true;
                var curState = GetAnimationStateByName(aniName);
                if (curState != null)
                {
                    _AnimationComponent.Play(aniName);
                    curState.time = pauseTime;
                    curState.speed = 0;
                }
                
            }
            else if (_AnimatorComponent != null)
            {
                // TODO:
            }
        }
        [NoToLua]

        public void PlayUnloadWeaponAni(bool backward)
        {
            if (_AnimationComponent != null)
            {
                var state = _UnloadWeaponState;
                if (_WaistTrans == null || state == null) return;
                state.weight = 1f;
                state.time = backward ? state.length : 0;
                state.speed = backward ? -1 : 1;

                var cur_ani = GetCurAnimNameAtLayer(0);
                if (cur_ani != NORMAL_STAND && cur_ani != BATTLE_STAND)
                    EnableMixingTransform(state, true);
                else
                    EnableMixingTransform(state, false);

                _AnimationComponent.Play(UNLOAD_WEAPON);

                // 记录layer 1正在播的动画
                _CurPlayingAnimation[1] = UNLOAD_WEAPON;

                if (!backward)
                    Invoke("ChangeUWAnimationLerpFlag", state.length - UNLOAD_WEAPON_PARAM_TIME);
            }
            else if (_AnimatorComponent != null)
            {

            }
        }

        public void PlayHurtAnimation(bool additive, string hurt_ani)
        {
            // 仅在站立和跑步状态下播放受伤
            if (_AnimationComponent != null)
            {
                var aniName = NORMAL_HURT;
                if(!string.IsNullOrEmpty(hurt_ani))
                {
                    aniName = hurt_ani;
                }

                var state = GetAnimationStateByName(aniName);
                if (state == null) return;

                state.layer = 1;
                if (additive)
                {
                    state.weight = 1f;
                    EnableMixingTransform(state, true);
                }
                else
                {
                    EnableMixingTransform(state, false);
                    state.time = 0;
                    // PlayQueued每次调用都会有不小的GC，此处优化实现方式，可能会有其他问题
                    //_AnimationComponent.PlayQueued(isCombat ? BATTLE_STAND : NORMAL_STAND);
                }
                _AnimationComponent.CrossFade(aniName, 0.1f);

                // 记录layer 1正在播的动画
                _CurPlayingAnimation[1] = NORMAL_HURT;
            }
            else if (_AnimatorComponent != null)
            {
            }
        }

        public void PlayDieAnimation(bool onlyLastFrame)
        {
            if (_AnimationComponent != null)
            {
                var dieAniState = GetAnimationStateByName(NORMAL_DIE);
                if (dieAniState == null) return;

                dieAniState.speed = 1;
                dieAniState.time = onlyLastFrame ? dieAniState.length : 0;

                dieAniState.wrapMode = WrapMode.ClampForever;
                _AnimationComponent.Play(NORMAL_DIE, PlayMode.StopAll);

                // 记录layer 0正在播的动画
                _CurPlayingAnimation[0] = NORMAL_DIE;
            }
        }

        //播放一个半身动画
        public void PlayPartialSkillAnimation(string aniname)
        {
            if (_AnimationComponent != null)
            {
                var aniState = GetAnimationStateByName(aniname);
                if (null == aniState) return;

                if (_CurPartialSkillAniName != "")
                {
                    _AnimationComponent.Stop(_CurPartialSkillAniName);
                    var curState = _AnimationComponent[_CurPartialSkillAniName];
                    curState.layer = 0;
                    curState.weight = 1f;
                    curState.enabled = false;
                    curState.RemoveMixingTransform(_WaistTrans);
                    _CurPartialSkillAniName = "";

                    // 记录layer 1正在播的动画
                    _CurPlayingAnimation[1] = "";
                }

                var newState = GetAnimationStateByName(aniname);
                if (newState != null && _WaistTrans != null)
                {
                    newState.layer = 1;
                    newState.weight = 1f;
                    newState.enabled = false;
                    newState.AddMixingTransform(_WaistTrans);
                    _CurPartialSkillAniName = aniname;

                    // 记录layer 1正在播的动画
                    _CurPlayingAnimation[1] = aniname;
                }
                else
                {
                    HobaDebuger.Log("error occur, aniname not exits or bip not exist! aniname = " + aniname);
                    return;
                }

                //开播
                _AnimationComponent.Play(aniname);
            }
        }

        public void StopPartialSkillAnimation(string aniname)
        {
            if (aniname != _CurPartialSkillAniName)
            {
                Debug.Log("error occur in StopPartialAnimation, stop error ani ");
                return;
            }

            _AnimationComponent.Stop(aniname);
            _CurPartialSkillAniName = "";
        }

        public float BluntCurSkillAnimation(float bluntTime, bool correctWhenEnd)
        {
            if (bluntTime <= 0) return 1;

            if (_AnimationComponent != null)
            {
                var layer = string.IsNullOrEmpty(_CurPartialSkillAniName) ? 0 : 1;
                var curAniName = GetCurAnimNameAtLayer(layer);
                if (string.IsNullOrEmpty(curAniName)) return 1;

                var anim_state = GetAnimationStateByName(curAniName);
                if (anim_state == null) return 1;
                if (anim_state.wrapMode != WrapMode.Loop && anim_state.length < anim_state.time + bluntTime)
                {
                    Common.HobaDebuger.LogWarning("the animation pause time is too long");
                    return 1;
                }

                anim_state.speed = 0;
                _AnimationComponent.Play(curAniName);

                _BluntAniState = anim_state;
                _CorrectBluntSpeed = 1f;
                if (correctWhenEnd && anim_state.wrapMode != WrapMode.Loop && anim_state.length > anim_state.time)
                    _CorrectBluntSpeed = (anim_state.length - anim_state.time + bluntTime) / (anim_state.length - anim_state.time);

                if (IsInvoking("CancelBluntState"))
                    CancelInvoke("CancelBluntState");
                Invoke("CancelBluntState", bluntTime);
            }
            else if (_AnimatorComponent != null)
            {
            }
            return _CorrectBluntSpeed;
        }

        public float GetAniLength(string aniName)
        {
            if (_AnimationComponent != null)
            {
                var state = GetAnimationStateByName(aniName);
                return state != null ? state.length : 0;
            }
            else if (_AnimatorComponent != null)
            {
            }

            return 0;
        }

        public string GetCurAnimNameAtLayer(int layer)
        {
            if (_AnimationComponent != null)
            {
                if (layer < _CurPlayingAnimation.Length 
                    && _AnimationComponent.IsPlaying(_CurPlayingAnimation[layer]))
                    return _CurPlayingAnimation[layer];
            }
            else if (_AnimatorComponent != null)
            {
            }

            return string.Empty;
        }

        [NoToLua]
        public AnimationState GetCurAnimationStateAtLayer(int layer)
        {
            var name = GetCurAnimNameAtLayer(layer);
            if (string.IsNullOrEmpty(name)) return null;

            if (_AnimationComponent != null)
            {
                return _AnimationComponent[name];
            }

            return null;
        }

        [NoToLua]
        public void Sample()
        {
            if (_AnimationComponent != null)
            {
                _AnimationComponent.Sample();
            }
        }
        //获取当前动作的进度
        public float GetCurAnimTimeAtLayer(int layer)
        {
            var curAniName = GetCurAnimNameAtLayer(layer);
            if (string.IsNullOrEmpty(curAniName))
                return 0;
            var anim_state = GetAnimationStateByName(curAniName);
            if (anim_state != null)
            {
                if (anim_state.wrapMode != WrapMode.Loop)
                {
                    return anim_state.time;
                }
                else
                {
                    float pass = anim_state.time - ((int)(anim_state.time / anim_state.length)) * anim_state.length;
                    return pass;
                }
            }
            return 0;
        }

        public void PlayAssignedAniClip(string aniname, float time)
        {
            if (aniname != "" && time >= 0)
            {
                if (_AnimationComponent != null)
                {
                    // 记录layer 0正在播的动画
                    _CurPlayingAnimation[0] = aniname;

                    var curAniState = GetAnimationStateByName(aniname);
                    if (curAniState != null)
                    {
                        curAniState.time = time;
                        _AnimationComponent.Play(aniname, PlayMode.StopSameLayer);
                    }
                    else
                    {
                        var normalStand = GetAnimationStateByName(NORMAL_STAND);
                        if (normalStand != null)
                            _AnimationComponent.Play(NORMAL_STAND, PlayMode.StopSameLayer);
                    }
                }
                else if (_AnimatorComponent != null)
                {
                }
            }
        }

        public void EnableAnimationComponent(bool act)
        {
            if (_AnimationComponent != null)
            {
                _AnimationComponent.enabled = act;
            }
            else if (_AnimatorComponent != null)
            {
                // _AnimatorComponent.enabled = act;
            }
        }

        public void CloneAnimationState(string aniname)
        {
            if (_AnimationComponent == null) return;

            string clone_aniname = GetCloneAnimationName(aniname);
            AnimationState state = GetAnimationStateByName(clone_aniname);
            if (state != null) return; // 已拷贝

            state = GetAnimationStateByName(aniname);
            if (state == null) return;

            _AnimationComponent.AddClip(state.clip, clone_aniname);
        }

        #region 私有函数
        private void ChangeUWAnimationLerpFlag()
        {
            _IsUWAnimationInLerp = true;
        }

        private void RecordLayer0Animation()
        {
            _CurPlayingAnimation[0] = _ToRecordAnimation;
            _ToRecordAnimation = "";
        }

        private void CancelBluntState()
        {
            if (_AnimationComponent != null)
            {
                if (_BluntAniState != null)
                    _BluntAniState.speed = _CorrectBluntSpeed;

                _BluntAniState = null;
                _CorrectBluntSpeed = 1f;
            }
        }

        private void EnableMixingTransform(AnimationState state, bool enable)
        {
            if (null == _WaistTrans) return;

            if (enable)
            {
                state.AddMixingTransform(_WaistTrans);
            }
            else
            {
                // 先Add再Remove，如果不先Add，调Remove会报错  -- by lijian 
                // 没找到查询Mixing状态的函数
                state.AddMixingTransform(_WaistTrans);
                state.RemoveMixingTransform(_WaistTrans);
            }
        }
            
        //判断当前动作是否可以开启融合
        //使用CrossFade混合时，如果混合时间大于动画自身长度，动画自身会被滞留在最后一帧，直到混合结束    
        private bool IsCurAniCanCross(int layer, float fadeTime, string newAni)
        {
            if (_AnimationComponent != null)
            {
                //之前动作
                var curAniName = GetCurAnimNameAtLayer(layer);
                var newState = GetAnimationStateByName(newAni);
                if (!string.IsNullOrEmpty(curAniName))
                {
                    var curState = _AnimationComponent[curAniName];
                    if (curState != null)
                    {
                        if (newState != null && curState.wrapMode == newState.wrapMode && newState.wrapMode == WrapMode.Loop)
                            return true;

                        if (curState != newState)
                        {
                            if (fadeTime >= curState.length)
                                return false;
                        }
                    }
                }

                //将要播放动作            
                if (null != newState && fadeTime > 0 && (newState.length < fadeTime))
                    return false;
            }

            return true;
        }

        //加入受击的动作融合, 受击已经播出后再播run动作会
        private void MixAnimationAtLayerOne(string aniname)
        {
            if (_AnimationComponent != null)
            {
                // 如果layer 1 上正在播全身受伤/全身收剑拔剑，移动时需要重新Mix
                if (IsPlaying(NORMAL_HURT, NORMAL_HURT_CLONE) && (aniname == BATTLE_RUN || aniname == NORMAL_RUN))
                {
                    var state = _AnimationComponent[NORMAL_HURT];
                    if (null != _WaistTrans)
                        state.AddMixingTransform(_WaistTrans);
                    // 以下操作是要干什么？应该直接AddMixingTransform就足够了
                    //var pre_time = state.time;
                    //_AnimationComponent.Stop(NORMAL_HURT);
                    //state.time = pre_time;
                    //_AnimationComponent.CrossFade(NORMAL_HURT, 0.2f);
                }

                if (IsPlaying(UNLOAD_WEAPON, UNLOAD_WEAPON_CLONE))
                {
                    if (aniname == BATTLE_RUN || aniname == NORMAL_RUN)
                    {
                        var unload_state = _AnimationComponent[UNLOAD_WEAPON];
                        if (null != _WaistTrans && null != unload_state)
                            unload_state.AddMixingTransform(_WaistTrans);
                    }
                    else
                    {
                        // 以下逻辑应该在CombatStateChangeBehaviour中已经处理，不该在这里
                        //if (aniname != NORMAL_STAND && aniname != BATTLE_STAND)
                        //    _AnimationComponent.Stop(UNLOAD_WEAPON);

                    }
                }
            }
        }

        void Update()
        {
            if (_IsUWAnimationInLerp)
            {   
                // 通过插值的方式实现，其他方式会有跳帧
                _UnloadWeaponState.weight -= Time.deltaTime / (UNLOAD_WEAPON_PARAM_TIME);
                if (_UnloadWeaponState.weight <= 0)
                    _IsUWAnimationInLerp = false;
            }
        }

        private string GetFullAssetPath(string animationName)
        {
            if (_AnimationNameMap == null)
            {
                _AnimationNameMap = new Dictionary<string, int>();
                BuildAnimationMap(_AnimationNameMap);
            }

            int index = 0;
            if (_AnimationNameMap.TryGetValue(animationName, out index))
            {
                return HobaText.Format("{0}{1}", FRONT_NAME, _AnimationInfo.animationPaths[index]);
            }

//             var keyName = HobaText.Format("/{0}.anim", animationName);
//             for (int i = 0; i < _AnimationInfo.animationPaths.Length; i++)
//             {
//                 if (_AnimationInfo.animationPaths[i].EndsWith(keyName))
//                 {
//                     //return FRONT_NAME + _AnimationInfo.animationPaths[i];
//                     return HobaText.Format("{0}{1}", FRONT_NAME, _AnimationInfo.animationPaths[i]);
//                 }
//             }
            return string.Empty;
        }

        private void BuildAnimationMap(Dictionary<string, int> animationNameMap)
        {
            if (_AnimationInfo == null)
                return;

            for (int i = 0; i < _AnimationInfo.animationPaths.Length; i++)
            {
                string animation = GetAnimationName(_AnimationInfo.animationPaths[i]);
                if (!animationNameMap.ContainsKey(animation))
                    animationNameMap.Add(animation, i);
            }
        }

        private string GetAnimationName(string animationPath)
        {
            return Path.GetFileNameWithoutExtension(animationPath);
        }

        protected AnimationState GetAnimationStateByName(string animationName, bool aysn = true)
        {
            if (null == _AnimationComponent) return null;
            var anim = _AnimationComponent[animationName];
            if (null != anim) return anim;
            if (!aysn || null == _AnimationInfo) return null;

            var path = GetFullAssetPath(animationName);
            if (string.IsNullOrEmpty(path)) return null;
 
            var clip = CAssetBundleManager.SyncLoadAssetFromBundle<AnimationClip>(path, "animations", true);
            if (null == clip) return null;

            _AnimationComponent.AddClip(clip, animationName);
            return _AnimationComponent[animationName];
        }

        private void ResetAnimationState(string aniname, float life_time, bool clampForever, float fixedSpeed)
        {
            var state = GetAnimationStateByName(aniname);
            if (state == null) return;

            //极端操作的情况会导致这个问题, 原因未查明可以问问unity官方
            if (state.wrapMode != WrapMode.Loop)
            {
                //if (curAniState.time > 0)
                {
                    state.time = 0;
                }
            }
            FixedSpeed = fixedSpeed;
            if (life_time > 0)
                FixedSpeed = state.length / life_time;
            state.speed = FixedSpeed;
            if (clampForever)
                state.wrapMode = WrapMode.ClampForever;
        }

        public static string GetCloneAnimationName(string aniname)
        {
            return HobaText.Format("{0}_clone", aniname);
        }
        #endregion

        [NoToLua]
        public void OnRecycle()
        {
            if (IsInvoking())
                CancelInvoke();

            for (int i = 0; i < _CurPlayingAnimation.Length; ++i)
                _CurPlayingAnimation[i] = "";
            _ToRecordAnimation = "";  // 记录Queued操作信息

            // 加速或减速播放动画时的动画修正速度
            FixedSpeed = 1f;
            _IsUWAnimationInLerp = false;

            //当前正在进行的技能半身动画
            _CurPartialSkillAniName = "";
            _CorrectBluntSpeed = 0;
        }

//        [NoToLua]
//         public void DebugCurrentAnimation()
//         {
//             if (null == _AnimationComponent) return;
//             var animation = _AnimationComponent.GetComponent<Animation>();
//             bool isPlaying = false;
//             foreach (AnimationState item in animation)
//             {
//                 if (_AnimationComponent.IsPlaying(item.name))
//                 {
//                     Debug.LogError("角色id"+ this.transform.parent.name + "当前正在播放的动画名称是"+item.name+ "动画层级为"+item.layer);
//                     isPlaying = true;
//                 }
//             }
//             if (!isPlaying)
//             {
//                 Debug.LogError("角色id" + this.transform.parent.name + "没有播放任何动画");
//             }
//         }
    }
}

