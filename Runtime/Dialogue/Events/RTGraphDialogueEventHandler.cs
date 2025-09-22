namespace FuzzPhyte.Dialogue
{
    using System.Collections.Generic;
    using UnityEngine.Events;
    using UnityEngine;
    using System;
    using FuzzPhyte.Utility;

    /// <summary>
    /// Central runtime event hub. 
    /// Static C# event for global code subscriptions.
    /// Instance UnityEvent for scene-level hooks.
    /// Transports information between the graph dialogue system controls "who cares to listen?"
    /// </summary>
    public sealed class RTGraphDialogueEventHandler : MonoBehaviour, IFPDontDestroy
    {
        /// Dont destroy setup 
        public bool DontDestroy { get { return dontDestroy; } set { dontDestroy = value; } }
        [SerializeField] private bool dontDestroy;

        /// <summary>
        /// Static event: subscribe/unsubscribe from code.
        /// </summary>
        public event Action<GraphEventData> OnGraphDialogueEvent;

        [Header("Designer Hooks")]
        [Tooltip("Optional: route all raised events through this UnityEvent for inspector-based bindings.")]
        public GraphDialogueUnityEvent OnGraphEventUnity;

        [Header("Optional Defaults (auto-filled if empty)")]
        private static RTGraphDialogueEventHandler _instance;
        public static RTGraphDialogueEventHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<RTGraphDialogueEventHandler>();
                    if (_instance == null)
                    {
                        var go = new GameObject(nameof(RTGraphDialogueEventHandler));
                        _instance = go.AddComponent<RTGraphDialogueEventHandler>();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// keeping track of things
        /// </summary>
        private static readonly List<string> _scratchNexts = new List<string>(8);

        public void Awake()
        {
            if (_instance != null && _instance != this)
            {
                // If you drop multiple handlers in a scene, keep the first.
                Destroy(gameObject);
                return;
            }
            _instance = this;
            if (DontDestroy)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// Core Event Raise Function - use helper APIs to access it
        /// </summary>
        /// <param name="data"></param>
        private void Raise(GraphEventData data)
        {
            // Backfill helpful fields
            if (data.GraphId == string.Empty)
            {
                Debug.LogError($"Missing graphID");
                return;
            }
            if(data.ConversationId== string.Empty)
            {
                Debug.LogError($"Missing Conversation ID");
                return;
            }
            if (data.Timestamp <= 0) data.Timestamp = Time.realtimeSinceStartup;

            // Instance UnityEvent (designer hooks)
            OnGraphEventUnity?.Invoke(data);

            // Static event (code hooks)
            OnGraphDialogueEvent?.Invoke(data);            
        }

        #region Helper APIS
        public void RaiseDialogueSetup(string graphID, string convoID,RTFPNode ctxNode = null, object payload = null)
            => Raise(Build(
                graphId:graphID,
                conversationId:convoID,
                GraphDialogueEventType.DialogueSetup, ctxNode, payload));

        public void RaiseDialogueStart(string graphID, string ConvoID, RTEntryNode entry, string selectedNext = null,
            IReadOnlyList<string> candidates = null, object payload = null)
            => Raise(Build(
                graphId:graphID, 
                conversationId:ConvoID, 
                GraphDialogueEventType.DialogueStart, 
                entry, payload,selectedNext, candidates, entryNode: entry));

        /// <summary>
        /// Raise a dialogue event, could be next or current
        /// </summary>
        /// <param name="prev"></param>
        /// <param name="next"></param>
        /// <param name="selectedNext"></param>
        /// <param name="candidates"></param>
        /// <param name="payload"></param>
        /// <param name="graphId"></param>
        /// <param name="conversationId"></param>
        public void RaiseDialogueNext(string graphID, string convoID,RTFPNode prev, RTDialogueNode next, RTFPNode outNextNode, float addedDelay =0,string selectedNext = null,
            IReadOnlyList<string> candidates = null, object payload = null)
            => Raise(Build(
                graphId:graphID,
                conversationId: convoID, 
                GraphDialogueEventType.DialogueUserNext, 
                next, 
                payload,
                selectedNext, 
                candidates,
                userDelayTime:addedDelay,
                next: outNextNode,
                dialogueNode:next,
                previous: prev));
        public void RaiseResponseNext(string graphID, string convoID,RTFPNode prev, RTResponseNode next, RTFPNode outNextNode,float addedDelay = 0,string selectedNext = null,
            IReadOnlyList<string> candidates = null, object payload = null)
            => Raise(Build(
                graphId: graphID,
                conversationId: convoID,
                GraphDialogueEventType.DialogueUserResponseNext, 
                next, 
                payload,
                selectedNext, 
                candidates,
                userDelayTime:addedDelay,
                next: outNextNode,
                responseNode:next,
                previous: prev));

        public void RaiseDialoguePrevious(string graphID, string convoID,RTFPNode prev, RTDialogueNode current, RTFPNode outNextNode,float addedDelay=0,object payload = null)
            => Raise(Build(
                graphId: graphID,
                conversationId: convoID,
                GraphDialogueEventType.DialogueUserPrevious, 
                current, 
                payload,
                userDelayTime:addedDelay,
                next: outNextNode,
                dialogueNode:current,
                previous: prev));
        public void RaiseResponsePrevious(string graphID, string convoID, RTFPNode prev, RTResponseNode current, RTFPNode outNextNode,float addedDelay=0,object payload = null)
            => Raise(Build(
                graphId: graphID,
                conversationId: convoID,
                GraphDialogueEventType.DialogueUserResponsePrevious, 
                current, 
                payload,
                userDelayTime:addedDelay,
                next: outNextNode,
                responseNode: current,
                previous: prev));
        public void RaiseDialogueUserResponseCollected(string graphID, string convoID,RTResponseNode responseNode, int responseIndex,
            string responseId = null, string responseText = null, string selectedNext = null,
            IReadOnlyList<string> candidates = null, object payload = null)
            => Raise(Build(
                graphId: graphID,
                conversationId: convoID, 
                GraphDialogueEventType.DialogueUserResponseCollected, 
                responseNode, 
                payload, 
                selectedNext, 
                candidates,
                responseNode: responseNode,
                userResponseIndex: responseIndex, 
                userResponseId: responseId, 
                userResponseText: responseText));
        public void RaiseDialogueUserTranslate(string graphID, string convoID, RTFPNode prev, RTDialogueNode current, RTFPNode outNextNode,object payload = null)
            => Raise(Build(
                graphId: graphID,
                conversationId: convoID, 
                GraphDialogueEventType.DialogueUserTranslate, 
                current, 
                payload,
                next: outNextNode,
                dialogueNode:current,
                previous: prev));
        public void RaiseResponseUserTranslate(string graphID, string convoID,
            RTFPNode prev, RTResponseNode current, RTFPNode outNextNode,object payload = null,
            string graphId = null, string conversationId = null)
            => Raise(Build(
                graphId: graphID,
                conversationId: convoID,
                GraphDialogueEventType.DialogueUserTranslate, 
                current, 
                payload, 
                next: outNextNode,
                responseNode:current,
                previous: prev));
        public void RaiseDialogueEnd(string graphID, string convoID,RTExitNode exitNode, object payload = null)
            => Raise(Build(
                graphId: graphID,
                conversationId: convoID,
                GraphDialogueEventType.DialogueEnd, exitNode, payload, exitNode: exitNode));

        private static GraphEventData Build(
            string graphId,
            string conversationId,
            GraphDialogueEventType type,
            RTFPNode current,
            object payload,
            string selectedNext = null,
            IReadOnlyList<string> candidates = null,
            RTFPNode previous = null,
            RTFPNode next = null,
            RTDialogueNode dialogueNode = null,
            RTResponseNode responseNode = null,
            RTCharacterNode characterNode = null,
            RTSinglePromptNode singlePrompt = null,
            RTEntryNode entryNode = null,
            RTExitNode exitNode = null,
            RTCombineNode combineNode = null,
            RTOnewayNode onewayNode = null,
            RTTalkNode talkNode = null,
            float userDelayTime=0,
            int userResponseIndex = -1,
            string userResponseId = null,
            string userResponseText = null
        )
        {
            // If specific typed ref is null, try to auto-cast from 'current'
            if (current != null)
            {
                dialogueNode ??= current as RTDialogueNode;
                responseNode ??= current as RTResponseNode;
                characterNode ??= current as RTCharacterNode;
                singlePrompt ??= current as RTSinglePromptNode;
                entryNode ??= current as RTEntryNode;
                exitNode ??= current as RTExitNode;
                combineNode ??= current as RTCombineNode;
                onewayNode ??= current as RTOnewayNode;
                talkNode ??= current as RTTalkNode;
            }

            // Pull candidate next node indices from the current node, if not provided
            IReadOnlyList<string> nexts = candidates;
            if (nexts == null && current != null && current.outNodeIndices != null && current.outNodeIndices.Length > 0)
            {
                // Flatten all out ports -> connected nodes -> NodeIndex
                _scratchNexts.Clear();
                foreach (var port in current.outNodeIndices)
                {
                    if (port.ConnectedNodes == null) continue;
                    foreach (var cn in port.ConnectedNodes)
                    {
                        if (!string.IsNullOrEmpty(cn.NodeIndex))
                            _scratchNexts.Add(cn.NodeIndex);
                    }
                }
                nexts = _scratchNexts.ToArray();
            }

            return new GraphEventData
            {
                EventType = type,
                GraphId = graphId,
                ConversationId = conversationId,
                Timestamp = Time.realtimeSinceStartup,
                UserDelayPaddedTime = userDelayTime,
                CurrentNode = current,
                PreviousNode = previous,
                NextNode = next,
                DialogueNode = dialogueNode,
                ResponseNode = responseNode,
                CharacterNode = characterNode,
                SinglePromptNode = singlePrompt,
                EntryNode = entryNode,
                ExitNode = exitNode,
                CombineNode = combineNode,
                OnewayNode = onewayNode,
                TalkNode = talkNode,

                CandidateNextNodeIndices = nexts,
                SelectedNextNodeIndex = selectedNext,

                UserResponsePromptIndex = userResponseIndex,
                UserResponseId = userResponseId,
                UserResponseText = userResponseText,

                Payload = payload
            };
        }
        #endregion
    }
}
