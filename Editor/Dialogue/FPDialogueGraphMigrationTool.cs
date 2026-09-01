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
        private const string BackupSuffix = ".pre-6000.6-backup";

        [MenuItem("FuzzPhyte/Dialogue/Upgrade Selected FPDialogue Graphs to Unity 6000.6", priority = FuzzPhyte.Utility.FP_UtilityData.MENU_UTILITY_DIALOGUE + 20)]
        private static void MigrateSelectedGraphs()
        {
            var assetPaths = CollectSelectedGraphAssetPaths();
            RunMigration(assetPaths, "selected");
        }

        [MenuItem("FuzzPhyte/Dialogue/Upgrade Selected FPDialogue Graphs to Unity 6000.6", true, FuzzPhyte.Utility.FP_UtilityData.MENU_UTILITY_DIALOGUE + 20)]
        private static bool ValidateMigrateSelectedGraphs()
        {
            return CollectSelectedGraphAssetPaths().Count > 0;
        }

        [MenuItem("FuzzPhyte/Dialogue/Upgrade All FPDialogue Graphs to Unity 6000.6", priority = FuzzPhyte.Utility.FP_UtilityData.MENU_UTILITY_DIALOGUE + 21)]
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
                    "FP Dialogue Upgrade",
                    $"No {GraphExtension} assets were found in the {scopeLabel} scope.",
                    "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "FP Dialogue Upgrade",
                    $"This will create a {BackupSuffix} backup next to each graph, rewrite known legacy GraphToolkit identifiers, and reimport the asset for Unity 6000.6.\n\nAssets found: {assetPaths.Count}",
                    "Upgrade",
                    "Cancel"))
            {
                return;
            }

            var upgradedCount = 0;
            var skippedCount = 0;
            var failedAssets = new List<string>();

            try
            {
                AssetDatabase.StartAssetEditing();

                for (var i = 0; i < assetPaths.Count; i++)
                {
                    var assetPath = assetPaths[i];
                    EditorUtility.DisplayProgressBar(
                        "FP Dialogue Upgrade",
                        $"Upgrading {assetPath}",
                        (float)i / assetPaths.Count);

                    try
                    {
                        var result = MigrateGraphAsset(assetPath);
                        switch (result)
                        {
                            case MigrationResult.Migrated:
                                upgradedCount++;
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

            var message = $"Upgraded: {upgradedCount}\nSkipped: {skippedCount}\nFailed: {failedAssets.Count}";
            if (failedAssets.Count > 0)
            {
                message += "\n\nSee Console for exception details.";
                Debug.LogError($"FP Dialogue upgrade failures:\n{string.Join("\n\n", failedAssets)}");
            }

            EditorUtility.DisplayDialog("FP Dialogue Upgrade", message, "OK");
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

            var migratedText = RewriteLegacyGraphToolkitIdentifiers(originalText);
            var backupPath = fullPath + BackupSuffix;
            if (!File.Exists(backupPath))
            {
                File.WriteAllText(backupPath, originalText);
            }

            if (!string.Equals(originalText, migratedText, StringComparison.Ordinal))
            {
                File.WriteAllText(fullPath, migratedText);
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"Upgraded FP Dialogue graph for Unity 6000.6: {assetPath}");
            return MigrationResult.Migrated;
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
