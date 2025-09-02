namespace FuzzPhyte.Dialogue.Editor
{
    using FuzzPhyte.Utility;
    using System;
    using Unity.GraphToolkit.Editor;
    using UnityEngine;

    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    public class FPCombineNodes:Node
    {
        [SerializeField] protected int inputs = 2;
        [SerializeField] protected int portsDirtyTick;
        // What the ports were actually built with last time
        
        protected override void OnDefineOptions(IOptionDefinitionContext options)
        {
            options.AddOption<int>(nameof(inputs)).WithDisplayName("Inputs");
            options.AddOption<int>(nameof(portsDirtyTick)).ShowInInspectorOnly();
        }

        protected override void OnDefinePorts(IPortDefinitionContext ports)
        {
            var _ = portsDirtyTick;
            // Use applied value for stability (so drawing matches the last applied state)
            
            ports.AddInputPort<FPVisualNode>($"Option 1").Build();
            ports.AddInputPort<FPVisualNode>($"Option 2").Build();
            ports.AddOutputPort<FPVisualNode>("Out").Build();
        }
        

        public void TouchPorts() => portsDirtyTick++;
    }
}
