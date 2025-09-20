namespace FuzzPhyte.Dialogue
{
    using UnityEngine;
    using UnityEngine.Playables;
    
    public class FPDialogueReceiver:MonoBehaviour,INotificationReceiver
    {
        [Tooltip("Optional explicit target. If null, uses the track binding (RTDialogueDirector).")]
        public RTDialogueDirector director;

        protected virtual void Awake()
        {
            if(director == null)
            {
                director=this.GetComponent<RTDialogueDirector>();
            }
        }
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            Debug.LogWarning($"Notified!");
            if (notification is not FPDialogueCommandMarker cmd) return;

            if (!director) return;

            switch (cmd.command)
            {
                case FPDialogueCommand.StartConversation:
                    director.StartConversation();               // public API on your RTDialogueDirector
                    break;
                case FPDialogueCommand.NextConversation:
                    director.UserPromptNext();                  // advances through dialogue flow
                    break;
                case FPDialogueCommand.PreviousConversation:
                    director.UserPromptPrevious();
                    break;
                case FPDialogueCommand.TranslateConversation:
                    director.UserPromptTranslate();
                    break;
                case FPDialogueCommand.RespondIndex:
                    director.UserPromptResponse(Mathf.Max(0, cmd.responseIndex));
                    break;
                case FPDialogueCommand.SwapRuntimeGraph:
                    if (cmd.graphRef) director.NewDialogueAdded(cmd.graphRef);
                    break;
            }
        }
    }
}
