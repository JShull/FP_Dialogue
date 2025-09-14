namespace FuzzPhyte.Dialogue
{
    using System.Collections.Generic;
    using UnityEngine.Events;
    using UnityEngine;
    using System;
    /// <summary>
    /// All runtime dialogue flow event types.
    /// </summary>
    [Serializable]
    public enum GraphDialogueEventType
    {
        DialogueSetup,
        DialogueStart,
        DialogueNext,
        DialoguePrevious,
        DialogueUserRepeat,
        DialogueUserResponse,
        DialogueUserResponseDisplay,
        DialogueEnd
    }
    /// <summary>
    /// Listener contract for code-side subscribers.
    /// </summary>
    public interface IGraphDialogueEventListener
    {
        void OnGraphDialogueEvent(GraphEventData data);
    }

    /// <summary>
    /// UnityEvent wrapper for designer-side bindings.
    /// Note: Unity can't serialize interfaces, so we pass the struct directly.
    /// </summary>
    [Serializable]
    public class GraphDialogueUnityEvent : UnityEvent<GraphEventData> { }
    
    /// <summary>
    /// Main event data for the RTGraph System
    /// </summary>
    [Serializable]
    public struct GraphEventData
    {
        // Required
        public GraphDialogueEventType EventType;
        public string GraphId;            // Optional: your graph instance/asset id
        public string ConversationId;     // Optional: a specific run/session id
        public double Timestamp;          // Time.realtimeSinceStartup at raise time--might not need this

        // Node context
        public RTFPNode CurrentNode;      
        public RTFPNode PreviousNode; 

        // Common fast-access (null if not applicable)
        public RTDialogueNode DialogueNode;
        public RTResponseNode ResponseNode;
        public RTCharacterNode CharacterNode;
        public RTSinglePromptNode SinglePromptNode;
        public RTEntryNode EntryNode;
        public RTExitNode ExitNode;
        public RTCombineNode CombineNode;
        public RTOnewayNode OnewayNode;
        public RTTalkNode TalkNode;

        // Flow hints (optional)
        public IReadOnlyList<string> CandidateNextNodeIndices; // for UI / logic preview
        public string SelectedNextNodeIndex;                   // which path we chose (if applicable)

        // User input (for DialogueUserResponse)--dont think I will need any of this
        
        public int UserResponsePromptIndex;       // e.g., index in RTResponseNode.userIncomingPrompts
        public string UserResponseId;       // any custom id/string
        public string UserResponseText;     // raw user text (if your UX allows text answers)

        // Arbitrary attachable data (safe for cross-system)
        public object Payload;              // minimal boxing use; keep POCO/structs

        // Helpers
        public T As<T>() where T : class => Payload as T;

        public override string ToString()
        {
            string nodeIdx = CurrentNode != null ? CurrentNode.Index : "NULL";
            return $"[{EventType}] Graph:{GraphId} Conv:{ConversationId} Node:{nodeIdx} Time:{Timestamp:0.000}";
        }
    }
}
