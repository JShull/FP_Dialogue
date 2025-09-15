namespace FuzzPhyte.Dialogue
{
    using FuzzPhyte.Utility;
    using UnityEngine;
    using System;

    //struct that is passed around via the delegate through the FP_Dialogue_Manager
    [Serializable]
    public struct DialogueEventData
    {
        public string UserID;
        public int PotentialUserResponseIndex;
        public DialogueResponse PotentialUserResponse;
        public DialogueBase DialogueDataRef;
        public DialogueBlock DialogueBlockDataRef;
    }
    
}
