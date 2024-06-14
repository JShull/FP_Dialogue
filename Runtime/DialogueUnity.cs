using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuzzPhyte.Dialogue
{
    //Mono class to hold all of our data and various connections for runtime needs of passing that data around via the FP_Dialogue_Manager
    public class DialogueUnity : MonoBehaviour
    {
        [Tooltip("The core data for this dialogue")]
        public DialogueBase MainDialogueData;

        public int DialogueIndex = 0;
        private Canvas canvasRef;
        
        [Tooltip("The prefab to spawn for the UI dialogue block with content references as needed")]
        public GameObject UIDialoguePrefab;
        public List<GameObject> DialogueBlockList = new List<GameObject>();
        #region Delegate Events and Requirements
        public delegate void DialogueDelegate(DialogueEventData dialogueData);

        public event DialogueDelegate DialogueStartEvent;
        public event DialogueDelegate DialogueNextButtonEvent;
        public event DialogueDelegate DialoguePreviousButtonEvent;
        public event DialogueDelegate DialogueUserPromptEvent;
        public event DialogueDelegate DialogueEndEvent;
        #endregion

        public void SetupDialogue(Canvas theCanvasToUse)
        {
            //setup all spawnable UI items and cache them
            canvasRef = theCanvasToUse;
            //spawn all the UI items
            //cache them
            //set index

        }
        public void ActivateDialogue()
        {
            //show whatever UI based on the index
        }
        public void UINextButtonPushed()
        {
            //check if we are at the end of the dialogue
            //activate next dialogue if there is one
        }
        public void UIPreviousButtonPushed()
        {
            //check if we are at the beginning of the dialogue
            //activate previous dialogue if there is one

        } 
    }
}
