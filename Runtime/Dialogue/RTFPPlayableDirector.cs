namespace FuzzPhyte.Dialogue
{
    using UnityEngine;
    using UnityEngine.Playables;

    public class RTFPPlayableDirector : MonoBehaviour, IDialogueTimeline
    {
        public PlayableDirector TimelineDirector;
        [SerializeField] protected bool playedTimeline = false;
        public void PlayTimeline(bool singlePlay = true)
        {
            if (TimelineDirector == null) { return; }
            if (!playedTimeline)
            {
                TimelineDirector.Play();
                
            }
            if (singlePlay)
            {
                playedTimeline = true;
            }
        }

        public void SetupTimeline(RTTimelineDetails timelineDetails)
        {
            if(TimelineDirector!=null && timelineDetails != null)
            {
                TimelineDirector.playableAsset = timelineDetails.Timeline;
                TimelineDirector.time = timelineDetails.StartTime;
                TimelineDirector.playOnAwake = timelineDetails.PlayOnAwake;
                TimelineDirector.extrapolationMode = timelineDetails.WrapMode;
            }
        }

        public void StopTimeline()
        {
            if (TimelineDirector != null)
            {
                TimelineDirector.Stop();
            }
        }
        public void PauseTimeline()
        {
            if (TimelineDirector != null)
            {
                TimelineDirector.Pause();
            }
        }
        public void ResumeTimeline()
        {
            if (TimelineDirector != null)
            {
                TimelineDirector.Resume();
            }
        }
        public void ResetTimeline(float startTime = 0)
        {
            if (TimelineDirector != null)
            {
                TimelineDirector.Stop();
                TimelineDirector.time = startTime;
            }
        }
    }
}
