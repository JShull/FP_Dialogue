namespace FuzzPhyte.Dialogue
{

    using System.Collections.Generic;
    using UnityEngine;
    public class FP_Dialogue_Manager : MonoBehaviour
    {
        [Tooltip("Index of the current dialogue")]
        protected int dialogueIndex=0;
        [Tooltip("All DialogueUnity in the scene")]
        protected List<DialogueUnity> dialoguePlaceholderList = new List<DialogueUnity>();
        [Tooltip("A cached index by user string by active dialogue and the spawned UI related blocks associated with that dialogue base")]
        protected Dictionary<string,List<GameObject>> UserCurrentVisualDialogueBlocks = new Dictionary<string, List<GameObject>>();
        protected Dictionary<string,GameObject> UserCurrentDialogue = new Dictionary<string, GameObject>();
       
        /* Notes
            This class primary purpose is to manage the various DialogueUnity objects in the scene
            On start we need to then listen for the various events and when those events are invoked
            we just need to make sure we are syncing other services like network requirements etc.
            we might be changing various layout features and/or generate other components
            we are assuming these objects are in the Unity Rendering environment and thus 'GameObjects'
        */ 
        public virtual void ResetDialogueModel()
        {

        }
        public virtual void DialogueNextButtonAction()
        {

        }
        public virtual void DialoguePreviousButtonAction()
        {
            //clear out existing dialogue data
            //hide overlays and or other children items of our parent container
            //show the parent container
            //reset and show both buttons forward/backward conversation buttons
            //reset the index to 0
        }
        public virtual void DialogueUserPromptAction()
        {
            //this is a user prompt, we need to wait for a user input to continue
            //will have to filter through the data to see where to go next
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isComplete">if no other dialogue and there's no waiting on a user input = true</param>
        public virtual void InternalDialogueEnd(bool isComplete)
        {
            //we receive notice from the current DialogueUnity that it's 'done' and we activate our delegate
            //if isComplete is true then we can do a full end as this manager is "done"
            //if isComplete is false, we are then just waiting for a user input to advance
        }
    }
}
