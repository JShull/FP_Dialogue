using FuzzPhyte.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuzzPhyte.Dialogue
{
    /// <summary>
    /// This scriptable object allows us to combine a dialogue event with a passed data event
    /// we then pass this scriptable object to the CC_Manager for things like end of WebGL Scene
    /// </summary>

    [Serializable]
    [CreateAssetMenu(fileName = "ComboEvent", menuName = "FuzzPhyte/Dialogue/ComboEvent", order = 6)]
    public class FP_ComboConvo : FP_Notification
    {
        public FP_Dialogue DialogueData;
        [Tooltip("This string needs to match a function on the CC_Manager class")]
        public string ManagerMethodName;
    }
}
