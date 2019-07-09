using UnityEngine;
using System.Collections;
using GameLogic;

namespace EntityComponent
{
    public class HorseStandBehaviour : MonoBehaviour, IRecyclable, IEntityIdle
    {
        private const float ANIMATION_FADE_TIME = 0.1f; // 动画融合时间
        private const string IDLE_ANIMATION_NAME = "idle_c";
        private const string BORN_ANIMATION_NAME = "born_c";

        private int _MinStandLoop = 2;
        private int _MaxStandLoop = 5;
        private string _HostStandAnimName = string.Empty;
        private string _HostStandAnimNameClone = string.Empty;
        private AnimationUnit _AnimationUnitComp = null;
        private AnimationUnit _HostAnimationUnitComp = null;
        
        private Coroutine _IdleCoroutine = null;
        private Coroutine _BornCoroutine = null;

        public void Init(int minInterval, int maxInterval, string standAnimName, GameObject host)
        {
            _MinStandLoop = minInterval;
            _MaxStandLoop = maxInterval;
            _HostStandAnimName = standAnimName;
            _HostStandAnimNameClone = AnimationUnit.GetCloneAnimationName(standAnimName);
            _AnimationUnitComp = gameObject.GetComponent<AnimationUnit>();
            if (host != null)
                _HostAnimationUnitComp = host.GetComponent<AnimationUnit>();
        }

        public void StartIdle()
        {
            StopIdle();

            if (gameObject.activeInHierarchy)
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

        public void StartBorn()
        {
            if (_AnimationUnitComp != null)
            {
                StopIdle();
                StopBorn();
                if (_AnimationUnitComp.HasAnimation(BORN_ANIMATION_NAME))
                    _AnimationUnitComp.PlayAnimation(BORN_ANIMATION_NAME, 0, false, -1, false);
                if (_AnimationUnitComp.HasAnimation(AnimationUnit.NORMAL_STAND))
                    _AnimationUnitComp.PlayAnimation(AnimationUnit.NORMAL_STAND, ANIMATION_FADE_TIME, true, -1, false);

                if (gameObject.activeInHierarchy)
                    _BornCoroutine = StartCoroutine(WaitBornEnd().GetEnumerator());
            }
        }

        private void StopBorn()
        {
            if (_BornCoroutine != null)
            {
                StopCoroutine(_BornCoroutine);
                _BornCoroutine = null;
            }
        }

        [NoToLua]
        public void OnRecycle()
        {
            StopIdle();
            StopBorn();
        }

        IEnumerable PlayIdleAnimation()
        {
            if (_AnimationUnitComp != null && _AnimationUnitComp.HasAnimation(AnimationUnit.NORMAL_STAND))
            {
                while (true)
                {
                    var loop_time = Random.Range(_MinStandLoop, _MaxStandLoop);
                    float stand_length = _AnimationUnitComp.GetAniLength(AnimationUnit.NORMAL_STAND);
                    yield return new WaitForSeconds(loop_time * stand_length);

                    if (_AnimationUnitComp.IsPlaying(AnimationUnit.NORMAL_STAND, AnimationUnit.NORMAL_STAND_CLONE))
                    {
                        if (_AnimationUnitComp.HasAnimation(IDLE_ANIMATION_NAME))
                            _AnimationUnitComp.PlayAnimation(IDLE_ANIMATION_NAME, ANIMATION_FADE_TIME, false, -1, false);

                        //主角的人物需要同步
                        if (_HostAnimationUnitComp != null && _HostAnimationUnitComp.IsPlaying(_HostStandAnimName, _HostStandAnimNameClone))
                            _HostAnimationUnitComp.PlayAnimation(_HostStandAnimName, 0f, false, -1, false);

                        float idle_length = _AnimationUnitComp.GetAniLength(IDLE_ANIMATION_NAME);
                        yield return new WaitForSeconds(idle_length);

                        _AnimationUnitComp.PlayAnimation(AnimationUnit.NORMAL_STAND, 0, false, -1, false);
                        //if (_HostAnimationUnitComp != null && _HostAnimationUnitComp.IsPlaying(_HostStandAnimName))
                        //    _HostAnimationUnitComp.PlayAnimation(_HostStandAnimName, 0f, false, -1, false);
                    }
                }
            }
        }

        IEnumerable WaitBornEnd()
        {
            if (_AnimationUnitComp != null && _AnimationUnitComp.HasAnimation(BORN_ANIMATION_NAME))
            {
                float born_length = _AnimationUnitComp.GetAniLength(BORN_ANIMATION_NAME);
                yield return new WaitForSeconds(born_length);

                //if (_AnimationUnitComp.HasAnimation(AnimationUnit.NORMAL_STAND))
                //    _AnimationUnitComp.PlayAnimation(AnimationUnit.NORMAL_STAND, 0, false, -1, false);

                //if (_HostAnimationUnitComp != null && _HostAnimationUnitComp.IsPlaying(_HostStandAnimName))
                //    _HostAnimationUnitComp.PlayAnimation(_HostStandAnimName, 0, false, -1, false);

                StartIdle();
                _BornCoroutine = null;
            }
        }
    }
}
