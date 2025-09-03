namespace FuzzPhyte.Dialogue
{
    using UnityEngine;
    using FuzzPhyte.Utility;

    //Runtime entry point for animation needs to work with the Dialogue Graph

    public class FPCharacterAnimatorController : MonoBehaviour
    {
        public Animator BodyAnimator;
        public Animator FaceAnimator;

        public virtual void OverrideFaceAnimation(AnimationClip clip)
        {
            if (FaceAnimator != null)
            {
                //FaceAnimator.
            }
        }
    }
}
