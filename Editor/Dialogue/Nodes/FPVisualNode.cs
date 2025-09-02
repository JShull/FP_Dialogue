namespace FuzzPhyte.Dialogue.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Unity.GraphToolkit.Editor;
    /// <summary>
    /// Base abstract class for all Dialogue Block Nodes
    /// </summary>
    [Serializable]
    public abstract class FPVisualNode:Node
    {
        
        public const string PORT_ACTOR = "CharacterPort";
        public const string TALK_PORT_NODE = "TalkPort";

        [SerializeField] public string name;

        /// <summary>
        /// Defines common input and output execution ports for all nodes tied to the Dialogue System.
        /// </summary>
        /// <param name="scope">The scope to define the node.</param>
        protected void AddInputExecutionPorts(IPortDefinitionContext context)
        {
            context.AddInputPort<FPVisualNode>(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)
                .WithDisplayName(string.Empty)
                .WithConnectorUI(PortConnectorUI.Arrowhead)
                .Build();
        }
        protected void AddOutputExecutionPorts(IPortDefinitionContext context)
        {
            context.AddOutputPort<FPVisualNode>(FPDialogueGraphValidation.MAIN_PORT_DEFAULT_NAME)
               .WithDisplayName(string.Empty)
               .WithConnectorUI(PortConnectorUI.Arrowhead)
               .Build();
        }
        protected void AddInputActorPort(IPortDefinitionContext context) 
        {
            context.AddInputPort(PORT_ACTOR)
                .WithDisplayName("Character:")
                .WithConnectorUI(PortConnectorUI.Circle)
                .Build();
        }
        protected void AddOutputActorPort(IPortDefinitionContext context) 
        {
            context.AddOutputPort(PORT_ACTOR)
              .WithConnectorUI(PortConnectorUI.Circle)
              .Build();
        }
    }
}
