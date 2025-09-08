namespace FuzzPhyte.Dialogue
{
    using UnityEngine;

    /// <summary>
    /// Runtime Unity Mediator
    /// In charge of managing the Unity runtime environment being built by the graph editor data
    /// reading the graph
    /// which node we are actively on
    /// traversial of the graph / runtime
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
            }
            return false;
        }
        public bool EvaluateDialogueNode(RTDialogueNode node)
        {
            var mainDialogueData = node.mainDialogue;
            if (mainDialogueData != null) 
            {
                Debug.Log($"Main Text: {mainDialogueData.dialogueText} in the language of {mainDialogueData.language.ToString()}");
            }
                return true;
        }
    }
}
