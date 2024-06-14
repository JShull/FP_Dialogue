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

        public delegate void DialogueDelegate(DialogueEventData dialogueData);

        public event DialogueDelegate DialogueStartEvent;
        public event DialogueDelegate DialogueNextButtonEvent;
        public event DialogueDelegate DialoguePreviousButtonEvent;
        public event DialogueDelegate DialogueUserPromptEvent;
        public event DialogueDelegate DialogueEndEvent;

        public void UINextButtonPushed()
        {

        }
        public void UIPreviousButtonPushed()
        {

        } 
    }
}
