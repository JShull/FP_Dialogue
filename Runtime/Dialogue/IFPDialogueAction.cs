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
    /// <summary>Scene component contract for immediate dialogue actions.</summary>
    public interface IFPDialogueAction
    {
        /// <summary>Start the action and return; dialogue does not await completion.</summary>
        void ExecuteDialogueAction(GraphEventData data);
    }
}
