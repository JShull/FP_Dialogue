namespace FuzzPhyte.Dialogue
{
    using UnityEngine;
    using UnityEngine.Playables;
    using UnityEngine.Timeline;
    public enum FPDialogueCommand
    {
        NA=0,
        StartConversation=1,
        NextConversation=2,
        PreviousConversation=3,
        TranslateConversation=4,
        RespondIndex,       // uses responseIndex
        SwapRuntimeGraph    // uses graphRef (optional)
    }
    [System.Serializable]
    public class FPDialogueCommandMarker:Marker, INotification
    {
        public FPDialogueCommand command = FPDialogueCommand.StartConversation;

        [Tooltip("For when you need a specific user response from the graph, this can align to that")]
        public int responseIndex = 0;

        [Tooltip("Optional runtime graph to swap-to")]
        public RTFPDialogueGraph graphRef;

        // Required by INotification
        public PropertyName id => new PropertyName(nameof(FPDialogueCommandMarker));
    }
}
