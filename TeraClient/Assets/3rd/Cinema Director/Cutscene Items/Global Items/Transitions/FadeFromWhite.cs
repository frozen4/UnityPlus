// Cinema Suite
using UnityEngine;
using UnityEngine.UI;

namespace CinemaDirector
{
    /// <summary>
    /// Transition from White to Clear over time by overlaying a guiTexture.
    /// </summary>
    [CutsceneItem("Transitions", "Fade from White", CutsceneItemGenre.GlobalItem)]
    public class FadeFromWhite : CinemaGlobalAction
    {
        public Image image;
        private Color From = Color.white;
        private Color To = Color.clear;

        /// <summary>
        /// Enable the overlay texture and set the Color to White.
        /// </summary>
        public override void Trigger()
        {
            if (image != null)
            {
                image.enabled = true;
                image.color = From;
            }
        }

        /// <summary>
        /// Firetime is reached when playing in reverse, disable the effect.
        /// </summary>
        public override void ReverseTrigger()
        {
            End();
        }

        /// <summary>
        /// Update the effect over time, progressing the transition
        /// </summary>
        /// <param name="time">The time this action has been active</param>
        /// <param name="deltaTime">The time since the last update</param>
        public override void UpdateTime(float time, float deltaTime)
        {
            float transition = time / Duration;
            FadeToColor(From, To, transition);
        }

        /// <summary>
        /// Set the transition to an arbitrary time.
        /// </summary>
        /// <param name="time">The time of this action</param>
        /// <param name="deltaTime">the deltaTime since the last update call.</param>
        public override void SetTime(float time, float deltaTime)
        {
            if (image != null)
            {
                if (time >= 0 && time <= Duration)
                {
                    image.enabled = true;
                    UpdateTime(time, deltaTime);
                }
                else if (image.enabled)
                    image.enabled = false;
            }
        }

        /// <summary>
        /// End the effect by disabling the overlay texture.
        /// </summary>
        public override void End()
        {
            if (image != null)
                image.enabled = false;
        }

        /// <summary>
        /// The end of the action has been triggered while playing the Cutscene in reverse.
        /// </summary>
        public override void ReverseEnd()
        {
            if (image != null)
            {
                image.enabled = true;
                image.color = To;
            }
        }

        /// <summary>
        /// Disable the overlay texture
        /// </summary>
        public override void Stop()
        {
            if (image != null)
                image.enabled = false;
        }

        /// <summary>
        /// Fade from one colour to another over a transition period.
        /// </summary>
        /// <param name="from">The starting colour</param>
        /// <param name="to">The final colour</param>
        /// <param name="transition">the Lerp transition value</param>
        private void FadeToColor(Color from, Color to, float transition)
        {
            if (image != null)
                image.color = Color.Lerp(from, to, transition);
        }
    }
}