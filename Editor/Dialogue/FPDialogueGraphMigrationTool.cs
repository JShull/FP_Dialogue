namespace FuzzPhyte.Dialogue.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    internal static class FPDialogueGraphMigrationTool
    {
        private const string GraphExtension = ".fpdialogue";
        private const string LegacyGraphObjectScriptLine = "  m_Script: {fileID: 11500000, guid: 790b4d75d92f4b0984310a268dbd952f, type: 3}";
        private const string CurrentGraphObjectScriptLine = "  m_Script: {fileID: 12501, guid: 0000000000000000e000000000000000, type: 0}";
        private const string LegacyEditorClassIdentifier = "  m_EditorClassIdentifier: Unity.GraphToolkit.Editor::Unity.GraphToolkit.Editor.Implementation.GraphObjectImp";
        private const string CurrentEditorClassIdentifier = "  m_EditorClassIdentifier: UnityEditor.dll::Unity.GraphToolkit.Editor.Implementation.GraphObjectImp";
        private const string LegacyInternalAssembly = "asm: Unity.GraphToolkit.Internal.Editor";
        private const string LegacyEditorAssembly = "asm: Unity.GraphToolkit.Editor";
        private const string CurrentToolkitAssembly = "asm: UnityEditor.GraphToolkitModule";
        private const string LegacyAssemblyQualifiedEditor = ", Unity.GraphToolkit.Editor,";
        private const string LegacyAssemblyQualifiedInternal = ", Unity.GraphToolkit.Internal.Editor,";
        private const string CurrentAssemblyQualifiedToolkit = ", UnityEditor.GraphToolkitModule,";
        private const string BackupSuffix = ".legacy-backup";

        [MenuItem("FuzzPhyte/Dialogue/Migrate Selected FPDialogue Graphs")]
        private static void MigrateSelectedGraphs()
        {
            var assetPaths = CollectSelectedGraphAssetPaths();
            RunMigration(assetPaths, "selected");
        }

        [MenuItem("FuzzPhyte/Dialogue/Migrate Selected FPDialogue Graphs", true)]
        private static bool ValidateMigrateSelectedGraphs()
        {
            return CollectSelectedGraphAssetPaths().Count > 0;
        }

        [MenuItem("FuzzPhyte/Dialogue/Migrate All FPDialogue Graphs In Project")]
        private static void MigrateAllGraphs()
        {
            var assetPaths = CollectAllGraphAssetPaths();
            RunMigration(assetPaths, "project");
        }

        private static void RunMigration(IReadOnlyList<string> assetPaths, string scopeLabel)
        {
            if (assetPaths.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "FP Dialogue Migration",
                    $"No {GraphExtension} assets were found in the {scopeLabel} scope.",
                    "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "FP Dialogue Migration",
                    $"This will create a {BackupSuffix} backup next to each selected legacy graph, rewrite known Unity GraphToolkit identifiers, and reimport the asset.\n\nAssets found: {assetPaths.Count}",
                    "Migrate",
                    "Cancel"))
            {
                return;
            }

            var migratedCount = 0;
            var skippedCount = 0;
            var failedAssets = new List<string>();

            try
            {
                AssetDatabase.StartAssetEditing();

                for (var i = 0; i < assetPaths.Count; i++)
                {
                    var assetPath = assetPaths[i];
                    EditorUtility.DisplayProgressBar(
                        "FP Dialogue Migration",
                        $"Migrating {assetPath}",
                        (float)i / assetPaths.Count);

                    try
                    {
                        var result = MigrateGraphAsset(assetPath);
                        switch (result)
                        {
                            case MigrationResult.Migrated:
                                migratedCount++;
                                break;
                            case MigrationResult.Skipped:
                                skippedCount++;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        failedAssets.Add($"{assetPath}\n{ex.Message}");
                        Debug.LogException(ex);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }

            var message = $"Migrated: {migratedCount}\nSkipped: {skippedCount}\nFailed: {failedAssets.Count}";
            if (failedAssets.Count > 0)
            {
                message += "\n\nSee Console for exception details.";
                Debug.LogError($"FP Dialogue migration failures:\n{string.Join("\n\n", failedAssets)}");
            }

            EditorUtility.DisplayDialog("FP Dialogue Migration", message, "OK");
        }

        private static MigrationResult MigrateGraphAsset(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath) || !assetPath.EndsWith(GraphExtension, StringComparison.OrdinalIgnoreCase))
            {
                return MigrationResult.Skipped;
            }

            var fullPath = Path.GetFullPath(assetPath);
            if (!File.Exists(fullPath))
            {
                return MigrationResult.Skipped;
            }

            var originalText = File.ReadAllText(fullPath);
            if (string.IsNullOrWhiteSpace(originalText))
            {
                return MigrationResult.Skipped;
            }

            if (!LooksLikeLegacyGraphToolkitAsset(originalText))
            {
                return MigrationResult.Skipped;
            }

            var migratedText = RewriteLegacyGraphToolkitIdentifiers(originalText);
            if (string.Equals(originalText, migratedText, StringComparison.Ordinal))
            {
                return MigrationResult.Skipped;
            }

            var backupPath = fullPath + BackupSuffix;
            if (!File.Exists(backupPath))
            {
                File.WriteAllText(backupPath, originalText);
            }

            File.WriteAllText(fullPath, migratedText);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"Migrated FP Dialogue graph: {assetPath}");
            return MigrationResult.Migrated;
        }

        private static bool LooksLikeLegacyGraphToolkitAsset(string text)
        {
            return text.Contains(LegacyGraphObjectScriptLine, StringComparison.Ordinal) ||
                   text.Contains(LegacyEditorClassIdentifier, StringComparison.Ordinal) ||
                   text.Contains(LegacyInternalAssembly, StringComparison.Ordinal) ||
                   text.Contains(LegacyEditorAssembly, StringComparison.Ordinal) ||
                   text.Contains(LegacyAssemblyQualifiedEditor, StringComparison.Ordinal) ||
                   text.Contains(LegacyAssemblyQualifiedInternal, StringComparison.Ordinal);
        }

        private static string RewriteLegacyGraphToolkitIdentifiers(string text)
        {
            return text
                .Replace(LegacyGraphObjectScriptLine, CurrentGraphObjectScriptLine, StringComparison.Ordinal)
                .Replace(LegacyEditorClassIdentifier, CurrentEditorClassIdentifier, StringComparison.Ordinal)
                .Replace(LegacyInternalAssembly, CurrentToolkitAssembly, StringComparison.Ordinal)
                .Replace(LegacyEditorAssembly, CurrentToolkitAssembly, StringComparison.Ordinal)
                .Replace(LegacyAssemblyQualifiedInternal, CurrentAssemblyQualifiedToolkit, StringComparison.Ordinal)
                .Replace(LegacyAssemblyQualifiedEditor, CurrentAssemblyQualifiedToolkit, StringComparison.Ordinal);
        }

        private static List<string> CollectSelectedGraphAssetPaths()
        {
            var assetPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var guid in Selection.assetGUIDs)
            {
                var selectedPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrWhiteSpace(selectedPath))
                {
                    continue;
                }

                if (AssetDatabase.IsValidFolder(selectedPath))
                {
                    foreach (var graphPath in EnumerateGraphAssetPathsInFolder(selectedPath))
                    {
                        assetPaths.Add(graphPath);
                    }
                }
                else if (selectedPath.EndsWith(GraphExtension, StringComparison.OrdinalIgnoreCase))
                {
                    assetPaths.Add(selectedPath);
                }
            }

            return new List<string>(assetPaths);
        }

        private static List<string> CollectAllGraphAssetPaths()
        {
            return EnumerateGraphAssetPathsInFolder("Assets");
        }

        private static List<string> EnumerateGraphAssetPathsInFolder(string rootFolder)
        {
            var results = new List<string>();
            var rootFullPath = Path.GetFullPath(rootFolder);
            if (!Directory.Exists(rootFullPath))
            {
                return results;
            }

            foreach (var filePath in Directory.EnumerateFiles(rootFullPath, $"*{GraphExtension}", SearchOption.AllDirectories))
            {
                var normalizedPath = filePath.Replace('\\', '/');
                var assetsIndex = normalizedPath.IndexOf("/Assets/", StringComparison.OrdinalIgnoreCase);
                if (assetsIndex >= 0)
                {
                    results.Add(normalizedPath[(assetsIndex + 1)..]);
                }
                else if (normalizedPath.EndsWith(GraphExtension, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(normalizedPath);
                }
            }

            return results;
        }

        private enum MigrationResult
        {
            Skipped,
            Migrated
        }
    }
}
