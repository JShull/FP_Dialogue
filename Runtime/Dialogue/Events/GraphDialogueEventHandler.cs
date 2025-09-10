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
    /// </summary>
    public sealed class GraphDialogueEventHandler : MonoBehaviour, IFPDontDestroy
    {
        /// Dont destroy setup 
        public bool DontDestroy { get { return dontDestroy; } set { dontDestroy = value; } }
        [SerializeField] private bool dontDestroy;

        /// <summary>
        /// Static event: subscribe/unsubscribe from code.
        /// </summary>
        public static event Action<GraphEventData> OnGraphDialogueEvent;

        [Header("Designer Hooks")]
        [Tooltip("Optional: route all raised events through this UnityEvent for inspector-based bindings.")]
        public GraphDialogueUnityEvent OnGraphEventUnity;

        [Header("Optional Defaults (auto-filled if empty)")]
        public string DefaultGraphId;
        public string DefaultConversationId;

        // Instance registry so you can safely call Instance.Raise(...) if you want
        private static GraphDialogueEventHandler _instance;
        public static GraphDialogueEventHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GraphDialogueEventHandler>();
                    if (_instance == null)
                    {
                        var go = new GameObject(nameof(GraphDialogueEventHandler));
                        _instance = go.AddComponent<GraphDialogueEventHandler>();
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
            if (string.IsNullOrEmpty(data.GraphId)) data.GraphId = DefaultGraphId;
            if (string.IsNullOrEmpty(data.ConversationId)) data.ConversationId = DefaultConversationId;
            if (data.Timestamp <= 0) data.Timestamp = Time.realtimeSinceStartup;

            // Instance UnityEvent (designer hooks)
            OnGraphEventUnity?.Invoke(data);

            // Static event (code hooks)
            OnGraphDialogueEvent?.Invoke(data);            
        }

        #region Helper APIS
        // --------------------------------
        // Strongly-typed helper raise APIs
        // --------------------------------

        public void RaiseDialogueSetup(RTFPNode ctxNode = null, object payload = null,
            string graphId = null, string conversationId = null)
            => Raise(Build(GraphDialogueEventType.DialogueSetup, ctxNode, payload, graphId, conversationId));

        public void RaiseDialogueStart(RTEntryNode entry, string selectedNext = null,
            IReadOnlyList<string> candidates = null, object payload = null,
            string graphId = null, string conversationId = null)
            => Raise(Build(GraphDialogueEventType.DialogueStart, entry, payload, graphId, conversationId,
                            selectedNext, candidates, entryNode: entry));

        public void RaiseDialogueNext(RTFPNode prev, RTFPNode next, string selectedNext = null,
            IReadOnlyList<string> candidates = null, object payload = null,
            string graphId = null, string conversationId = null)
            => Raise(Build(GraphDialogueEventType.DialogueNext, next, payload, graphId, conversationId,
                            selectedNext, candidates, previous: prev));

        public void RaiseDialoguePrevious(RTFPNode prev, RTFPNode current, object payload = null,
            string graphId = null, string conversationId = null)
            => Raise(Build(GraphDialogueEventType.DialoguePrevious, current, payload, graphId, conversationId,
                            previous: prev));

        public void RaiseDialogueUserResponse(RTResponseNode responseNode, int responseIndex,
            string responseId = null, string responseText = null, string selectedNext = null,
            IReadOnlyList<string> candidates = null, object payload = null,
            string graphId = null, string conversationId = null)
            => Raise(Build(GraphDialogueEventType.DialogueUserResponse, responseNode, payload, graphId, conversationId,
                            selectedNext, candidates, responseNode: responseNode,
                            userResponseIndex: responseIndex, userResponseId: responseId, userResponseText: responseText));

        public void RaiseDialogueEnd(RTExitNode exitNode, object payload = null,
            string graphId = null, string conversationId = null)
            => Raise(Build(GraphDialogueEventType.DialogueEnd, exitNode, payload, graphId, conversationId, exitNode: exitNode));

        private static GraphEventData Build(
            GraphDialogueEventType type,
            RTFPNode current,
            object payload,
            string graphId,
            string conversationId,
            string selectedNext = null,
            IReadOnlyList<string> candidates = null,
            RTFPNode previous = null,
            RTDialogueNode dialogueNode = null,
            RTResponseNode responseNode = null,
            RTCharacterNode characterNode = null,
            RTSinglePromptNode singlePrompt = null,
            RTEntryNode entryNode = null,
            RTExitNode exitNode = null,
            RTCombineNode combineNode = null,
            RTOnewayNode onewayNode = null,
            RTTalkNode talkNode = null,
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

                CurrentNode = current,
                PreviousNode = previous,

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
