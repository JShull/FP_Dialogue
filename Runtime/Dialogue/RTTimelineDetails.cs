using FuzzPhyte.Utility;
using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace FuzzPhyte.Dialogue
{
    public enum TimelineType
    {
        NA=0,
        Input = 1,
        Output = 2
    }
    [Serializable]
    [CreateAssetMenu(fileName = "RTTimelineDetails", menuName = "FuzzPhyte/Dialogue/Graph/Create Timeline Details")]
    public class RTTimelineDetails : FP_Data
    {
        public string TimlineName = "DefaultTimeline";
        public TimelineType TType = TimelineType.Input;
        [Tooltip("Timline associated with node")]
        public TimelineAsset Timeline;
        [TextArea(2,4)]
        public string TimelineDescription;
    }
}
