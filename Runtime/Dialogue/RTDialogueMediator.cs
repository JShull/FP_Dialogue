namespace FuzzPhyte.Dialogue
{
    using UnityEngine;

    /// <summary>
    /// Runtime Unity Mediator
    /// In charge of managing the Unity real time nodes and processing
    /// reading the graph
    /// which node we are actively on
    /// 
    /// </summary>
    public class RTDialogueMediator : MonoBehaviour
    {
        public bool EvaluateCondition(RTFPNode node)
        {
            return false;
        }
        public bool EvaluateEntryNode(RTEntryNode node)
        {
            var tAsset = node.incomingTimelineAsset;
            if (tAsset!=null)
            {
                Debug.Log($"Entry Node has a timeline asset");
                return true;
            }
            else
            {
                Debug.Log($"Entry node does not have a timeline asset");
                return true;
            }
            //return false;
        }
        public bool EvaluateExitNode(RTExitNode node)
        {
            var tAsset = node.timelineAsset;
            if (tAsset != null) 
            {
                Debug.Log($"Exit Node has a timeline asset");
                return true;
            }
            else
            {
                Debug.Log($"Exit node does not have a timeline asset!");
                return true;
            }

        }
        public bool EvaluateDialogueNode(RTDialogueNode node)
        {
            var mainDialogueData = node.mainDialogue;
            if (mainDialogueData != null) 
            {
                Debug.Log($"Main Text: {mainDialogueData.dialogueText} in the language of {mainDialogueData.language.ToString()}");
            }
            else
            {
                return false;
            }
            return true;
        }
        
        public bool EvaluateTimelineDetails(RTTimelineDetails details)
        {
            if (details != null)
            {
                return true;
            }
            return false;
        }
        public bool EvaluateResponseNode(RTResponseNode node)
        {
            var mainResponseData = node.userIncomingPrompts;
            if(mainResponseData != null)
            {
                for(int i=0;i< mainResponseData.Count; i++)
                {
                    Debug.Log($"Response Index{i}: {mainResponseData[i].mainDialogue.dialogueText} in the language of {mainResponseData[i].mainDialogue.language.ToString()}");
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        public bool EvaluateCombination(RTCombineNode node)
        {
            var incomingNodes = node.inNodeIndices;
            var outgoingNodes = node.outNodeIndices;
            if (incomingNodes.Length > 0 && outgoingNodes.Length>0)
            {
                string combinedNodes = string.Empty;
                for (int i = 0; i < incomingNodes.Length; i++)
                {
                    if (i == incomingNodes.Length - 1)
                    {
                        combinedNodes += incomingNodes[i];
                    }
                    else
                    {
                        combinedNodes += incomingNodes[i] + ", ";
                    }
                }
                string combineNodesOut = string.Empty;
                for (int i = 0; i < outgoingNodes.Length; i++)
                {
                    if (i == outgoingNodes.Length - 1)
                    {
                        combineNodesOut += outgoingNodes[i];
                    }
                    else
                    {
                        combineNodesOut += outgoingNodes[i] + ", ";
                    }

                }
                Debug.Log($"Combine Nodes in: {combinedNodes} and combine nodes out: {combineNodesOut}");
                return true;
            }
            return false;
        }
    }
}
