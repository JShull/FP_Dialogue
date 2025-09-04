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
        public RTFPNode(string index)
        {
            Index = index;
        }
    }
    #region Runtime Nodes Associated with Dialogue System
    //Editor: EntryNode class
    [Serializable]
    public class RTEntryNode : RTFPNode
    {
        protected TimelineAsset incomingTimelineAsset;
        protected string outIndex;
        public TimelineAsset GetIncomingTimeline { get { return incomingTimelineAsset; } }

        public RTEntryNode(string index,string outIndexNode,TimelineAsset inTimelineAsset=null):base(index)
        {
            this.outIndex = outIndexNode;
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
        protected string inNode;
        public TimelineAsset GetOutgoingTimeline { get { return outgoingTimelineAsset; } }

        public RTExitNode(string index, string incomnigNodeExit,TimelineAsset outTimelineAsset = null):base(index)
        {
            inNode = incomnigNodeExit;
            if (outgoingTimelineAsset == null && outTimelineAsset != null)
            {
                outgoingTimelineAsset = outTimelineAsset;
            }
        }
    }

    [Serializable]
    public class RTOnewayNode :RTFPNode
    {
        protected string incomingIndex;
        protected string outgoingIndex;
        public RTOnewayNode(string index, string incomingNode, string outcomingNode) : base(index) 
        {
            this.incomingIndex = incomingNode;
            this.outgoingIndex = outcomingNode;
        }
    }
    //Editor: FPCombineNode class
    [Serializable]
    public class RTCombineNode : RTFPNode
    {
        protected string inNodeOneIndex;
        protected string inNodeTwoIndex;
        protected string outNodeOneIndex;

        public RTCombineNode(string index, string nodeOne, string nodeTwo, string outcomeNode):base(index)
        {
            this.inNodeOneIndex = nodeOne;
            this.inNodeTwoIndex = nodeTwo;
            this.outNodeOneIndex = outcomeNode;
        }
    }
    //Editor: SetFPCharacterNode class
    [Serializable]
    public class RTCharacterNode : RTFPNode
    {
        protected string characterName;
        protected string outNodeIndex;
        protected FP_Character characterData;
        protected FP_Gender gender;
        protected FP_Ethnicity ethnicity;
        protected FP_Language firstLang;
        protected FP_Language secondLang;
        protected FP_Language thirdLang;
        protected int age;
        protected SkinnedMeshRenderer characterSkinMeshRenderer;
        protected FP_Theme characterTheme;
        
        public RTCharacterNode(string index, string outNode,FP_Character dataFile, SkinnedMeshRenderer characterSkin, FP_Theme charTheme, bool replaceLocalData = true):base(index)
        {
            
            if (replaceLocalData)
            {
                this.characterName = dataFile.name;
                this.gender = dataFile.CharacterGender;
                this.ethnicity = dataFile.CharacterEthnicity;
                this.firstLang = dataFile.CharacterLanguages.Primary;
                this.secondLang = dataFile.CharacterLanguages.Secondary;
                this.thirdLang = dataFile.CharacterLanguages.Tertiary;
                this.age = dataFile.CharacterAge;
            }
            this.outNodeIndex = outNode;
            this.characterSkinMeshRenderer = characterSkin;
            this.characterTheme = charTheme;
            this.characterData = dataFile;
        }
        public RTCharacterNode(string index, string outNode,string name, FP_Gender gender, FP_Ethnicity ethnicity, FP_Language firstLang, FP_Language secondLang, FP_Language thirdLang, int age, SkinnedMeshRenderer characterSkinMeshRenderer, FP_Theme characterTheme):base(index)
        {
            this.outNodeIndex= outNode;
            this.characterName = name;
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
        //this is a node of other nodes
        protected List<RTSinglePromptNode> userIncomingPrompts = new List<RTSinglePromptNode>();
        protected List<string> userOutcomesConnectors = new List<string>();
        protected RTCharacterNode character;

        public RTResponseNode(string index, List<RTSinglePromptNode> incomingPromptNodes,List<string> outputNodes,RTCharacterNode theCharacter):base(index)
        {
            if (incomingPromptNodes.Count == outputNodes.Count)
            {
                userIncomingPrompts.Clear();
                userOutcomesConnectors.Clear();

                userIncomingPrompts.AddRange(incomingPromptNodes);
                userOutcomesConnectors.AddRange(outputNodes);
            }
            else
            {
                Debug.LogError($"Lists can't be different sizes");
            }
            character = theCharacter;
        }
    }
    //Editor: SetFPSinglePromptNode
    [Serializable]
    public class RTSinglePromptNode : RTFPNode
    {
        protected RTTalkNode mainDialogue;
        protected RTTalkNode translatedDialogue;
        protected Sprite promptIcon;
        protected GameObject spawnLocation;
        protected string outIndex;

        public RTSinglePromptNode(string index,string outcomingNodeIndex,RTTalkNode dialogue, RTTalkNode translation, Sprite promptIcon, GameObject spawnLocation):base(index)
        {
            this.outIndex = outcomingNodeIndex;
            this.mainDialogue = dialogue;
            this.translatedDialogue = translation;
            this.promptIcon = promptIcon;
            this.spawnLocation = spawnLocation;
        }
    }
    //Editor: SetFPDialogueNode class
    [Serializable]
    public class RTDialogueNode : RTFPNode
    {
        //this is a node of other nodes
        protected string incomingIndex;
        protected string outgoingIndex;
        protected RTTalkNode mainDialogue;
        protected RTTalkNode translatedDialogue;
        protected RTCharacterNode incomingCharacter;
        protected RTCharacterNode outgoingCharacter;
        public RTDialogueNode(string index,string incominIndex, string outIndex, RTTalkNode dialogue,RTCharacterNode incomingCharacterNode,RTCharacterNode outgoingCharacaterNode = null,RTTalkNode transDialogue=null) : base(index)
        {
            this.incomingIndex = incominIndex;
            this.outgoingIndex = outIndex;
            this.mainDialogue = dialogue;
            this.translatedDialogue = transDialogue;
            this.incomingCharacter = incomingCharacterNode;
            this.outgoingCharacter = outgoingCharacaterNode;
        }
    }
    //Editor: SetFPTalkNode class
    [Serializable]
    public class RTTalkNode : RTFPNode
    {
        protected FP_Language language;
        protected string headerText;
        protected string dialogueText;
        protected AudioClip textAudio;
        protected AnimationClip faceAnimation;
        protected bool hasAudio;
        protected bool hasAnimation;
        protected string outIndex;
        public bool HasAudio { get { return hasAudio; } }
        public bool HasAnimation { get { return hasAnimation; } }

        public RTTalkNode(string index, string outIndex, FP_Language theLanguage, string headText, string convoText, AudioClip textAudio = null, AnimationClip faceAnimation = null):base(index)
        {
            this.outIndex = outIndex;
            this.language = theLanguage;
            this.headerText = headText;
            this.dialogueText = convoText;
            if (textAudio == null)
            {
                hasAudio = false;
            }
            else
            {
                this.textAudio = textAudio;
                hasAudio = true;
            }
            if (faceAnimation==null)
            {
                hasAnimation = false;
            }
            else
            {
                hasAnimation= true;
                this.faceAnimation = faceAnimation;
            }
        }
    }
    #endregion
}
