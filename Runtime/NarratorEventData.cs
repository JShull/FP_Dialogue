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
