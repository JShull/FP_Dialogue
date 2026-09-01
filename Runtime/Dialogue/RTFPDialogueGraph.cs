// Copyright (c) 2026 John B. Shull
// FuzzPhyte LLC is a company associated with John B. Shull
// This file is part of FP_Dialogue Package.
//
// Public license: GNU GPLv3-or-later.
// Commercial/proprietary use requires a separate license from John B. Shull.
//
// See LICENSE.md COMMERCIAL-LICENSE.md, and NOTICE.md.

namespace FuzzPhyte.Dialogue
{
    using UnityEngine;
    using FuzzPhyte.Utility;
    using System.Collections.Generic;
    using System.Linq;

    [CreateAssetMenu(fileName = "RunTimeDialogueGraph", menuName = "FuzzPhyte/Dialogue/Graph/Create Runtime SOGraph")]
    public class RTFPDialogueGraph : FP_Data
    {
        [SerializeReference] public string GraphID;
        [SerializeReference] public RTEntryNode MainEntryNode;
        [SerializeReference]
        public List<RTFPNode> Nodes = new ();
        [System.NonSerialized] public Dictionary<string, RTFPNode> AllNodesByIndex = new();
        [System.NonSerialized] public Dictionary<string, RTEntryNode> AllEntryNodes = new();
        [System.NonSerialized] public Dictionary<string, RTExitNode> AllExitNodes = new();
        [System.NonSerialized] public Dictionary<string, RTDialogueNode> AllDialogueNodes = new();
        [System.NonSerialized] public Dictionary<string, RTResponseNode> AllResponseNodes = new();
        [System.NonSerialized] public Dictionary<string, RTCharacterNode> AllCharacterNodes = new();

        private void OnEnable()
        {
            RebuildNodeLookups();
        }

        /// <summary>
        /// Rebuilds runtime lookup caches from the serialized polymorphic node list.
        /// </summary>
        public void RebuildNodeLookups()
        {
            AllNodesByIndex ??= new Dictionary<string, RTFPNode>();
            AllEntryNodes ??= new Dictionary<string, RTEntryNode>();
            AllExitNodes ??= new Dictionary<string, RTExitNode>();
            AllDialogueNodes ??= new Dictionary<string, RTDialogueNode>();
            AllResponseNodes ??= new Dictionary<string, RTResponseNode>();
            AllCharacterNodes ??= new Dictionary<string, RTCharacterNode>();

            AllNodesByIndex.Clear();
            AllEntryNodes.Clear();
            AllExitNodes.Clear();
            AllDialogueNodes.Clear();
            AllResponseNodes.Clear();
            AllCharacterNodes.Clear();

            if (Nodes == null)
            {
                return;
            }

            foreach (var node in Nodes)
            {
                if (node == null || string.IsNullOrEmpty(node.Index))
                {
                    continue;
                }

                AllNodesByIndex[node.Index] = node;
                switch (node)
                {
                    case RTEntryNode entryNode:
                        AllEntryNodes[node.Index] = entryNode;
                        break;
                    case RTExitNode exitNode:
                        AllExitNodes[node.Index] = exitNode;
                        break;
                    case RTDialogueNode dialogueNode:
                        AllDialogueNodes[node.Index] = dialogueNode;
                        break;
                    case RTResponseNode responseNode:
                        AllResponseNodes[node.Index] = responseNode;
                        break;
                    case RTCharacterNode characterNode:
                        AllCharacterNodes[node.Index] = characterNode;
                        break;
                }
            }
        }
        
        /// <summary>
        /// Initial entry point for main "in" node, but also can be used for other things
        /// </summary>
        /// <param name="inNode"></param>
        public void SetupGraphEntryPoint(RTEntryNode inNode)
        {
            if (MainEntryNode == null)
            {
                MainEntryNode = inNode;
            }
            else
            {
                Debug.LogWarning($"Trying to replace the Main Entry Node, one is already assigned?!");
            }
        }
        public void AddRTNodeToList(RTFPNode Node)
        {
            if (Node == null || string.IsNullOrEmpty(Node.Index))
            {
                return;
            }

            if (!Nodes.Contains(Node))
            {
                bool match = Nodes.Any(n => n.Index == Node.Index);
                if (!match)
                {
                    Nodes.Add(Node);
                }
            }

            AllNodesByIndex[Node.Index] = Node;
        }
        public void AddEntryNode(RTEntryNode entryNode)
        {
            if (!AllEntryNodes.ContainsKey(entryNode.Index))
            {
                AllEntryNodes.Add(entryNode.Index, entryNode);
                ReturnEntryNode(entryNode.Index);
            }
            
        }
        public void AddExitNode(RTExitNode exitNode)
        {
            if (!AllExitNodes.ContainsKey(exitNode.Index))
            {
                AllExitNodes.Add(exitNode.Index, exitNode);
                ReturnExitNode(exitNode.Index);
            }
        }
        public void AddDialogueNode(RTDialogueNode dialogueNode)
        {
            if (!AllDialogueNodes.ContainsKey(dialogueNode.Index)) 
            {
                AllDialogueNodes.Add(dialogueNode.Index, dialogueNode);
                ReturnDialogueNode(dialogueNode.Index);
            }
        }
        public void AddCombineNode(RTCombineNode combineNode)
        {

        }
        public void AddCharacterNode(RTCharacterNode characterNode)
        {
            if (!AllCharacterNodes.ContainsKey(characterNode.Index)) 
            {
                AllCharacterNodes.Add(characterNode.Index, characterNode);
                ReturnCharacterNode(characterNode.Index);
            }
        }
        public void AddResponseNode(RTResponseNode responseNode)
        {
            if (!AllResponseNodes.ContainsKey(responseNode.Index))
            {
                AllResponseNodes.Add(responseNode.Index, responseNode);
                ReturnResponeNode(responseNode.Index);
            }
        }
        public void AddTalkNode(RTTalkNode talkNode)
        {

        }

        #region Test Functions
        public void ReturnEntryNode(string value)
        {
            if (AllEntryNodes.ContainsKey(value))
            {
                var entryNode = AllEntryNodes[value];
                //Debug.Log($"Details of Entry Node: {entryNode.Index}");
                if (entryNode.incomingTimelineAsset != null)
                {
                    //Debug.Log($"Timeline duration: {entryNode.incomingTimelineAsset.Timeline.duration}");
                }
                for (int i = 0; i < entryNode.NextNodeIndices.Count; i++) 
                {
                    //Debug.Log($"Entry Node connected to --> {entryNode.NextNodeIndices[i]}");
                }
            }
        }
        public void ReturnExitNode(string value)
        {
            
            if (AllExitNodes.ContainsKey(value))
            {
                var exitNode = AllExitNodes[value];
                //Debug.Log($"Details of Exit Node: {exitNode.Index}");
                if (exitNode.outgoingTimelineDetails != null)
                {
                    //Debug.Log($"Timeline duration: {exitNode.outgoingTimelineDetails.Timeline.duration}");
                }
                for (int i = 0; i < exitNode.inNodeIndices.Length; i++)
                {
                    //Debug.Log($"Exit Node incoming connection <-- {exitNode.inNodeIndices[i]}");
                }
            }
        }
        public void ReturnDialogueNode(string value)
        {
            if (AllDialogueNodes.ContainsKey(value))
            {
                var dialogeNode = AllDialogueNodes[value];
                //Debug.Log($"Details of dialoge node: {dialogeNode.Index}");
                if (dialogeNode.mainDialogue != null)
                {
                    //Debug.Log($"Main Text for Dialogue: {dialogeNode.mainDialogue.language}: {dialogeNode.mainDialogue.dialogueText}");
                    if (dialogeNode.mainDialogue.textAudio != null)
                    {
                        //Debug.Log($"Audio clip length = {dialogeNode.mainDialogue.textAudio.length}");
                    }
                }
                if (dialogeNode.translatedDialogue != null)
                {
                    //Debug.Log($"Translation for {dialogeNode.translatedDialogue.language} is {dialogeNode.translatedDialogue.dialogueText}");
                    if (dialogeNode.translatedDialogue.textAudio != null)
                    {
                        //Debug.Log($"Translation audio clip length = {dialogeNode.translatedDialogue.textAudio.length}");
                    }
                }
                
                
            }
        }
        public void ReturnResponeNode(string value)
        {
            if (AllResponseNodes.ContainsKey(value))
            {
                var responseNode = AllResponseNodes[value];
                //Debug.Log($"Details of response node: {responseNode.Index}");
                if (responseNode.userIncomingPrompts.Count > 0)
                {
                    for (int i = 0; i < responseNode.userIncomingPrompts.Count; i++)
                    {
                        var aResponse = responseNode.userIncomingPrompts[i];
                        if (aResponse != null)
                        {
                            if (aResponse.mainDialogue != null)
                            {
                                //Debug.Log($"Main Text for Response: {aResponse.mainDialogue.language}: {aResponse.mainDialogue.dialogueText}");
                                if (aResponse.mainDialogue.textAudio != null)
                                {
                                    //Debug.Log($"Audio clip length = {aResponse.mainDialogue.textAudio.length}");
                                }
                            }
                        }
                        
                    }
                }
            }
        }
        public void ReturnCharacterNode(string value)
        {
            if (AllCharacterNodes.ContainsKey(value))
            {
                var characterNode = AllCharacterNodes[value];
                //Debug.Log($"Details of character node: {characterNode.Index}");
                
            }
        }
        #endregion
    }
}
