namespace FuzzPhyte.Dialogue.Editor
{
    using System;
    using UnityEngine;
    using Unity.GraphToolkit.Editor;
    /// <summary>
    /// Base abstract class for all Dialogue Block Nodes
    /// </summary>
    [Serializable]
    public abstract class FPVisualNode:Node
    {

        [SerializeField] protected string name;
        public string Name { get { return name; } }
        public abstract void SetupIndex(string passedName);
        /// <summary>
        /// Defines common input and output execution ports for all nodes tied to the Dialogue System.
        /// </summary>
        /// <param name="scope">The scope to define the node.</param>
        
        protected void AddInputExecutionPorts(IPortDefinitionContext context)
        {
            context.AddInputPort<FPVisualNode>(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)
                .WithDisplayName("Flow In")
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
        protected void AddOutputExecutionPorts(IPortDefinitionContext context)
        {
            context.AddOutputPort<FPVisualNode>(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)
               .WithDisplayName("Flow Out")
               .WithConnectorUI(PortConnectorUI.Arrowhead)
               .Build();
        }
        protected void AddInputActorPort(IPortDefinitionContext context) 
        {
            context.AddInputPort<SetFPCharacterNode>(FPDialogueGraphValidation.PORT_ACTOR)
                .WithDisplayName("Character In:")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
        }
        protected void AddOutputActorPort(IPortDefinitionContext context) 
        {
            context.AddOutputPort<SetFPCharacterNode>(FPDialogueGraphValidation.PORT_ACTOR)
                .WithDisplayName("Character Out:")
              .WithConnectorUI(PortConnectorUI.Circle)
              .Build();
        }
    }
}
