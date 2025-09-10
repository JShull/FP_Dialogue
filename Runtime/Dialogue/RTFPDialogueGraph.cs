namespace FuzzPhyte.Dialogue
{
    using UnityEngine;
    using FuzzPhyte.Utility;
    using System.Collections.Generic;
    using System.Linq;

    [CreateAssetMenu(fileName = "RunTimeDialogueGraph", menuName = "FuzzPhyte/Dialogue/Graph/Create Runtime SOGraph")]
    public class RTFPDialogueGraph : FP_Data
    {
        [SerializeReference] public RTEntryNode MainEntryNode;
        [SerializeReference]
        public List<RTFPNode> Nodes = new ();
        [SerializeReference]public Dictionary<string, RTFPNode> AllNodesByIndex = new Dictionary<string,RTFPNode> ();
        [SerializeReference] public Dictionary<string, RTEntryNode> AllEntryNodes = new Dictionary<string, RTEntryNode>();
        [SerializeReference] public Dictionary<string, RTExitNode> AllExitNodes = new Dictionary<string,RTExitNode>();
        [SerializeReference] public Dictionary<string, RTDialogueNode>AllDialogueNodes = new Dictionary<string,RTDialogueNode>();
        [SerializeReference] public Dictionary<string, RTResponseNode>AllResponseNodes = new Dictionary<string,RTResponseNode> ();
        [SerializeReference] public Dictionary<string, RTCharacterNode>AllCharacterNodes = new Dictionary<string,RTCharacterNode> ();
        
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
            if (!Nodes.Contains(Node))
            {
                bool match = Nodes.Any(n => n.Index == Node.Index);
                if (!match)
                {
                    Nodes.Add(Node);
                }
            }
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
