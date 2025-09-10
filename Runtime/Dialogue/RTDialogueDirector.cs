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
        /// <summary>
        /// Active Node our Director is "on"
        /// </summary>
        [SerializeField]protected RTFPNode currentNode;

        protected Dictionary<System.Type, object> executors;

        /// <summary>
        /// event setup
        /// </summary>
        [Header("Events")]
        [SerializeField] private GraphDialogueEventHandler eventHandler;
        [SerializeField] private string graphID; //optional right now
        [SerializeField] private string conversationID; //optinal right now
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
            if (!eventHandler) eventHandler = GraphDialogueEventHandler.Instance;
            /// graph setup
            if (RuntimeGraph == null)
            {
                Debug.LogError($"Missing a runtime graph reference!");
                return;
            }
            eventHandler.DefaultGraphId = graphID;
            eventHandler.DefaultConversationId = conversationID;
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
        /// <summary>
        /// Prototype: testing walking the graph and only looking at the first output
        /// </summary>
        protected void SetupGraph()
        {
            if (RuntimeGraph.MainEntryNode != null)
            {
                currentNode = RuntimeGraph.MainEntryNode;
            }
            else
            {
                currentNode = RuntimeGraph.Nodes[0];
                //find entry node instead of first in runtime graph index?
                for (int i = 0; i < RuntimeGraph.Nodes.Count; i++)
                {
                    var node = RuntimeGraph.Nodes[i];
                    if (node is RTEntryNode)
                    {
                        currentNode = (RTEntryNode)node;
                        break;
                    }
                }
            }
            eventHandler.RaiseDialogueSetup();
            if (currentNode != null)
            {
                eventHandler.RaiseDialogueStart(currentNode as RTEntryNode);
            }
            else
            {
                Debug.LogError($"Current Node == null");
            }

                AdvanceUntilInteractive();
           
            /// actually walk the graph based on setup requirements, pull in data and continue until
            /// we are at a dialogue or a prompt
            /*
            if (currentNode is RTEntryNode anEntryNode) 
            {
                if (!executors.TryGetValue(currentNode.GetType(), out var executor))
                {
                    Debug.LogError($"No Executor found for node type: {currentNode.GetType().Name}");
                }
                //check next node?
                var entryExecutor = (IRTFPDialogueNodeExecutor<RTEntryNode>)executor;
                entryExecutor.Execute(anEntryNode, this);
                currentNode = anEntryNode.NextNodeIndices.Count > 0
                    ? RuntimeGraph.Nodes[currentNode.NextNodeIndices[0]]
                    : null;
            }
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
                    
                }else if(currentNode is RTDialogueNode dialogueNode)
                {
                    var dialogueExecutor = (IRTFPDialogueNodeExecutor<RTDialogueNode>)executor;
                    dialogueExecutor.Execute(dialogueNode, this);
                    currentNode = dialogueNode.NextNodeIndices.Count > 0
                        ? RuntimeGraph.Nodes[dialogueNode.NextNodeIndices[0]]
                        : null;
                }else if(currentNode is RTResponseNode responseNode)
                {
                    var responseExecutor = (IRTFPDialogueNodeExecutor<RTResponseNode>)executor;
                    responseExecutor.Execute(responseNode, this);
                    currentNode = responseNode.NextNodeIndices.Count > 0
                        ? RuntimeGraph.Nodes[responseNode.NextNodeIndices[0]]
                        : null;
                }else if(currentNode is RTCombineNode combineNode)
                {
                    var combineExecutor = (IRTFPDialogueNodeExecutor<RTCombineNode>)executor;
                    combineExecutor.Execute(combineNode, this);
                    currentNode = combineNode.NextNodeIndices.Count > 0
                        ? RuntimeGraph.Nodes[combineNode.NextNodeIndices[0]]
                        : null;
                }else
                {
                    currentNode = null;
                }
            }
            */
            //need to get to our first dialogue and/or prompt node type to "start" dialogue
        }
        
        protected void AdvanceUntilInteractive()
        {
            while (currentNode != null)
            {
                if (!executors.TryGetValue(currentNode.GetType(), out var executor))
                {
                    Debug.LogError($"No Executor found for node type: {currentNode.GetType().Name}");
                    return;
                }
                // If we hit an interactive node, STOP (do not Execute here).
                if (currentNode is RTDialogueNode || currentNode is RTResponseNode)
                {
                    // Let UI/mediator handle it now.
                    return;
                }

                // Exit ends traversal immediately after executing.
                if (currentNode is RTExitNode exitNode)
                {
                    var exitExec = (IRTFPDialogueNodeExecutor<RTExitNode>)executor;
                    exitExec.Execute(exitNode, this);
                    currentNode = null;
                    return;
                }

                // Auto-execute everything else, then advance.
                if (currentNode is RTEntryNode entryNode)
                {
                    var ex = (IRTFPDialogueNodeExecutor<RTEntryNode>)executor;
                    ex.Execute(entryNode, this);
                    currentNode = FirstNext(entryNode);
                }
                else if (currentNode is RTOnewayNode onewayNode)
                {
                    var ex = (IRTFPDialogueNodeExecutor<RTOnewayNode>)executor;
                    ex.Execute(onewayNode, this);
                    currentNode = FirstNext(onewayNode);
                }
                else if (currentNode is RTCombineNode combineNode)
                {
                    var ex = (IRTFPDialogueNodeExecutor<RTCombineNode>)executor;
                    ex.Execute(combineNode, this);
                    currentNode = FirstNext(combineNode);
                }
                else if (currentNode is RTCharacterNode charNode)
                {
                    var ex = (IRTFPDialogueNodeExecutor<RTCharacterNode>)executor;
                    ex.Execute(charNode, this);
                    currentNode = FirstNext(charNode);
                }
                else if (currentNode is RTSinglePromptNode singlePromptNode)
                {
                    var ex = (IRTFPDialogueNodeExecutor<RTSinglePromptNode>)executor;
                    ex.Execute(singlePromptNode, this);
                    currentNode = FirstNext(singlePromptNode);
                }
                else if (currentNode is RTTalkNode talkNode)
                {
                    var ex = (IRTFPDialogueNodeExecutor<RTTalkNode>)executor;
                    ex.Execute(talkNode, this);
                    currentNode = FirstNext(talkNode);
                }
                else
                {
                    Debug.LogWarning($"Unrecognized node type {currentNode.GetType().Name}; stopping.");
                    return;
                }
            }
        }
        /// <summary>
        /// Prefer NextNodeIndices. If empty, fall back to first connected out port target by Index string.
        /// </summary>
        private RTFPNode FirstNext(RTFPNode node)
        {
            // Primary: integer indices
            if (node.NextNodeIndices != null && node.NextNodeIndices.Count > 0)
            {
                return RuntimeGraph.Nodes[node.NextNodeIndices[0]];
            }

            // Fallback: port wiring by string NodeIndex
            if (node.outNodeIndices != null)
            {
                for (int p = 0; p < node.outNodeIndices.Length; p++)
                {
                    var cn = node.outNodeIndices[p].ConnectedNodes;
                    if (cn == null) continue;
                    for (int c = 0; c < cn.Length; c++)
                    {
                        var targetIndexString = cn[c].NodeIndex;
                        var found = FindByIndexString(targetIndexString);
                        if (found != null) return found;
                    }
                }
            }
            return null;
        }
        private RTFPNode FindByIndexString(string indexString)
        {
            if (string.IsNullOrEmpty(indexString)) return null;
            // Linear scan; if you keep a Dictionary<string, RTFPNode> map, swap it in here.
            for (int i = 0; i < RuntimeGraph.Nodes.Count; i++)
            {
                if (RuntimeGraph.Nodes[i].Index == indexString) return RuntimeGraph.Nodes[i];
            }
            return null;
        }
        protected void ProcessNode()
        {

        }
    }
}
