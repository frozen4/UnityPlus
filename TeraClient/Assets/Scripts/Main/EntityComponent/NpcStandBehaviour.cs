using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Common;
using GameLogic;

namespace EntityComponent
{
    public class NpcStandBehaviour : MonoBehaviour, IRecyclable, IEntityIdle
    {
        private const int MIN_STAND_LOOP = 6; //stand_c动画最小循环次数
        private const int MAX_STAND_LOOP = 15; //stand_c动画最大循环次数
        private const string FRONT_NAME = "Assets/Characters/";
        private const string TALK_GREET = "greet_c";
        private const float ANIMAITON_FADE_TIME = 0.1f; // 动画融合时间

        private Animation _AnimationComp = null;
        private AnimationUnit _AnimationUnitComp = null;
        private AnimationInfo _AnimationInfo = null;
        private string _StandAnimName = "";
        private float _StandAnimLength = 0f;

        private List<string> _IdleAnimList = null;
        private Coroutine _IdleCoroutine = null;

        public void Init(string standAni)
        {
            _StandAnimName = standAni;
            _AnimationComp = gameObject.GetComponent<Animation>();
            _AnimationUnitComp = gameObject.GetComponent<AnimationUnit>();
            _AnimationInfo = GetComponent<AnimationInfo>();
            if (_AnimationUnitComp != null && null != _AnimationInfo)
            {
                var aniState = GetAnimationStateByName(standAni);
                _StandAnimLength = aniState != null ? aniState.length : 0;

                if (null != _AnimationInfo.animationPaths)
                {
                    foreach (var v in _AnimationInfo.animationPaths)
                    {
                        if (!v.Contains("idle")) continue;

                        if (_IdleAnimList == null)
                            _IdleAnimList = new List<string>();
                        var temp = v.Split('/');
                        if (0 < temp.Length)
                        {
                            var animationName = System.IO.Path.GetFileNameWithoutExtension(temp[temp.Length - 1]);
                            //GetAnimationStateByName(animationName);
                            _IdleAnimList.Add(animationName);
                        }
                    }
                }
            }
        }

        public void StartIdle()
        {
            if (_AnimationUnitComp == null) return;

            StopIdle();

            if (!string.IsNullOrEmpty(_StandAnimName) && null != GetAnimationStateByName(_StandAnimName))
                _AnimationUnitComp.PlayAnimation(_StandAnimName, ANIMAITON_FADE_TIME, false, -1, false);

            // 规则：如果默认站立动作不是stand_c，则一定不会播休闲
            if (_StandAnimName != AnimationUnit.NORMAL_STAND)
                return;

            if (_IdleAnimList == null || _IdleAnimList.Count <= 0)
                return;

            if (gameObject.activeSelf)
                _IdleCoroutine = StartCoroutine(PlayIdleAnimation().GetEnumerator());
        }

        public void StopIdle()
        {
            if (_IdleCoroutine != null)
            {
                StopCoroutine(_IdleCoroutine);
                _IdleCoroutine = null;
            }
        }

        protected AnimationState GetAnimationStateByName(string animationName)
        {
            if (null == _AnimationUnitComp) return null;
            if (null != _AnimationComp[animationName]) return _AnimationComp[animationName];
            if (null == _AnimationInfo) return null;

            string path = GetFullAssetPath(animationName);
            if (string.IsNullOrEmpty(path))
            {
                //HobaDebuger.Log("cant find animationPath and transform name  is "+ this.transform.name +" animationName is "+ animationName);
                return null;
            }
            AnimationClip clip = CAssetBundleManager.SyncLoadAssetFromBundle<AnimationClip>(path, "animations", true);
            if (null == clip)
            {
                HobaDebuger.Log("cant Sync Load clip Resource and transform name  is " + this.transform.name + " animationName is " + animationName);
                return null;
            }

            _AnimationComp.AddClip(clip, animationName);
            return _AnimationComp[animationName];
        }
        private string GetFullAssetPath(string animationName)
        {
            var keyAnimationName = HobaText.Format("/{0}.anim", animationName);
            for (int i = 0; i < _AnimationInfo.animationPaths.Length; i++)
            {
                if (_AnimationInfo.animationPaths[i].EndsWith(keyAnimationName))
                {
                    return HobaText.Format("{0}{1}", FRONT_NAME, _AnimationInfo.animationPaths[i]);
                }
            }
            return string.Empty;
        }

        //开始交谈
        public void StartNpcTalk()
        {
            if (_AnimationUnitComp != null && null != GetAnimationStateByName(TALK_GREET))
            {
                // 规则：只有正在播放stand_c，才会过渡到Greet
                if (!_AnimationUnitComp.IsPlaying(AnimationUnit.NORMAL_STAND, AnimationUnit.NORMAL_STAND_CLONE))
                    return;

                StopIdle();
                if (null != GetAnimationStateByName(TALK_GREET))
                    _AnimationUnitComp.PlayAnimation(TALK_GREET, ANIMAITON_FADE_TIME, false, -1, false);
               if (null!= GetAnimationStateByName(_StandAnimName))
                    _AnimationUnitComp.PlayAnimation(_StandAnimName, ANIMAITON_FADE_TIME, true, -1, false);

                //交谈动作结束后重新开始休闲
               if (gameObject.activeInHierarchy)
                   StartCoroutine(WaitTalkEnd().GetEnumerator());
               
            }
        }

        private IEnumerable PlayIdleAnimation()
        {
            //没有休闲动作则跳过 (调用入口处已经做过检查)
            // if (_AnimationUnitComp != null && _IdleAnimList != null && _IdleAnimList.Count > 0)

            while (true)
            {
                var count = Random.Range(MIN_STAND_LOOP, MAX_STAND_LOOP + 1);
                yield return new WaitForSeconds(count * _StandAnimLength); //等待循环n次stand_c后播放idle_c

                //若此时没有播放stand_c，继续下次循环
                if (_AnimationUnitComp.IsPlaying(_StandAnimName, AnimationUnit.GetCloneAnimationName(_StandAnimName)))
                {
                    int animIndex = Random.Range(0, _IdleAnimList.Count);
                    string animName = _IdleAnimList[animIndex];
                    var idleAnimationState = GetAnimationStateByName(animName);
                    if (idleAnimationState != null)
                    {
                        _AnimationUnitComp.PlayAnimation(animName, ANIMAITON_FADE_TIME, false, -1, false);

                        // 不再使用Queue的方式进行，因为美术资源精诚将Idle作为Loop类型的  -- added by Jerry
                        yield return new WaitForSeconds(idleAnimationState.length - ANIMAITON_FADE_TIME);
                        if (_AnimationUnitComp.IsPlaying(animName))
                            _AnimationUnitComp.PlayAnimation(_StandAnimName, ANIMAITON_FADE_TIME, false, -1, false);
                    }
                }
            }
        }

        private IEnumerable WaitTalkEnd()
        {
            if (null != GetAnimationStateByName(TALK_GREET))
            {
                var length = _AnimationUnitComp.GetAniLength(TALK_GREET);
                yield return new WaitForSeconds(length);
                StartIdle();
            }
        }

        [NoToLua]
        public void OnRecycle()
        {
            StopIdle();
            if (_IdleAnimList != null)
                _IdleAnimList.Clear();
        }
    }
}
