namespace FuzzPhyte.Dialogue
{
    using System;

    //struct that is passed around via the delegate through the FP_Dialogue_Manager
    [Serializable]
    public struct NarratorEventData
    {
        public string UserID;
        public DialogueBase DialogueDataRef;
        
        public DialogueBlock DialogueBlockDataRef;
    }
}
