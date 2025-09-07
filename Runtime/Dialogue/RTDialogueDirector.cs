using System.Collections.Generic;
using UnityEngine;

namespace FuzzPhyte.Dialogue
{
    /// <summary>
    /// Placeholder for managing data across our editor graph input and our runtime needs in Unity
    /// </summary>
    public class RTDialogueDirector : MonoBehaviour
    {
        [Header("Graph")]
        public RTFPDialogueGraph RuntimeGraph;

        protected Dictionary<System.Type, object> executors;

        protected void Awake()
        {
            executors = new Dictionary<System.Type, object>
            {
                {typeof(RTEntryNode),new EntryNodeExecutor()},
                {typeof(RTExitNode),new ExitNodeExecutor()},
                {typeof(RTOnewayNode),new OnewayExecutor() },
                {typeof(RTCombineNode), new CombineNodeExecutor()},
                {typeof(RTCharacterNode),new CharacterNodeExecutor() },
                {typeof(RTResponseNode),new ResponseNodeExecutor()},
                {typeof(RTSinglePromptNode),new SinglePromptNodeExecutor()},
                {typeof(RTDialogueNode), new DialogueNodeExecutor()},
                {typeof(RTTalkNode), new TalkNodeExecutor()}
            };
        }
        protected void Start()
        {
            if (RuntimeGraph == null)
            {
                Debug.LogError($"Missing a runtime graph reference!");
                return;
            }
            /// JOHN: we now walk the node graph based on our runtime nodes
            /// we use the executor class when it's time = this is based on node type
            /// in a full graph you would do this in a while loop (we can't for dialogue)
            /// we need to now manage how we are going to move through the system based on current node and then it's type
            /// we only need to move through the primary type of nodes

            SetupGraph();

        }
        /// <summary>
        /// add a new dialogue to the system
        /// </summary>
        /// <param name="newGraph"></param>
        public void NewDialogueAdded(RTFPDialogueGraph newGraph)
        {
            /// check for other requirements and/or if we need to do anything (don't interrupt our current flow!)
            
            RuntimeGraph = newGraph;
        }
        protected void SetupGraph()
        {
            var currentNode = RuntimeGraph.Nodes[0];
            while (currentNode != null)
            {
                if(!executors.TryGetValue(currentNode.GetType(),out var executor))
                {
                    Debug.LogError($"No Executor found for node type: {currentNode.GetType().Name}");
                    break;
                }
                
                if(currentNode is RTEntryNode entryNode)
                {
                    var entryExecutor = (IRTFPDialogueNodeExecutor<RTEntryNode>)executor;
                    entryExecutor.Execute(entryNode, this);
                    currentNode = entryNode.NextNodeIndices.Count > 0
                        ? RuntimeGraph.Nodes[entryNode.NextNodeIndices[0]]
                        : null;
                    /*
                     * JOHN the actual method 
                    foreach(var nextIndex in entryNode.NextNodeIndices)
                    {
                        var nextNode = RuntimeGraph.Nodes[nextIndex];
                    }
                    */
                }else if(currentNode is RTDialogueNode dialogueNode)
                {
                    var dialogueExecutor = (IRTFPDialogueNodeExecutor<RTDialogueNode>)executor;
                    dialogueExecutor.Execute(dialogueNode, this);
                    currentNode = dialogueNode.NextNodeIndices.Count > 0
                        ? RuntimeGraph.Nodes[dialogueNode.NextNodeIndices[0]]
                        : null;
                }
                else
                {
                    currentNode = null;
                }
            }
            //need to get to our first dialogue and/or prompt node type to "start" dialogue
        }
    }
}
