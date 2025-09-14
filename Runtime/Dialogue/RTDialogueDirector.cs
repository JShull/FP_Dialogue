namespace FuzzPhyte.Dialogue
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Placeholder for managing data across our editor graph input and our runtime needs in Unity
    /// Director: traverse the graph, emit and trigger events = "What node is next?"
    /// </summary>
    public class RTDialogueDirector : MonoBehaviour
    {
        [Header("Graph")]
        public RTFPDialogueGraph RuntimeGraph;
        public Stack<string> ProgressionPath = new();
        /// <summary>
        /// Active Node our Director is "on"
        /// </summary>
        [SerializeField] protected RTFPNode currentNode;

        protected Dictionary<System.Type, object> executors;

        /// <summary>
        /// event setup
        /// </summary>
        [Header("Events")]
        [SerializeField] private RTGraphDialogueEventHandler eventHandler;
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
            if (!eventHandler) eventHandler = RTGraphDialogueEventHandler.Instance;
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
            ProgressionPath.Clear();
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
                //push current node on the stack
                ProgressionPath.Push(currentNode.Index);
                if (currentNode is RTDialogueNode || currentNode is RTResponseNode)
                {
                    var previousNode = FirstPrevious(currentNode);
                    if (currentNode is RTDialogueNode)
                    {
                        //var talkExec = (IRTFPDialogueNodeExecutor<RTDialogueNode>)executor;
                        //talkExec.Execute(currentNode as RTDialogueNode, this);
                        eventHandler.RaiseDialogueNext(previousNode, currentNode);
                    }
                    else
                    {
                        //var talkResponseExec = (IRTFPDialogueNodeExecutor<RTResponseNode>)executor;
                        //talkResponseExec.Execute(currentNode as RTResponseNode, this);
                        eventHandler.RaiseDialogueUserResponseDisplay(currentNode as RTResponseNode, previousNode);
                    }
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
        /// Need To Test This Sunday JOHN
        /// </summary>
        protected void PreviousUntilInteractive()
        {
            if (currentNode == null)
            {
                Debug.LogWarning($"Current node is null; cannot go to previous.");
                return;
            }

            var previousNode = FirstPrevious(currentNode);
            string currentIndex = string.Empty;
            if (previousNode != null && ProgressionPath.Count > 0)
            {
                currentIndex = ProgressionPath.Pop(); //removes current node
            }

            while (previousNode != null)
            {
                if (!executors.TryGetValue(previousNode.GetType(), out var executor))
                {
                    Debug.LogError($"No Executor found for node type: {previousNode.GetType().Name}");
                    return;
                }
                if (previousNode is RTDialogueNode || previousNode is RTResponseNode)
                {
                    ProgressionPath.Pop(); //removes it
                    currentNode = previousNode;
                    previousNode = FirstPrevious(currentNode);
                    eventHandler.RaiseDialoguePrevious(currentNode, previousNode);
                    return;
                }
                if (previousNode is RTEntryNode)
                {
                    Debug.LogWarning($"Reached Entry node; cannot go previous.");
                    ProgressionPath.Push(currentIndex); //put current back on the stack
                    return;
                }
                if (previousNode is RTOnewayNode oneWay)
                {
                    Debug.LogWarning($"Cannot go previous past Oneway # {oneWay.Index} node.");
                    ProgressionPath.Push(currentIndex); //put current back on the stack
                    return;
                }
                if (previousNode is RTCombineNode combineNode)
                {
                    // we pop this and then continue to pop until we find a dialogue or response node
                    ProgressionPath.Pop(); //removes combined node
                    if (ProgressionPath.Count > 0)
                    {
                        previousNode = FindByIndexString(ProgressionPath.Pop());
                    }
                }
            }
        }
        /// <summary>
        /// Prefer NextNodeIndices. If empty, fall back to first connected out port target by Index string.
        /// </summary>
        protected RTFPNode FirstNext(RTFPNode node)
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
        protected RTFPNode FirstPrevious(RTFPNode node)
        {
            // Fallback: port wiring by string NodeIndex
            if (node.inNodeIndices != null)
            {
                for (int p = 0; p < node.inNodeIndices.Length; p++)
                {
                    var cn = node.inNodeIndices[p].ConnectedNodes;
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
        protected RTFPNode FindByIndexString(string indexString)
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
        #region Public Methods

        public void UserPromptResponse(int promptIndex)
        {
            if (currentNode is RTResponseNode responseNode)
            {
                // this is informing the system of the user response (raise the event and then update the graph)
                eventHandler.RaiseDialogueUserResponse(responseNode, promptIndex);

                //progress the graph forward
                currentNode = FirstNext(currentNode);
                //we need to go to the next dialogue node if we can
                AdvanceUntilInteractive();
            }
            else
            {
                Debug.LogWarning($"Current node is not a ResponseNode; cannot accept user prompt response.");
            }
        }
        [ContextMenu("Dialogue User Input Previous")]
        public void UserPromptPrevious()
        {
            //we need to go back to the previous dialogue node if we can
            PreviousUntilInteractive();
        }
        /// <summary>
        /// Public UI/Method to repeat
        /// </summary>
        [ContextMenu("Dialogue User Input Repeat")]
        public void UserPromptRepeat()
        {
            var previousNode = FirstPrevious(currentNode);
            if (currentNode is RTDialogueNode dialogueNode)
            {
                eventHandler.RaiseDialogueNext(previousNode, currentNode);
            }
            else if (currentNode is RTResponseNode responseNode)
            {
                eventHandler.RaiseDialogueUserResponseDisplay(responseNode, previousNode);
            }
            else
            {
                Debug.LogWarning($"Current node is not a DialogueNode or ResponseNode; cannot repeat.");
            }
        }
        /// <summary>
        /// Public UI/Method to go to the next dialogue node
        /// </summary>
        [ContextMenu("Dialogue User Input Next")]
        public void UserPromptNext()
        {
            //we need to go to the next dialogue node if we can
            currentNode = FirstNext(currentNode);
            AdvanceUntilInteractive();
        }
        /// <summary>
        /// add a new dialogue to the system
        /// </summary>
        /// <param name="newGraph"></param>
        public void NewDialogueAdded(RTFPDialogueGraph newGraph)
        {
            /// check for other requirements and/or if we need to do anything (don't interrupt our current flow!)
            RuntimeGraph = newGraph;
            //clean up?
            SetupGraph();
        }
        #endregion
    }
}
