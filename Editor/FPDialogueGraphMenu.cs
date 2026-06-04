// Copyright (c) 2026 John B. Shull
// FuzzPhyte LLC is a company associated with John B. Shull
// This file is part of FP_Dialogue Package.
//
// Public license: GNU GPLv3-or-later.
// Commercial/proprietary use requires a separate license from John B. Shull.
//
// See LICENSE.md COMMERCIAL-LICENSE.md, and NOTICE.md.

namespace FuzzPhyte.Dialogue.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using Unity.GraphToolkit.Editor;

    public static class FPDialogueGraphMenu
    {
        private const string CreatePath = "Assets/Create/FuzzPhyte/Dialogue/Graph/";

        [MenuItem(CreatePath + "Create Blank Dialogue Graph", priority = 0)]
        public static void CreateDialogueGraph()
        {
            string dateString = System.DateTime.Now.ToShortDateString();
            string safeDateString = dateString.Replace("/", "-").Replace("\\", "-");
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<FPDialogueGraph>("FPDialogueGraph_"+safeDateString);
        }
        [MenuItem("FuzzPhyte/Dialogue/Graph/Create Blank Graph", priority = FuzzPhyte.Utility.FP_UtilityData.MENU_UTILITY_DIALOGUE)]
        public static void CreateDialogueGraphMenu()
        {
            string dateString = System.DateTime.Now.ToShortDateString();
            string safeDateString = dateString.Replace("/", "-").Replace("\\", "-");
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<FPDialogueGraph>("FPDialogueGraph_" + safeDateString);
        }

    }
}
