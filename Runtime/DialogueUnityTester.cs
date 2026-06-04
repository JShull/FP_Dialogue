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
