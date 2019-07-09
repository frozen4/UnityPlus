using UnityEngine;

namespace EntityComponent
{
    public class CBulletTimeAnimation : MonoBehaviour
    {
        private AnimationUnit _AnimationUnit = null;
        private float _Speed;
        private AnimationState _CurAnimationState0 = null;
        private AnimationState _CurAnimationState1 = null;

        public void OnStart(float speed)
        {
            _AnimationUnit = gameObject.GetComponent<AnimationUnit>();
            if (_AnimationUnit == null)
            {
                Destroy(this);
                return;
            }

            _Speed = speed;

            _CurAnimationState0 = _AnimationUnit.GetCurAnimationStateAtLayer(0);
            _CurAnimationState1 = _AnimationUnit.GetCurAnimationStateAtLayer(1);

            if (_CurAnimationState0 != null)
                _CurAnimationState0.enabled = false;

            if (_CurAnimationState1 != null)
                _CurAnimationState1.enabled = false;
        }

        void Update()
        {
            if (_CurAnimationState0 != null && !_CurAnimationState0.enabled)
            {
                _CurAnimationState0.time += _Speed * Time.deltaTime;
                _CurAnimationState0.enabled = true;
                _AnimationUnit.Sample();
                _CurAnimationState0.enabled = false;
            }

            if (_CurAnimationState1 != null && !_CurAnimationState1.enabled)
            {
                _CurAnimationState1.time += _Speed * Time.deltaTime;
                _CurAnimationState1.enabled = true;
                _AnimationUnit.Sample();
                _CurAnimationState1.enabled = false;
            }
        }

        public void OnFinish()
        {
            if (_CurAnimationState0 != null)
                _CurAnimationState0.enabled = true;

            if (_CurAnimationState1 != null)
                _CurAnimationState1.enabled = true;

            Destroy(this);
        }
    }
}

