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
        public string NodeType;
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
        public RTTimelineDetails incomingTimelineAsset;
        public TimelineAsset timelineAsset;
        //public RTTimelineDetails GetTimelineAssetFile { get { return incomingTimelineAsset; } }
        public string[] outIndex;
        public string[] NextNodes { get { return outIndex; } }
        

        public RTEntryNode(string index,List<string> outIndexNode, RTTimelineDetails inTimelineAsset =null):base(index)
        {
            this.outIndex = outIndexNode.ToArray();
            NodeType = "RTEntryNode";
            if(incomingTimelineAsset == null && inTimelineAsset != null)
            {
                incomingTimelineAsset = inTimelineAsset;
                timelineAsset = incomingTimelineAsset.Timeline;
            }
        }
        public RTEntryNode(string index, List<string> outIndexNode, TimelineAsset inTimelineAsset = null) : base(index) 
        {
            NodeType = "RTEntryNode";
            this.outIndex = outIndexNode.ToArray();
            timelineAsset = inTimelineAsset;
        }

    }
    //Editor: ExitNode class
    [Serializable]
    public class RTExitNode : RTFPNode
    {
        public RTTimelineDetails outgoingTimelineDetails;
        public TimelineAsset timelineAsset;
        public string[] inNode;
       
        public RTExitNode(string index, List<string> incomingNodeExit,RTTimelineDetails outTimelineAsset = null):base(index)
        {
            inNode = incomingNodeExit.ToArray();
            NodeType = "RTExitNode";
            if (timelineAsset == null && outTimelineAsset != null)
            {
                outgoingTimelineDetails = outTimelineAsset;
                timelineAsset = outTimelineAsset.Timeline;
            }
        }
        public RTExitNode(string index, List<string> incomingNodeExit, TimelineAsset outTimelineAsset = null) : base(index)
        {
            inNode = incomingNodeExit.ToArray();
            NodeType = "RTExitNode";
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
            NodeType = "RTOnewayNode";
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
            NodeType = "RTCombineNode";
            this.inNodeOneIndex = nodeOne;
            this.inNodeTwoIndex = nodeTwo;
            this.outNodeOneIndex = outcomeNode.ToArray();
        }
    }
    //Editor: SetFPCharacterNode class
    [Serializable]
    public class RTCharacterNode : RTFPNode
    {

        public string characterName;
        public string outNodeIndex;
        public FP_Character characterData;
        public FP_Gender gender;
        public FP_Ethnicity ethnicity;
        public FP_Language firstLang;
        public FP_Language secondLang;
        public FP_Language thirdLang;
        public int age;
        public string characterSkinMeshRendererID;
        public FP_Theme characterTheme;
      
        
        public RTCharacterNode(string index, string outNode,FP_Character dataFile, string characterSkin, FP_Theme charTheme, bool replaceLocalData = true):base(index)
        {
            NodeType = "RTCharacterNode";
            if (replaceLocalData)
            {
                this.characterName = dataFile.CharacterName;
                this.gender = dataFile.CharacterGender;
                this.ethnicity = dataFile.CharacterEthnicity;
                this.firstLang = dataFile.CharacterLanguages.Primary;
                this.secondLang = dataFile.CharacterLanguages.Secondary;
                this.thirdLang = dataFile.CharacterLanguages.Tertiary;
                this.age = dataFile.CharacterAge;
            }
            this.outNodeIndex = outNode;
            this.characterSkinMeshRendererID = characterSkin;
            this.characterTheme = charTheme;
            this.characterData = dataFile;
        }
        public RTCharacterNode(string index, string outNode,string name, FP_Gender gender, FP_Ethnicity ethnicity, FP_Language firstLang, FP_Language secondLang, FP_Language thirdLang, int age, string characterSkinMeshRenderer, FP_Theme characterTheme):base(index)
        {
            NodeType = "RTCharacterNode";
            this.outNodeIndex= outNode;
            this.characterName = name;
            this.gender = gender;
            this.ethnicity = ethnicity;
            this.firstLang = firstLang;
            this.secondLang = secondLang;
            this.thirdLang = thirdLang;
            this.age = age;
            this.characterSkinMeshRendererID = characterSkinMeshRenderer;
            this.characterTheme = characterTheme;
        }
    }
    //Editor: SetFPResponseNode class
    [Serializable]
    public class RTResponseNode : RTFPNode
    {
        //this is a node of other nodes
        public List<RTSinglePromptNode> userIncomingPrompts = new List<RTSinglePromptNode>();
        
        public List<string> userOutcomesConnectors = new List<string>();
        public RTCharacterNode character;
        

        public RTResponseNode(string index, List<RTSinglePromptNode> incomingPromptNodes,List<string> outputNodes,RTCharacterNode theCharacter):base(index)
        {
            NodeType = "RTResponseNode";
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
        public string outIndex;
        [Space]
        public Sprite promptIcon;
        public string spawnLocationID;
        public GameObject spawnLocation;
        public RTTalkNode mainDialogue;
        public RTTalkNode translatedDialogue;

        public RTSinglePromptNode(string index,string outcomingNodeIndex,RTTalkNode dialogue, RTTalkNode translation, Sprite promptIcon, string spawnID,GameObject spawnLocation=null):base(index)
        {
            NodeType = "RTSinglePromptNode";
            this.spawnLocationID = spawnID;
            this.outIndex = outcomingNodeIndex;
            this.promptIcon = promptIcon;
            this.spawnLocation = spawnLocation;
            this.mainDialogue = dialogue;
            this.translatedDialogue = translation;
        }
    }
    //Editor: SetFPDialogueNode class
    [Serializable]
    public class RTDialogueNode : RTFPNode
    {
        //this is a node of other nodes
        public string incomingIndex;
        public string outgoingIndex;
        [Space]
        public RTTalkNode mainDialogue;
        public RTTalkNode translatedDialogue;
        public RTCharacterNode incomingCharacter;
        public RTCharacterNode outgoingCharacter;
        
        
        public RTDialogueNode(string index,string incominIndex, string outIndex, RTTalkNode dialogue,RTCharacterNode incomingCharacterNode,RTCharacterNode outgoingCharacaterNode = null,RTTalkNode transDialogue=null) : base(index)
        {
            NodeType = "RTDialogueNode";
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
        public string outIndex;
        [Space]
        public FP_Language language;
        public string headerText;
        public string dialogueText;  
        public AudioClip textAudio;
        public AnimationClip faceAnimation;
        public bool hasAudio;
        public bool hasAnimation;
        
        public bool HasAudio { get { return hasAudio; } }
        public bool HasAnimation { get { return hasAnimation; } }

        public RTTalkNode(string index, string outIndex, FP_Language theLanguage, string headText, string convoText, AudioClip textAudio = null, AnimationClip faceAnimation = null):base(index)
        {
            NodeType = "RTTalkNode";
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
