using UnityEngine;

namespace CinemaDirector
{
    [CutsceneItemAttribute("uGUI", "Switch Canvas Camera", CutsceneItemGenre.ActorItem)]
    public class CanvasCameraSwitchEvent : CinemaActorEvent
    {
        public Camera Camera;
        Camera initialCamera;

        public override void Trigger(GameObject actor)
        {
            Canvas canvasComponent = actor.GetComponent<Canvas>();

            if (actor != null && Camera != null && canvasComponent != null)
            {
                if (canvasComponent.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    Debug.LogWarning("Current canvas render mode does not target a camera");
                    initialCamera = Camera.main;
#if ART_USE
                    initialCamera = Camera.main;
#else
                    initialCamera = Main.Main3DCamera;
#endif
                }
                else
                {
                    initialCamera = canvasComponent.worldCamera;
                    canvasComponent.worldCamera = Camera;
                }
            }
        }

        public override void Reverse(GameObject actor)
        {
            Canvas canvasComponent = actor.GetComponent<Canvas>();

            if (actor != null && Camera != null && canvasComponent != null)
            {
                if (canvasComponent.renderMode == RenderMode.ScreenSpaceOverlay)
                    Debug.LogWarning("Current canvas render mode does not target a camera");
                else
                    canvasComponent.worldCamera = initialCamera;
            }
        }

        public override void Stop(GameObject actor)
        {
            if (actor == null)
                return;
            Canvas canvasComponent = actor.GetComponent<Canvas>();
            if (canvasComponent != null)
                canvasComponent.worldCamera = initialCamera;
        }
    }
}