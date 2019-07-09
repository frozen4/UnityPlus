using UnityEngine;

namespace EZCameraShake
{
    public enum CameraShakeState { FadingIn, FadingOut, Sustained, Inactive }

    public class CameraShakeInstance : Hoba.ObjectPool.PooledObject
    {
        /// <summary>
        /// The intensity of the shake. It is recommended that you use ScaleMagnitude to alter the magnitude of a shake.
        /// </summary>
        public float Magnitude;

        /// <summary>
        /// Roughness of the shake. It is recommended that you use ScaleRoughness to alter the roughness of a shake.
        /// </summary>
        public float Roughness;

        /// <summary>
        /// How much influence this shake has over the local position axes of the camera.
        /// </summary>
        public Vector3 PositionInfluence;

        /// <summary>
        /// How much influence this shake has over the local rotation axes of the camera.
        /// </summary>
        public Vector3 RotationInfluence;

        /// <summary>
        /// Should this shake be removed from the CameraShakeInstance list when not active?
        /// </summary>
        public bool DeleteOnInactive = true;


        float roughMod = 1, magnMod = 1;
        float fadeOutDuration, fadeInDuration, keepDuration;
        bool sustain;
        float currentFadeTime;
        float tick = 0;
        Vector3 amt;

        private float TimeStartFadeIn = 0;
        private float TimeStartKeep = 0;
        private float TimeStartFadeOut = 0;
        //private float EndTime = 0;

        /// <summary>
        /// Will set the instance that will start a sustained shake.
        /// </summary>
        /// <param name="magnitude">The intensity of the shake.</param>
        /// <param name="roughness">Roughness of the shake. Lower values are smoother, higher values are more jarring.</param>
        public void Set(float magnitude, float roughness)
        {
            this.Magnitude = magnitude;
            this.Roughness = roughness;
            sustain = true;

            tick = Random.Range(-100, 100);
        }

        /// <summary>
        /// Will set the instance that will shake once and fade over the given number of seconds.
        /// </summary>
        /// <param name="magnitude">The intensity of the shake.</param>
        /// <param name="fadeOutTime">How long, in seconds, to fade out the shake.</param>
        /// <param name="roughness">Roughness of the shake. Lower values are smoother, higher values are more jarring.</param>

        public void Set(float magnitude, float roughness, float fadeInTime, float fadeOutTime, float keepTime = 0)
        {
            this.Magnitude = magnitude;
            fadeOutDuration = Mathf.Max(0.01f, fadeOutTime);

            fadeInTime = Mathf.Max(0.01f, fadeInTime);
            fadeInDuration = fadeInTime;

            keepDuration = keepTime;

            this.Roughness = roughness;
            if (fadeInTime > 0)
            {
                sustain = true;
                currentFadeTime = 0;
            }
            else
            {
                sustain = false;
                currentFadeTime = 1;
            }

            tick = Random.Range(-100, 100);
        }

        public void SetTimer()
        {
            TimeStartFadeIn = Time.realtimeSinceStartup;
            TimeStartKeep = TimeStartFadeIn + fadeInDuration;
            TimeStartFadeOut = TimeStartKeep + keepDuration;
            //EndTime = TimeStartFadeOut + fadeOutDuration;
        }

        public Vector3 UpdateShake()
        {
            amt.x = Mathf.PerlinNoise(tick, 0) - 0.5f;
            amt.y = Mathf.PerlinNoise(0, tick) - 0.5f;
            amt.z = Mathf.PerlinNoise(tick, tick) - 0.5f;

            if (fadeInDuration > 0 && sustain)
            {
                if (currentFadeTime < 1)
				{
                    //currentFadeTime += Time.deltaTime / fadeInDuration;
                    currentFadeTime = (Time.realtimeSinceStartup - TimeStartFadeIn) / fadeInDuration;
                 
				}
				else if(keepDuration > 0)
				{
					//currentFadeTime += 0;
					sustain = true;
				}
                else if (fadeOutDuration > 0)
				{
                    sustain = false;
				}
            }

            if (!sustain) {

                //currentFadeTime -= Time.deltaTime / fadeOutDuration;
                currentFadeTime = 1.0f - (Time.realtimeSinceStartup - TimeStartFadeOut) / fadeOutDuration;
			}

			//keep shaking duration...
			if (sustain && keepDuration > 0 && currentFadeTime > 0.99f) 
			{
                //keep tick...
                //Debug.Log("keepDuration:  " + keepDuration);
                //keepDuration -= Time.deltaTime;
                keepDuration = TimeStartFadeOut - Time.realtimeSinceStartup;
                tick += Time.deltaTime * Roughness * roughMod;             
            } 
			else 
			{
				if (sustain)
					tick += Time.deltaTime * Roughness * roughMod;
				else
					tick += Time.deltaTime * Roughness * roughMod * currentFadeTime;
			}

            return amt * Magnitude * magnMod * currentFadeTime;
        }

        /// <summary>
        /// Starts a fade out over the given number of seconds.
        /// </summary>
        /// <param name="fadeOutTime">The duration, in seconds, of the fade out.</param>
        public void StartFadeOut(float fadeOutTime)
        {
            if (fadeOutTime == 0)
                currentFadeTime = 0;

            fadeOutDuration = fadeOutTime;
            fadeInDuration = 0;
            sustain = false;
        }

        /// <summary>
        /// Starts a fade in over the given number of seconds.
        /// </summary>
        /// <param name="fadeInTime">The duration, in seconds, of the fade in.</param>
        public void StartFadeIn(float fadeInTime)
        {
            if (fadeInTime == 0)
                currentFadeTime = 1;

            fadeInDuration = fadeInTime;
            fadeOutDuration = 0;
            sustain = true;
        }

        /// <summary>
        /// Scales this shake's roughness while preserving the initial Roughness.
        /// </summary>
        public float ScaleRoughness
        {
            get { return roughMod; }
            set { roughMod = value; }
        }

        /// <summary>
        /// Scales this shake's magnitude while preserving the initial Magnitude.
        /// </summary>
        public float ScaleMagnitude
        {
            get { return magnMod; }
            set { magnMod = value; }
        }

        /// <summary>
        /// A normalized value (about 0 to about 1) that represents the current level of intensity.
        /// </summary>
        public float NormalizedFadeTime
        { get { return currentFadeTime; } }

        bool IsShaking
        { get { return currentFadeTime > 0 || sustain; } }

        bool IsFadingOut
        { get { return !sustain && currentFadeTime > 0; } }

        bool IsFadingIn
        { get { return currentFadeTime < 1 && sustain && fadeInDuration > 0; } }

        /// <summary>
        /// Gets the current state of the shake.
        /// </summary>
        public CameraShakeState CurrentState
        {
            get
            {
                if (IsFadingIn)
                    return CameraShakeState.FadingIn;
                else if (IsFadingOut)
                    return CameraShakeState.FadingOut;
                else if (IsShaking)
                    return CameraShakeState.Sustained;
                else
                    return CameraShakeState.Inactive;
            }
        }
    }
}