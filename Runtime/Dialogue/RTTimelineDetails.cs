namespace FuzzPhyte.Dialogue
{
    using FuzzPhyte.Utility;
    using System;
    using UnityEngine;
    using UnityEngine.Timeline;
    using UnityEngine.Playables;
    [Serializable]
    public enum TimelineType
    {
        NA=0,
        Input = 1,
        Output = 2
    }
    [Serializable]
    public enum TimelineAction
    {
        NA = 0,
        Play = 1,
        Pause = 2,
        Resume = 3,
        Stop = 4,
        Reset = 5,
        Setup = 6,
        SetupAndPlay = 7,
    }
    [Serializable]
    [CreateAssetMenu(fileName = "RTTimelineDetails", menuName = "FuzzPhyte/Dialogue/Graph/Create Timeline Details")]
    public class RTTimelineDetails : FP_Data
    {
        [Tooltip("If we have to go find this director")]
        public string BinderDirectorLookUpName = "GameObjectName";
        public string TimelineName = "DefaultTimeline";
        public bool PlayOnAwake = false;
        public double StartTime = 0;
        public DirectorWrapMode WrapMode = DirectorWrapMode.None;
        public TimelineType TType = TimelineType.Input;
        public TimelineAction TAction = TimelineAction.NA;
        [Tooltip("Timline associated with node")]
        public TimelineAsset Timeline;
        public bool PlayTimelineOnce = true;
        [TextArea(2,4)]
        public string TimelineDescription;
    }
}
