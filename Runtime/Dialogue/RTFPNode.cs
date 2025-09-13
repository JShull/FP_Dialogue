namespace FuzzPhyte.Dialogue
{
    using FuzzPhyte.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Timeline;
    [Serializable]
    public enum FPPortType
    {
        NA=0,
        INPort=1,
        OUTPort=2,
    }
    /// <summary>
    /// Runtime Abstract Dialogue Node
    /// </summary>
    [Serializable]
    public abstract class RTFPNode
    {
        [SerializeField]
        public string NodeType;
        [SerializeField]
        public string Index;
        [Space]
        [Header("Graph Indices")]
        public List<int> NextNodeIndices = new List<int>();
        [Space]
        [Header("Dialoge Indices")]
        public RTFPNodePort[] inNodeIndices;
        public RTFPNodePort[] outNodeIndices;
        public RTFPNode(string index)
        {
            Index = index;
        }
    }
    /// <summary>
    /// A Port and the details of the ConnectedNodes (node and port it's connected to in an array)
    /// </summary>
    [Serializable]
    public struct RTFPNodePort
    {
        public string MyPort;
        public RTFPNodeDetails[] ConnectedNodes;
    }
    /// <summary>
    /// a Node and Port index for the dialogue needs
    /// </summary>
    [Serializable]
    public struct RTFPNodeDetails
    {
        public FPPortType PortType;
        public string NodeIndex;
        public string PortIndex;
    }
    #region Runtime Nodes Associated with Dialogue System
    //Editor: EntryNode class
    [Serializable]
    public class RTEntryNode : RTFPNode
    {
        [Space]
        [Header("Entry Node Details")]
        public RTTimelineDetails incomingTimelineAsset;
        public TimelineAsset timelineAsset;
        
        public RTEntryNode(string index, RTFPNodePort outIndexNode) : base(index)
        {
            NodeType = "RTEntryNode";
            this.outNodeIndices = new RTFPNodePort[1];
            this.outNodeIndices[0] = outIndexNode;
        }
        public RTEntryNode(string index,List<RTFPNodePort>outIndexNode, RTTimelineDetails inTimelineAsset =null):base(index)
        {
            this.outNodeIndices = outIndexNode.ToArray();
            NodeType = "RTEntryNode";
            if(incomingTimelineAsset == null && inTimelineAsset != null)
            {
                incomingTimelineAsset = inTimelineAsset;
                timelineAsset = incomingTimelineAsset.Timeline;
            }
        }
        public RTEntryNode(string index, List<RTFPNodePort> outIndexNode, TimelineAsset inTimelineAsset = null) : base(index) 
        {
            NodeType = "RTEntryNode";
            this.outNodeIndices = outIndexNode.ToArray();
            timelineAsset = inTimelineAsset;
        }

    }
    //Editor: ExitNode class
    [Serializable]
    public class RTExitNode : RTFPNode
    {
        [Space]
        [Header("Exit Node Details")]
        public RTTimelineDetails outgoingTimelineDetails;
        public TimelineAsset timelineAsset;

        public RTExitNode(string index,RTFPNodePort inIndexNode) : base(index)
        {
            NodeType = "RTExitNode";
            this.inNodeIndices = new RTFPNodePort[1];
            this.inNodeIndices[0] = inIndexNode;
        }
        public RTExitNode(string index, List<RTFPNodePort> incomingNodeExit,RTTimelineDetails outTimelineAsset = null):base(index)
        {
            inNodeIndices = incomingNodeExit.ToArray();
            NodeType = "RTExitNode";
            if (timelineAsset == null && outTimelineAsset != null)
            {
                outgoingTimelineDetails = outTimelineAsset;
                timelineAsset = outTimelineAsset.Timeline;
            }
        }
        public RTExitNode(string index, List<RTFPNodePort> incomingNodeExit, TimelineAsset outTimelineAsset = null) : base(index)
        {
            inNodeIndices = incomingNodeExit.ToArray();
            NodeType = "RTExitNode";
            timelineAsset = outTimelineAsset;
            
        }
    }

    [Serializable]
    public class RTOnewayNode :RTFPNode
    {
        public RTOnewayNode(string index, RTFPNodePort inIndexNode, RTFPNodePort outIndexNode) : base(index)
        {
            NodeType = "RTOnewayNode";
            this.inNodeIndices = new RTFPNodePort[1];
            this.inNodeIndices[0] = inIndexNode;
            this.outNodeIndices = new RTFPNodePort[1];
            this.outNodeIndices[0] = outIndexNode;
        }
    }
    
    [Serializable]
    public class RTCombineNode : RTFPNode
    {
        public RTCombineNode(string index, RTFPNodePort nodeOne, RTFPNodePort nodeTwo, List<RTFPNodePort> outcomeNodes):base(index)
        {
            NodeType = "RTCombineNode";
            this.inNodeIndices = new RTFPNodePort[2];
            this.inNodeIndices[0] = nodeOne;
            this.inNodeIndices[1] = nodeTwo;
            this.outNodeIndices = outcomeNodes.ToArray();
        }
        public RTCombineNode(string index, List<RTFPNodePort>incomingNodes, List<RTFPNodePort> outcomeNode):base(index)
        {
            NodeType = "RTCombineNode";
            this.inNodeIndices = incomingNodes.ToArray();
            this.outNodeIndices = outcomeNode.ToArray();
        }
    }
    //Editor: SetFPCharacterNode class
    [Serializable]
    public class RTCharacterNode : RTFPNode
    {
        [Space]
        [Header("Characte Node Details")]
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
        [Space]
        [Header("Response Node Details")]
        //this is a node of other nodes
        public List<RTSinglePromptNode> userIncomingPrompts = new List<RTSinglePromptNode>();
        
        //public List<string> userOutcomesConnectors = new List<string>();
        public RTCharacterNode character;
        
        public RTResponseNode(string index, RTFPNodePort inIndexNode,List<RTSinglePromptNode>incomingPromptNodes, List<RTFPNodePort> outIndexPrompts,RTCharacterNode theCharacter) : base(index)
        {
            NodeType = "RTResponseNode";
            if (incomingPromptNodes.Count == outIndexPrompts.Count)
            {
                userIncomingPrompts.Clear();
                outNodeIndices = outIndexPrompts.ToArray();
                inNodeIndices = new RTFPNodePort[1];
                inNodeIndices[0] = inIndexNode;
                userIncomingPrompts.AddRange(incomingPromptNodes);
            }
            else
            {
                Debug.LogError($"Lists can't be different sizes");
            }
            character = theCharacter;
        }
        public bool ValidPromptResponse(RTSinglePromptNode prompt)
        {
            return userIncomingPrompts.Contains(prompt) ? true : false;
        }
        public RTFPNodePort? UserPromptResponseNodeDetails(RTSinglePromptNode prompt)
        {
            if (userIncomingPrompts.Contains(prompt))
            {
                //index of prompt in list?
                var validIndex = userIncomingPrompts.FindIndex(a => a == prompt);
                if (outNodeIndices.Length >= validIndex)
                {
                    return outNodeIndices[validIndex];
                }
            }
            return null;
        }
    }
    //Editor: SetFPSinglePromptNode
    [Serializable]
    public class RTSinglePromptNode : RTFPNode
    {
        [Space]
        [Header("Prompt Details")]
        public Sprite promptIcon;
        public string spawnLocationID;
        public GameObject spawnLocation;
        public RTTalkNode mainDialogue;
        public RTTalkNode translatedDialogue;

        public RTSinglePromptNode(string index, RTFPNodePort outcomingNodeIndex,RTTalkNode dialogue, RTTalkNode translation, Sprite promptIcon, string spawnID,GameObject spawnLocation=null):base(index)
        {
            NodeType = "RTSinglePromptNode";
            this.spawnLocationID = spawnID;
            this.outNodeIndices = new RTFPNodePort[1];
            this.outNodeIndices[0] = outcomingNodeIndex;
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
        //public string incomingIndex;
        //public string outgoingIndex;
        [Space]
        public RTTalkNode mainDialogue;
        public RTTalkNode translatedDialogue;
        public RTCharacterNode incomingCharacter;
        public RTCharacterNode outgoingCharacter;
        public ExposedReference<GameObject> YesPrefab;
        public ExposedReference<GameObject> NoPrefab;
        public bool waitforUser;//drives a UI prompt box for next/previous/repeat
        public string YesGameObjectSceneName;
        public string NoGameObjectSceneName;
        public bool usePrefabs;
        public bool useNames;
        public RTDialogueNode(string index, RTFPNodePort incominIndex, RTFPNodePort outIndex, RTTalkNode dialogue,RTCharacterNode incomingCharacterNode,bool waitForUser = false,RTCharacterNode outgoingCharacterNode = null,RTTalkNode transDialogue=null) : base(index)
        {
            usePrefabs = false;
            useNames = false;
            NodeType = "RTDialogueNode";
            waitforUser = waitForUser;
            this.inNodeIndices = new RTFPNodePort[1];
            this.inNodeIndices[0] = incominIndex;
            this.outNodeIndices = new RTFPNodePort[1];
            this.outNodeIndices[0] = outIndex;
            
            this.mainDialogue = dialogue;
            this.translatedDialogue = transDialogue;
            this.incomingCharacter = incomingCharacterNode;
            this.outgoingCharacter = outgoingCharacterNode;
        }
        public RTDialogueNode(string index, RTFPNodePort incominIndex,RTFPNodePort outIndex, RTTalkNode dialogue,RTCharacterNode incomingCharacterNode, ExposedReference<GameObject> yesPrefab, ExposedReference<GameObject> noPrefab,bool waitForUser = false, RTCharacterNode outgoingCharacterNode = null,RTTalkNode transDialogue = null) : base(index)
        {
            usePrefabs = true;
            useNames = false;
            NodeType = "RTDialogueNode";
            waitforUser = waitForUser;
            this.inNodeIndices = new RTFPNodePort[1];
            this.inNodeIndices[0] = incominIndex;
            this.outNodeIndices = new RTFPNodePort[1];
            this.outNodeIndices[0] = outIndex;

            this.mainDialogue = dialogue;
            this.translatedDialogue = transDialogue;
            this.incomingCharacter = incomingCharacterNode;
            this.outgoingCharacter = outgoingCharacterNode;
            this.YesPrefab = yesPrefab;
            this.NoPrefab = noPrefab;
        }
        public RTDialogueNode(string index, RTFPNodePort incominIndex, RTFPNodePort outIndex, RTTalkNode dialogue, RTCharacterNode incomingCharacterNode, string yesGameObjectName, string noGameObjectName, bool waitForUser = false, RTCharacterNode outgoingCharacterNode = null, RTTalkNode transDialogue = null) : base(index)
        {
            usePrefabs = false;
            useNames = true;
            NodeType = "RTDialogueNode";
            waitforUser = waitForUser;
            this.inNodeIndices = new RTFPNodePort[1];
            this.inNodeIndices[0] = incominIndex;
            this.outNodeIndices = new RTFPNodePort[1];
            this.outNodeIndices[0] = outIndex;

            this.mainDialogue = dialogue;
            this.translatedDialogue = transDialogue;
            this.incomingCharacter = incomingCharacterNode;
            this.outgoingCharacter = outgoingCharacterNode;
            this.YesGameObjectSceneName = yesGameObjectName;
            this.NoGameObjectSceneName = noGameObjectName;
        }
    }
    //Editor: SetFPTalkNode class
    [Serializable]
    public class RTTalkNode : RTFPNode
    {
        //public string outIndex;
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

        public RTTalkNode(string index, RTFPNodePort outIndex, FP_Language theLanguage, string headText, string convoText, AudioClip textAudio = null, AnimationClip faceAnimation = null):base(index)
        {
            NodeType = "RTTalkNode";
            this.outNodeIndices = new RTFPNodePort[1];
            this.outNodeIndices[0]= outIndex;
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
