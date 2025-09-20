
namespace FuzzPhyte.Dialogue.Editor
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.Timeline;
    using UnityEngine;
    using UnityEngine.Timeline;
    [CustomTimelineEditor(typeof(FPDialogueCommandMarker))]
    public class FPDialogueCommandMarkerEditor : MarkerEditor
    {
        static Color ColorFor(FPDialogueCommand cmd) => cmd switch
        {
            FPDialogueCommand.StartConversation => new Color(0.20f, 0.65f, 0.85f),
            FPDialogueCommand.NextConversation => new Color(0.20f, 0.75f, 0.30f),
            FPDialogueCommand.PreviousConversation => new Color(0.95f, 0.55f, 0.10f),
            FPDialogueCommand.TranslateConversation => new Color(0.60f, 0.40f, 0.85f),
            FPDialogueCommand.RespondIndex => new Color(0.95f, 0.25f, 0.55f),
            FPDialogueCommand.SwapRuntimeGraph => new Color(0.25f, 0.55f, 0.95f),
            _ => new Color(0.5f, 0.5f, 0.5f)
        };
        static Texture2D IconFor(FPDialogueCommand cmd)
        {
            string name = cmd switch
            {
                FPDialogueCommand.StartConversation => "PlayButton On",
                FPDialogueCommand.NextConversation => "d_forward",
                FPDialogueCommand.PreviousConversation => "d_back",
                FPDialogueCommand.TranslateConversation => "d_UnityEditor.InspectorWindow",
                FPDialogueCommand.RespondIndex => "d_FilterByType",
                FPDialogueCommand.SwapRuntimeGraph => "d_Refresh",
                _ => "d_Profiler.Timeline"
            };
            return (Texture2D)EditorGUIUtility.IconContent(name).image;
        }

        public override MarkerDrawOptions GetMarkerOptions(IMarker marker)
        {
            var opts = base.GetMarkerOptions(marker);
            if (marker is FPDialogueCommandMarker m)
            {
                opts.tooltip = m.command switch
                {
                    FPDialogueCommand.RespondIndex => $"Respond Index: {m.responseIndex}",
                    FPDialogueCommand.SwapRuntimeGraph => m.graphRef ? $"Swap Graph → {m.graphRef.name}" : "Swap Graph (none)",
                    _ => m.command.ToString()
                };
            }
            return opts;
        }

        public override void DrawOverlay(IMarker marker, MarkerUIStates uiState, MarkerOverlayRegion region)
        {
            if (marker is not FPDialogueCommandMarker m) return;
            var r = region.markerRegion;

            EditorGUI.DrawRect(r, ColorFor(m.command));

            var icon = IconFor(m.command);
            if (icon != null)
            {
                float size = Mathf.Min(r.height - 2f, 14f);
                var iconRect = new Rect(r.x + (r.width - size) * 0.5f, r.y + 2f, size, size);
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
            }

            var label = m.command == FPDialogueCommand.RespondIndex ? $"#{m.responseIndex}"
                        : m.command == FPDialogueCommand.SwapRuntimeGraph && m.graphRef ? m.graphRef.name
                        : m.command.ToString();

            var style = EditorStyles.miniBoldLabel;
            style.alignment = TextAnchor.LowerCenter;
            GUI.Label(new Rect(r.x, r.y, r.width, r.height - 1f), label, style);
        }
    }
#endif
}
