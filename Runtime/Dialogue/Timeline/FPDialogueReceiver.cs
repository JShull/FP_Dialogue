namespace FuzzPhyte.Dialogue
{
    using UnityEngine;
    using UnityEngine.Playables;
    
    public class FPDialogueReceiver:MonoBehaviour,INotificationReceiver
    {
        [Tooltip("Must have a component that is using the IDialogueDirectorActions interface")]
        public GameObject DirectorObjectRef;
        [Tooltip("Optional explicit target. If null, uses the track binding (RTDialogueDirector).")] 
        public IDialogueDirectorActions DialogueDirector;
        protected virtual void Awake()
        {
            DialogueDirector = DirectorObjectRef.GetComponent<IDialogueDirectorActions>();
            if (DialogueDirector == null)
            {
                Debug.LogError($"Missing a reference to a class that is using the IDialogueDirectorActions");
            }
            //if(director == null)
            //{
           //     director=this.GetComponent<RTDialogueDirector>();
           // }
        }
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            Debug.LogWarning($"Notified!");
            if (notification is not FPDialogueCommandMarker cmd) return;

            if (DialogueDirector==null) return;

            switch (cmd.command)
            {
                case FPDialogueCommand.StartConversation:
                    DialogueDirector.StartConversation();
                    //director.StartConversation();               // public API on your RTDialogueDirector
                    break;
                case FPDialogueCommand.NextConversation:
                    DialogueDirector.UserPromptNext();
                    //director.UserPromptNext();                  // advances through dialogue flow
                    break;
                case FPDialogueCommand.PreviousConversation:
                    DialogueDirector.UserPromptPrevious();
                    //director.UserPromptPrevious();
                    break;
                case FPDialogueCommand.TranslateConversation:
                    DialogueDirector.UserPromptTranslate();
                    //director.UserPromptTranslate();
                    break;
                case FPDialogueCommand.RespondIndex:
                    DialogueDirector.UserPromptResponse(Mathf.Max(0, cmd.responseIndex));
                    //director.UserPromptResponse(Mathf.Max(0, cmd.responseIndex));
                    break;
                case FPDialogueCommand.SwapRuntimeGraph:
                    if (cmd.graphRef)
                    {
                        DialogueDirector.NewDialogueAdded(cmd.graphRef);
                    }
                    //if (cmd.graphRef) director.NewDialogueAdded(cmd.graphRef);
                    break;
            }
        }
    }
}
