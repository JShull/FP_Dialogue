using System.Collections;
using System.Collections.Generic;
using FuzzPhyte.Utility.Notification;
using UnityEngine;

namespace FuzzPhyte.Dialogue
{
    public class FP_Dialogue_Manager : MonoBehaviour
    {
        [Tooltip("Index of the current dialogue")]
        protected int dialogueIndex=0;

        protected List<GameObject> dialoguePlaceholderList = new List<GameObject>();
        protected List<GameObject> spawnedVisuals = new List<GameObject>();
        #region Delegate Related Parameters
        public delegate void DialogueDelegate(FP_OverlayNotification dialogueData);
        public event DialogueDelegate DialogueStartEvent;
        public event DialogueDelegate DialogueNextButtonEvent;
        public event DialogueDelegate DialoguePreviousButtonEvent;
        public event DialogueDelegate DialogueUserPromptEvent;
        public event DialogueDelegate DialogueEndEvent;
        #endregion
        /* Notes
            This class needs to hold the data for a dialogue and manage that flow
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
            //will have to filter through the FP_OverlayNotification data to see where to go next
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isComplete">if no other dialogue and there's no waiting on a user input = true</param>
        public virtual void InternalDialogueEnd(bool isComplete)
        {
            //we receive notice from the current FP_Dialogue that it's 'done' and we activate our delegate
            //if isComplete is true then we can do a full end as this manager is "done"
            //if isComplete is false, we are then just waiting for a user input to advance
        }
    }
}
