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
    internal class FPCombineNode:FPVisualNode
    {
        public override void SetupIndex(string passedName)
        {
            this.name = passedName;
        }
      
        protected override void OnDefineOptions(IOptionDefinitionContext options)
        {
            options.AddOption<int>(FPDialogueGraphValidation.PORT_NUMBER_OPTIONS)
                .WithDisplayName("Number of Inputs")
                .WithDefaultValue(2)
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext ports)
        {
            // Use applied value for stability (so drawing matches the last applied state)
           
            var portCountOption = GetNodeOptionByName(FPDialogueGraphValidation.PORT_NUMBER_OPTIONS);
            portCountOption.TryGetValue<int>(out var portCount);
            for(var i = 0; i < portCount; i++)
            {
                ports.AddInputPort<FPVisualNode>(FPDialogueGraphValidation.PORT_INDEX_OP+i.ToString())
                .WithDisplayName($"Option {i+1}")
                .Build();
            }
            ports.AddOutputPort<FPVisualNode>(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)
                .WithDisplayName("Flow Out")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();

            //OLD STUFF
            //ports.AddInputPort<FPVisualNode>(FPDialogueGraphValidation.PORT_COMBINE_OPONE)
            //    .WithDisplayName("Option 1")
            //    .Build();
            //ports.AddInputPort<FPVisualNode>(FPDialogueGraphValidation.PORT_COMBINE_OPTWO)
            //    .WithDisplayName("Option 2")
            //    .Build();
            
        }
    }
}
