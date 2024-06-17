namespace FuzzPhyte.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using FuzzPhyte.Utility;
    using UnityEditor.EditorTools;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Dialogue Response", menuName = "FuzzPhyte/Dialogue/Dialogue Response", order = 7)]
    public class DialogueResponse : FP_Data
    {
        public string ResponseText;
        public bool FinishDialogue;
        public Sprite ResponseIcon;
        [Space]
        [Header("Dialogue Item Generation")]
        [Tooltip("Are we going to generate this item?")]
        public bool ReceivedItem;
        [Tooltip("Generated ID/Item")]
        public FP_Data ReceivedObjectID;
        [Space]
        [Tooltip("Are we going to give an item?")]
        public bool GiveAnItem;
        [Tooltip("Given ID/Item")]
        public FP_Data GivenObjectID;
    }
}
