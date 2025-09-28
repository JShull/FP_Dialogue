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
        [MenuItem("FuzzPhyte/Dialogue/Graph/Create Blank Graph", priority = FuzzPhyte.Utility.FP_UtilityData.ORDER_SUBMENU_LVL6)]
        public static void CreateDialogueGraphMenu()
        {
            string dateString = System.DateTime.Now.ToShortDateString();
            string safeDateString = dateString.Replace("/", "-").Replace("\\", "-");
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<FPDialogueGraph>("FPDialogueGraph_" + safeDateString);
        }

    }
}
