namespace FuzzPhyte.Dialogue
{
    using FuzzPhyte.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Timeline;
    /// <summary>
    /// Runtime Abstract Dialogue Node
    /// </summary>
    [Serializable]
    public abstract class RTFPNode
    {
        public string Index;
        public List<int> NextNodeIndices = new List<int>();
    }
    #region Runtime Nodes Associated with Dialogue System
    //Editor: EntryNode class
    [Serializable]
    public class RTEntryNode : RTFPNode
    {
        protected TimelineAsset incomingTimelineAsset;
        public TimelineAsset GetIncomingTimeline { get { return incomingTimelineAsset; } }

        public RTEntryNode(TimelineAsset inTimelineAsset=null)
        {
            if(incomingTimelineAsset == null && inTimelineAsset != null)
            {
                incomingTimelineAsset = inTimelineAsset;
            }
        }
    }
    //Editor: ExitNode class
    [Serializable]
    public class RTExitNode : RTFPNode
    {
        protected TimelineAsset outgoingTimelineAsset;
        public TimelineAsset GetOutgoingTimeline { get { return outgoingTimelineAsset; } }

        public RTExitNode(TimelineAsset outTimelineAsset = null)
        {
            if (outgoingTimelineAsset == null && outTimelineAsset != null)
            {
                outgoingTimelineAsset = outTimelineAsset;
            }
        }
    }
    //Editor: FPCombineNode class
    [Serializable]
    public class RTCombineNode : RTFPNode
    {

    }
    //Editor: SetFPCharacterNode class
    [Serializable]
    public class RTCharacterNode : RTFPNode
    {
        protected string name;
        protected FP_Gender gender;
        protected FP_Ethnicity ethnicity;
        protected FP_Language firstLang;
        protected FP_Language secondLang;
        protected FP_Language thirdLang;
        protected int age;
        protected SkinnedMeshRenderer characterSkinMeshRenderer;
        protected FP_Theme characterTheme;

        public RTCharacterNode(string name, FP_Gender gender, FP_Ethnicity ethnicity, FP_Language firstLang, FP_Language secondLang, FP_Language thirdLang, int age, SkinnedMeshRenderer characterSkinMeshRenderer, FP_Theme characterTheme)
        {
            this.name = name;
            this.gender = gender;
            this.ethnicity = ethnicity;
            this.firstLang = firstLang;
            this.secondLang = secondLang;
            this.thirdLang = thirdLang;
            this.age = age;
            this.characterSkinMeshRenderer = characterSkinMeshRenderer;
            this.characterTheme = characterTheme;
        }
    }
    //Editor: SetFPResponseNode class
    [Serializable]
    public class RTResponseNode : RTFPNode
    {

    }
    //Editor: SetFPDialogueNode class
    [Serializable]
    public class RTDialogueNode : RTFPNode
    {

    }
    //Editor: SetFPTalkNode class
    [Serializable]
    public class RTTalkNode : RTFPNode
    {

    }
    #endregion
}
