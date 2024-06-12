namespace FuzzPhyte.Dialogue
{
    using UnityEngine;
    using FuzzPhyte.Utility;
    using System;

    [Serializable]
    [CreateAssetMenu(fileName = "CharacterData", menuName = "FuzzPhyte/Dialogue/Character", order = 1)]
    public class FP_Character : FP_Data
    {
        public string CharacterName;
        public FP_Gender CharacterGender;
        public FP_Theme CharacterTheme;
        public int CharacterAge;
    }
}
