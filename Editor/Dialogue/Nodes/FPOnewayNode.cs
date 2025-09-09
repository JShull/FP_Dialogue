namespace FuzzPhyte.Dialogue.Editor
{
    using FuzzPhyte.Utility;
    using System;
    using Unity.GraphToolkit.Editor;
    using UnityEngine;

    /// <summary>
    /// Lets the runtime system know we can't go backwards from this point
    /// utilized to restrict dialogue scenarios if needed
    /// </summary>
    [UseWithGraph(typeof(FPDialogueGraph))]
    [Serializable]
    internal class FPOnewayNode:FPVisualNode
    {
        public override void SetupIndex(string passedName)
        {
            this.name = passedName;
        }

        protected override void OnDefinePorts(IPortDefinitionContext ports)
        {
            ports.AddInputPort<FPVisualNode>(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)
                .WithDisplayName("Flow In")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
            ports.AddOutputPort<FPVisualNode>(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)
               .WithDisplayName("Flow Out")
               .WithConnectorUI(PortConnectorUI.Arrowhead)
               .Build();
        }
    }
}
