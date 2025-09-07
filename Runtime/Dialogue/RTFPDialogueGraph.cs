namespace FuzzPhyte.Dialogue
{
    using UnityEngine;
    using FuzzPhyte.Utility;
    using System.Collections.Generic;
    using System.Linq;

    [CreateAssetMenu(fileName = "RunTimeDialogueGraph", menuName = "FuzzPhyte/Dialogue/Graph/Create Runtime SOGraph")]
    public class RTFPDialogueGraph : FP_Data
    {
        [SerializeReference]
        public List<RTFPNode> Nodes = new ();
        public Dictionary<string, RTFPNode> AllNodesByIndex = new Dictionary<string,RTFPNode> ();
        public Dictionary<string, RTEntryNode> AllEntryNodes = new Dictionary<string, RTEntryNode>();
        public Dictionary<string, RTExitNode> AllExitNodes = new Dictionary<string,RTExitNode>();
        public Dictionary<string, RTDialogueNode>AllDialogueNodes = new Dictionary<string,RTDialogueNode>();
        public Dictionary<string, RTResponseNode>AllResponseNodes = new Dictionary<string,RTResponseNode> ();
        public Dictionary<string, RTCharacterNode>AllCharacterNodes = new Dictionary<string,RTCharacterNode> ();
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
        
        public void ReturnEntryNode(string value)
        {
            if (AllEntryNodes.ContainsKey(value))
            {
                var entryNode = AllEntryNodes[value];
                Debug.Log($"Details of Entry Node: {entryNode.Index}");
                if (entryNode.GetTimelineAssetFile != null)
                {
                    Debug.Log($"Timeline duration: {entryNode.GetIncomingTimeline.duration}");
                }
                for (int i = 0; i < entryNode.NextNodes.Length; i++) 
                {
                    Debug.Log($"Entry Node connected to --> {entryNode.NextNodes[i]}");
                }
            }
        }
        public void ReturnExitNode(string value)
        {
            
            if (AllExitNodes.ContainsKey(value))
            {
                var exitNode = AllExitNodes[value];
                Debug.Log($"Details of Exit Node: {exitNode.Index}");
                if (exitNode.GetTimelineAssetFile != null)
                {
                    Debug.Log($"Timeline duration: {exitNode.GetOutgoingTimeline.duration}");
                }
                for (int i = 0; i < exitNode.InNodes.Length; i++)
                {
                    Debug.Log($"Exit Node incoming connection <-- {exitNode.InNodes[i]}");
                }
            }
        }
        public void ReturnDialogueNode(string value)
        {
            if (AllDialogueNodes.ContainsKey(value))
            {
                var dialogeNode = AllDialogueNodes[value];
                Debug.Log($"Details of dialoge node: {dialogeNode.Index}");
                if (dialogeNode.GetMainDialogue != null)
                {
                    Debug.Log($"Main Text for Dialogue: {dialogeNode.GetMainDialogue.Language}: {dialogeNode.GetMainDialogue.DialogueText}");
                    if (dialogeNode.GetMainDialogue.AudioClip != null)
                    {
                        Debug.Log($"Audio clip length = {dialogeNode.GetMainDialogue.AudioClip.length}");
                    }
                }
                if (dialogeNode.GetTranslationDialogue != null)
                {
                    Debug.Log($"Translation for {dialogeNode.GetTranslationDialogue.Language} is {dialogeNode.GetTranslationDialogue.DialogueText}");
                    if (dialogeNode.GetTranslationDialogue.AudioClip != null)
                    {
                        Debug.Log($"Translation audio clip length = {dialogeNode.GetTranslationDialogue.AudioClip.length}");
                    }
                }
                
                
            }
        }
        public void ReturnResponeNode(string value)
        {
            if (AllResponseNodes.ContainsKey(value))
            {
                var responseNode = AllResponseNodes[value];
                Debug.Log($"Details of response node: {responseNode.Index}");
                if (responseNode.UserPrompts.Count > 0)
                {
                    for (int i = 0; i < responseNode.UserPrompts.Count; i++)
                    {
                        var aResponse = responseNode.UserPrompts[i];
                        if (aResponse.GetMainDialogue != null)
                        {
                            Debug.Log($"Main Text for Response: {aResponse.GetMainDialogue.Language}: {aResponse.GetMainDialogue.DialogueText}");
                            if (aResponse.GetMainDialogue.AudioClip != null)
                            {
                                Debug.Log($"Audio clip length = {aResponse.GetMainDialogue.AudioClip.length}");
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
                Debug.Log($"Details of character node: {characterNode.Index}");
                //if(characterNode.)
            }
        }

    }
}
