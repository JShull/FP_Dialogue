namespace FuzzPhyte.Dialogue.Editor
{
    using FuzzPhyte.Utility;
    using System;
    using Unity.GraphToolkit.Editor;
    using UnityEngine;

    /// <summary>
    /// Accepts two incoming ports and no doesn't care, just goes to one
    /// for tracing purposes
    /// </summary>
    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    public class FPCombineNode:FPVisualNode
    {
        public override void SetupIndex(string passedName)
        {
            this.name = passedName;
        }
        [SerializeField] protected int inputs = 2;
        [SerializeField] protected int portsDirtyTick;
        // What the ports were actually built with last time
        
        protected override void OnDefineOptions(IOptionDefinitionContext options)
        {
            options.AddOption<int>(nameof(inputs)).WithDisplayName("Number of Inputs");
            options.AddOption<int>(nameof(portsDirtyTick)).ShowInInspectorOnly();
        }

        protected override void OnDefinePorts(IPortDefinitionContext ports)
        {
            // Use applied value for stability (so drawing matches the last applied state)
            
            ports.AddInputPort<FPVisualNode>(FPDialogueGraphValidation.PORT_COMBINE_OPONE)
                .WithDisplayName("Option 1")
                .Build();
            ports.AddInputPort<FPVisualNode>(FPDialogueGraphValidation.PORT_COMBINE_OPTWO)
                .WithDisplayName("Option 2")
                .Build();
            AddOutputExecutionPorts(ports);
        }
    }
}
