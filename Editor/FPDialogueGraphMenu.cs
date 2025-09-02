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
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<FPDialogueGraph>("FPDialogueGraph");
        }
        [MenuItem(CreatePath + "Create Graph (from DialogueBase)", priority = 1)]
        public static void CreateGraphFromDialogueBase()
        {
            var db = Selection.activeObject as DialogueBase;
            if (db == null)
            {
                EditorUtility.DisplayDialog("Create Graph", "Select a DialogueBase asset first.", "OK");
                return;
            }

            // Create a sibling .fpdlg graph asset next to the DialogueBase
            var basePath = AssetDatabase.GetAssetPath(db);
            var folder = Path.GetDirectoryName(basePath);
            var nameNoExt = Path.GetFileNameWithoutExtension(basePath);
            var graphPath = Path.Combine(folder ?? "Assets", $"{nameNoExt}.fpdlg");

            var graph = FPDialogueGraphIO.CreateOrLoadGraph(graphPath, db);
            FPDialogueGraphIO.ImportFrom(db, graph);
            EditorUtility.DisplayDialog("Create Graph", "Graph created & imported.", "OK");
        }
       

        [MenuItem(CreatePath + "Export Graph → DialogueBase", priority = 2)]
        public static void ExportGraphToDialogueBase()
        {
            var sel = Selection.activeObject;
            if (sel == null)
            {
                EditorUtility.DisplayDialog("Export Graph", "Select an .fpdlg graph asset in the Project window.", "OK");
                return;
            }

            var selPath = AssetDatabase.GetAssetPath(sel);
            if (string.IsNullOrEmpty(selPath))
            {
                EditorUtility.DisplayDialog("Export Graph", "Couldn’t resolve asset path for selection.", "OK");
                return;
            }

            // If user selected a DialogueBase by mistake, try the sibling .fpdlg
            string graphPath = selPath.EndsWith(".fpdlg")
                ? selPath
                : Path.Combine(Path.GetDirectoryName(selPath) ?? "Assets",
                               Path.GetFileNameWithoutExtension(selPath) + ".fpdlg");

            var graph = GraphDatabase.LoadGraph<FPDialogueGraph>(graphPath);
            if (graph == null)
            {
                EditorUtility.DisplayDialog("Export Graph",
                    "Couldn’t load FPDialogueGraph. Select the .fpdlg asset (graph file) and try again.", "OK");
                return;
            }

            // Resolve DialogueBase via the GUID stamped on the graph; fallback to manual pick
            var db = DialogueAssetUtil.LoadDialogueBaseByGuid(graph.dialogueBaseGuid);
            if (db == null)
            {
                var pick = EditorUtility.OpenFilePanel("Select DialogueBase asset", Application.dataPath, "asset");
                if (!string.IsNullOrEmpty(pick))
                {
                    var rel = "Assets" + pick.Substring(Application.dataPath.Length);
                    db = AssetDatabase.LoadAssetAtPath<DialogueBase>(rel);
                }
            }

            if (db == null)
            {
                EditorUtility.DisplayDialog("Export Graph", "Could not resolve DialogueBase for export.", "OK");
                return;
            }

            FPDialogueGraphIO.ExportToSOs(graph, db);
            EditorUtility.DisplayDialog("Export Graph", "Export complete.", "OK");
        }
        // Validator so the menu is only enabled when an .fpdlg is selected
        [MenuItem(CreatePath + "Export Graph → DialogueBase", validate = true)]
        private static bool ExportGraphToDialogueBase_Validate()
        {
            var sel = Selection.activeObject;
            if (!sel) return false;
            var path = AssetDatabase.GetAssetPath(sel);
            return !string.IsNullOrEmpty(path) && path.EndsWith(".fpdlg");
        }
    }
}
