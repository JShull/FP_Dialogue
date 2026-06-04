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
    using System;

    [Serializable]
    [CreateAssetMenu(fileName = "CharacterData", menuName = "FuzzPhyte/Dialogue/Character", order = 1)]
    public class FP_Character : FP_Data
    {
        public string CharacterName;
        public FP_Gender CharacterGender;
        public FP_Ethnicity CharacterEthnicity;
        public FP_Multilingual CharacterLanguages;
        public FP_Theme CharacterTheme;
        public int CharacterAge;
    }
}
