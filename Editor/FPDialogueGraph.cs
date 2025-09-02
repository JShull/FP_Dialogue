namespace FuzzPhyte.Dialogue.Editor
{
    using System.Collections.Generic;
    using Unity.GraphToolkit.Editor;
    using UnityEngine;
    using System;
    using FuzzPhyte.Utility;

    // Control-flow phantom type for port compatibility
    [Serializable] public struct DialogueFlow { };
    [Graph(AssetExtension, GraphOptions.SupportsSubgraphs)]
    public class FPDialogueGraph:Graph
    {
        public const string AssetExtension = "fpdlg";
        // Link back to the DialogueBase asset this graph represents
        [SerializeField] public string dialogueBaseGuid;

        public override void OnGraphChanged(GraphLogger logger)
        {
            // 1) Run node actions triggered via options
            FPDialogueGraphCommands.ProcessNodeActions(this);

            // 2) Validation messages
            FPDialogueGraphValidation.Run(this, logger);
        }
    }
    public enum NodeAction
    {
        None = 0,
        AddResponse,
        RemoveSelected,
        MoveUp,
        MoveDown,
        LoadSelectedText,   
        ApplySelectedText, 
        RefreshPorts,
        PingBackingAssets
    }
    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    public class EntryNode : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext ports)
        {
            // Single "Start" output
            ports.AddOutputPort<DialogueFlow>("Start").Build();
        }
    }

    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    public class DialogueBlockNode : ContextNode
    {
        // In DialogueBlockNode
        [SerializeField] public int responsesCount; 

        public void SyncDerivedUIFields()
        {
            
        }
        
        [SerializeField] public string blockGuid; // GUID of the DialogueBlock SO
        [SerializeField] public string speaker;

        [SerializeField] public FP_Language language;
        [SerializeField, TextArea(2,4)] public string line;
        [SerializeField] public AudioClip clip;
        
        // Stable response slots (IDs persisted for round-trip)

        // “Option-backed” UI controls
        [SerializeField] public NodeAction action = NodeAction.None;

        // bump this to force OnDefinePorts to run
        [SerializeField] private int portsDirtyTick;

        protected override void OnDefineOptions(IOptionDefinitionContext options)
        {
            // Core line fields
            options.AddOption<string>(nameof(blockGuid)).WithDisplayName("Block GUID").ShowInInspectorOnly();
            options.AddOption<string>(nameof(speaker)).WithDisplayName("Speaker");
            options.AddOption<FP_Language>(nameof(language)).WithDisplayName("Speaker Language");
            options.AddOption<string>(nameof(line)).WithTooltip("Text to be said").WithDisplayName("Line");
            options.AddOption<AudioClip>(nameof(clip)).WithDisplayName("Audio Clip");
            // Response editing helpers
            options.AddOption<NodeAction>(nameof(action)).WithDisplayName("Action");

            // Visibility for sanity-checking
            options.AddOption<int>(nameof(responsesCount)).WithDisplayName("User Responses?");
            // Hidden tick to make port layout depend on state
            options.AddOption<int>(nameof(portsDirtyTick)).ShowInInspectorOnly();
        }

        protected override void OnDefinePorts(IPortDefinitionContext ports)
        {
            var _ = portsDirtyTick; // make port definition depend on tick

            ports.AddInputPort<DialogueFlow>("In").Build();

            // If there are no choices, expose a single "Next" port for linear flow.
            if (responsesCount>=0)
            {
                ports.AddOutputPort<DialogueFlow>("Next").Build();
                return;
            }
        }
        // Helper used by the command processor
        public void TouchPorts() => portsDirtyTick++;
    }

    [UseWithContext(typeof(DialogueBlockNode))]
    [Serializable]
    public class DialogueLanguageNode : BlockNode
    {
        [SerializeField] public FP_Language Translatedlanguage;
        [SerializeField] public string line;
        [SerializeField] public AudioClip clip;

        protected override void OnDefineOptions(IOptionDefinitionContext options)
        {
            // Core line fields
            options.AddOption<FP_Language>(nameof(Translatedlanguage)).WithDisplayName("Translated Language");
            options.AddOption<string>(nameof(line)).WithTooltip("Translated Text").WithDisplayName("Text:");
            options.AddOption<AudioClip>(nameof(clip)).WithDisplayName("Translated Clip");
            // Response editing helpers
        }
    }
}
