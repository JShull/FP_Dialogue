namespace FuzzPhyte.Dialogue.Editor
{
    using System;
    using UnityEngine;
    using Unity.GraphToolkit.Editor;

    [Graph("fpchar", GraphOptions.SupportsSubgraphs)]
    public class FPCharacterGraph:Graph
    {
        [SerializeField] public string characterGuid; //FP_Character asset GUID

        public override void OnGraphChanged(GraphLogger graphLogger)
        {
            base.OnGraphChanged(graphLogger);
        }
    }
    [UseWithGraph(typeof(FPCharacterGraph))]
    [Serializable]
    public class CharacterEntryNode : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext ports)
        {
            ports.AddOutputPort<DialogueFlow>("Enter").Build();
        }
    }
    [UseWithGraph(typeof(FPCharacterGraph))]
    [Serializable]
    public class CharacterExitNode : Node
    {
        [SerializeField] public string exitLabel = "Done"; // Shown in subgraph-call node as an output

        protected override void OnDefineOptions(IOptionDefinitionContext options)
        {
            options.AddOption<string>(nameof(exitLabel)).WithDisplayName("Exit Label");
        }

        protected override void OnDefinePorts(IPortDefinitionContext ports)
        {
            ports.AddInputPort<DialogueFlow>("In").Build();
        }
    }
}
