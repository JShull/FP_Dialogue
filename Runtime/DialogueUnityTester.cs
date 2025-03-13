namespace FuzzPhyte.Dialogue
{ 
    using UnityEngine;
    /// <summary>
    /// Quick mono script used for testing the DialogueUnity class
    /// </summary>
    [RequireComponent(typeof(DialogueUnity))]
    public class DialogueUnityTester : MonoBehaviour
    {
        protected DialogueUnity eventRef;
        public bool TestFromData = false;
        public KeyCode TestKey = KeyCode.Space;
        public Canvas CanvasTestRef;
        public string ClientTestID;
        protected bool setupComplete;
        
        protected virtual void Awake()
        {
            eventRef = this.GetComponent<DialogueUnity>();
            //other actions in start
        }
        private void Start()
        {
            //testing
            if (TestFromData && eventRef != null)
            {
                eventRef.SetupDialogue(CanvasTestRef, ClientTestID);
                setupComplete = true;
            }
        }
        protected virtual void Update()
        {
            if (TestFromData && Input.GetKeyUp(TestKey) && !eventRef.DialogueActive &&setupComplete)
            {
                eventRef.ActivateDialogue();
            }
        }
    }
}
