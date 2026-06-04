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
