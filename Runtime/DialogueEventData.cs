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
        public DialogueUserResponse PotentialUserResponse;
        public DialogueBase DialogueDataRef;
        public DialogueBlock DialogueBlockDataRef;
    }
    //struct for user response data
    [Serializable]
    public struct DialogueUserResponse
    {
        public string ResponseText;
        public Sprite ResponseIcon;
        [Tooltip("If we need to check inventory for an item")]
        public FP_Data InventoryUniqueObjectID;
    }
}
