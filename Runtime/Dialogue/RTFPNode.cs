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
        protected RTTimelineDetails incomingTimelineAsset;
        protected TimelineAsset timelineAsset;
        public RTTimelineDetails GetTimelineAssetFile { get { return incomingTimelineAsset; } }
        protected string[] outIndex;
        public string[] NextNodes { get { return outIndex; } }
        public TimelineAsset GetIncomingTimeline
        {
            get
            {
                return timelineAsset;
            }
        }

        public RTEntryNode(string index,List<string> outIndexNode, RTTimelineDetails inTimelineAsset =null):base(index)
        {
            this.outIndex = outIndexNode.ToArray();
            if(incomingTimelineAsset == null && inTimelineAsset != null)
            {
                incomingTimelineAsset = inTimelineAsset;
                timelineAsset = incomingTimelineAsset.Timeline;
            }
        }
        public RTEntryNode(string index, List<string> outIndexNode, TimelineAsset inTimelineAsset = null) : base(index) 
        {
            this.outIndex = outIndexNode.ToArray();
            timelineAsset = inTimelineAsset;
        }

    }
    //Editor: ExitNode class
    [Serializable]
    public class RTExitNode : RTFPNode
    {
        protected RTTimelineDetails outgoingTimelineDetails;
        protected TimelineAsset timelineAsset;
        protected string[] inNode;
        public string[] InNodes { get { return inNode; } }
        public TimelineAsset GetOutgoingTimeline { get { return timelineAsset; } }
        public RTTimelineDetails GetTimelineAssetFile { get { return outgoingTimelineDetails; } }
       
        public RTExitNode(string index, List<string> incomingNodeExit,RTTimelineDetails outTimelineAsset = null):base(index)
        {
            inNode = incomingNodeExit.ToArray();
            if (timelineAsset == null && outTimelineAsset != null)
            {
                outgoingTimelineDetails = outTimelineAsset;
                timelineAsset = outTimelineAsset.Timeline;
            }
        }
        public RTExitNode(string index, List<string> incomingNodeExit, TimelineAsset outTimelineAsset = null) : base(index)
        {
            inNode = incomingNodeExit.ToArray();
            timelineAsset = outTimelineAsset;
            
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
        protected string[] outNodeOneIndex;

        public RTCombineNode(string index, string nodeOne, string nodeTwo, List<string> outcomeNode):base(index)
        {
            this.inNodeOneIndex = nodeOne;
            this.inNodeTwoIndex = nodeTwo;
            this.outNodeOneIndex = outcomeNode.ToArray();
        }
    }
    //Editor: SetFPCharacterNode class
    [Serializable]
    public class RTCharacterNode : RTFPNode
    {
        protected string characterName;
        protected string outNodeIndex;
        protected FP_Character characterData;
        public FP_Character ReturnCharacterData { get { return characterData; } }
        protected FP_Gender gender;
        protected FP_Ethnicity ethnicity;
        protected FP_Language firstLang;
        protected FP_Language secondLang;
        protected FP_Language thirdLang;
        protected int age;
        protected GameObject characterSkinMeshRenderer;
        public GameObject GetCharacterHeadObject { get { return characterSkinMeshRenderer; } }
        protected FP_Theme characterTheme;
        public FP_Theme GetCharacterTheme { get { return characterTheme; } }
        
        public RTCharacterNode(string index, string outNode,FP_Character dataFile, GameObject characterSkin, FP_Theme charTheme, bool replaceLocalData = true):base(index)
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
        public RTCharacterNode(string index, string outNode,string name, FP_Gender gender, FP_Ethnicity ethnicity, FP_Language firstLang, FP_Language secondLang, FP_Language thirdLang, int age, GameObject characterSkinMeshRenderer, FP_Theme characterTheme):base(index)
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
        public List<RTSinglePromptNode> UserPrompts
        {
            get { return userIncomingPrompts; }
        }
        protected List<string> userOutcomesConnectors = new List<string>();
        protected RTCharacterNode character;
        public RTCharacterNode ResponseCharacter
        {
            get { return character; }
        }

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
        public RTTalkNode GetMainDialogue
        {
            get { return mainDialogue; }
        }
        protected RTTalkNode translatedDialogue;
        public RTTalkNode GetTranslatedDialogue
        {
            get { return translatedDialogue; }
        }
        protected Sprite promptIcon;
        protected GameObject spawnLocation;
        public GameObject SpawnLocation
        {
            get { return spawnLocation; }
        }
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
        public RTTalkNode GetMainDialogue { get { return mainDialogue; } }
        public RTTalkNode GetTranslationDialogue { get { return translatedDialogue; } }
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
        public FP_Language Language { get {  return language; } }
        protected string headerText;
        public string HeaderText { get{ return headerText; } }
        protected string dialogueText;
        public string DialogueText { get{ return dialogueText; } }
        protected AudioClip textAudio;
        public AudioClip AudioClip { get { return textAudio; } }
        protected AnimationClip faceAnimation;
        public AnimationClip FaceAnimation { get { return faceAnimation; } }
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
