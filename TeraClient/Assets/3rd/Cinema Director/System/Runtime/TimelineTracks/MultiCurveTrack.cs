// Cinema Suite 2014

using UnityEngine;
namespace CinemaDirector
{
    [TimelineTrackAttribute("Curve Track", TimelineTrackGenre.MultiActorTrack, CutsceneItemGenre.MultiActorCurveClipItem)]
    public class MultiCurveTrack : TimelineTrack, IActorTrack
    {

        public override void Initialize()
        {
            TimelineItem[] timelineItems = this.GetTimelineItems();
            for (int i = 0; i < timelineItems.Length; i++)
            {
                CinemaMultiActorCurveClip clipCurve = timelineItems[i] as CinemaMultiActorCurveClip;
                clipCurve.Initialize();
            }
        }

        public override void UpdateTrack(float time, float deltaTime)
        {
            base.elapsedTime = time;
            TimelineItem[] timelineItems = this.GetTimelineItems();
            for (int i = 0; i < timelineItems.Length; i++)
            {
                CinemaMultiActorCurveClip clipCurve = timelineItems[i] as CinemaMultiActorCurveClip;
                clipCurve.SampleTime(time);
            }
        }

        public override void Stop()
        {
            TimelineItem[] timelineItems = this.GetTimelineItems();
            for (int i = 0; i < timelineItems.Length; i++)
            {
                CinemaMultiActorCurveClip clipCurve = timelineItems[i] as CinemaMultiActorCurveClip;
                clipCurve.Revert();
            }
        }

        public override TimelineItem[] GetTimelineItems()
        {
            return GetComponentsInChildren<CinemaMultiActorCurveClip>();
        }

        public Transform GetActor()
        {
            ActorTrackGroup component = base.transform.parent.GetComponent<ActorTrackGroup>();
            if (component == null)
            {
                return null;
            }
            return component.Actor; 
        }
    }
}