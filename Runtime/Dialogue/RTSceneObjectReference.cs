namespace FuzzPhyte.Dialogue
{
    using System;
    using UnityEngine;
    using FuzzPhyte.Utility;
    [Serializable]
    [CreateAssetMenu(fileName = "RTSceneObjectReference", menuName = "FuzzPhyte/Dialogue/Graph/CreateSceneOBJRef.")]
    public class RTSceneObjectReference : FP_Data
    {
        [SerializeField]
        public ExposedReference<GameObject> SceneObject;
    }
}
