using UnityEngine;
using CinemaDirector.Helpers;
using System.Collections;

namespace CinemaDirector
{
    [CutsceneItemAttribute("Animation", "Play Animation", CutsceneItemGenre.ActorItem)]
    public class PlayAnimationEvent : CinemaActorAction
    {
        public AnimationClip animationClip = null;

        public WrapMode wrapMode;

        public float length;

        private AnimationState _state;

        void Awake()
        {
            // Loop & PingPong clips can be of any length, other wrap modes can not be longer than clip, but can be shorter
            if (wrapMode != WrapMode.Loop && wrapMode != WrapMode.PingPong && animationClip)
            {
                if (base.Duration > animationClip.length)
                    base.Duration = animationClip.length;
            }                
        }

        public override void Trigger(GameObject Actor)
        {
			if(!Actor.activeSelf)
				Actor.SetActive(true);

            Animation animation = Actor.GetComponent<Animation>();

            if (!animationClip || !animation)
            {
                Debug.LogWarning("animation = null when PlayAnimationEvent Trigger");
                return;
            }

            string clipName = animationClip.name;
            _state = animation[clipName];
            if (_state == null)
                animation.AddClip(animationClip, clipName);
            if (_state != null)
                _state.time = length;

            animation.wrapMode = wrapMode;
            animation.Play(clipName);
        }

        public override void UpdateTime(GameObject Actor, float runningTime, float deltaTime)
        {
#if IN_GAME
            // Note: 
            // ��IN_GAMEģʽ���Ľϴ�
            // IN_GAME ģʽ��֧��CG�����󣬳����ж��󶯻��������������
            // ���Animation��CinemaActorClipCurve����ͬһ���ڵ㣬���ܳ���Ч������
            // Animation�Խڵ��pos�����Ḳ�ǵ�ClipCurve����������������CGʱ�����
#else
            Animation animation = Actor.GetComponent<Animation>();

            if (!animation || animationClip == null)
            {
                return;
            }

            string clipName = animationClip.name;

            AnimationState state = animation[clipName];
            if (state == null)
            {
                animation.AddClip(animationClip, clipName);
                state = animation[clipName];
            }

            if (!animation.IsPlaying(clipName))
            {
                animation.wrapMode = wrapMode;
                animation.Play(clipName);
            }

            state.time = runningTime;
            state.enabled = true;
            animation.Sample();
            state.enabled = false;
#endif
        }

        public override void End(GameObject Actor)
        {
            //Animation animation = Actor.GetComponent<Animation>();
            //if (animation)
            //    animation.Stop();
        }

        public override void Stop()
        {
            if (_state)
            {
                if (_state.time >= _state.length)
                    return;
                if (_state.time != _state.length)
                    _state.time = _state.length;
            }
            else
            {
                ActorTrackGroup atg = GetComponentInParent<ActorTrackGroup>();
                if (!atg)
                    return;
                if (!atg.Actor)
                    return;
                Animation animation = atg.Actor.GetComponent<Animation>();
                if (!animation)
                    return;

                if (!animationClip)
                    return;

                string clipname = animationClip.name;
                AnimationState state = animation[clipname];
                if (_state == null)
                {
                    animation.AddClip(animationClip, clipname);
                    state = animation[clipname];
                }
                animation.wrapMode = wrapMode;
                if (state != null)
                    state.time = state.length;
                animation.Play(clipname);
            }
            base.Stop();
        }

    }
}