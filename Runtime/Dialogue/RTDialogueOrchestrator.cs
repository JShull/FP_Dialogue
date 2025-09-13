namespace FuzzPhyte.Dialogue
{
    using UnityEngine;

    /// <summary>
    /// our glue to the dialogue graph dialogue system
    /// answers the question of = "What needs to come together to interface with the conversation?"
    /// General Notes: only this class ever touches prefabs and scene objects. Mediator is in charge of confirming node info is valid. Director never knows about UI/UX presentation
    /// </summary>
    public class RTDialogueOrchestrator : MonoBehaviour
    {
        [SerializeField] protected RTGraphDialogueEventHandler handler;
        [SerializeField] protected RTDialogueMediator mediator;
        [SerializeField] protected RTExposedBinder binder;

        protected void Awake()
        {
            if (!handler) handler = RTGraphDialogueEventHandler.Instance;
            if (!mediator) mediator = GetComponent<RTDialogueMediator>();
            if (!binder) binder = GetComponent<RTExposedBinder>();

            if(handler == null || mediator == null || binder == null)
            {
                Debug.LogError($"Missing a critical reference!");
            }
        }

        protected void OnEnable()
        {
            if (handler != null) 
            {
                handler.OnGraphDialogueEvent += Handle;
            }
        }
        protected void OnDisable() 
        {
            if (handler != null)
            {
                handler.OnGraphDialogueEvent -= Handle;
            }
        }
        protected void Handle(GraphEventData data)
        {
            switch (data.EventType)
            {
                case GraphDialogueEventType.DialogueSetup:
                    // Populate binder or init transient state if needed
                    // need binder on setup?
                    break;

                case GraphDialogueEventType.DialogueStart:
                    // Drive any entry logic (e.g., play a timeline, fade-in, spawn an NPC, etc.)
                    if (data.EntryNode != null) 
                    { 
                        var startResponse = mediator.EvaluateEntryNode(data.EntryNode); 
                    }
                    //binder needed on dialogue starting? (entry node? = no)
                    break;

                case GraphDialogueEventType.DialogueNext:
                    // Evaluate the new node and act
                    RouteEvaluateNext(data.CurrentNode);
                    //binder needed on dialogue next? (maybe) as this action is between node types (could be user prompt next, could be dialogue, could be coming out of a one way?)
                    break;
                case GraphDialogueEventType.DialogueUserResponse:
                    // You might want to update UI or analytics here before the Director advances
                    if (data.ResponseNode != null)
                    {
                        var userResponseMediator = mediator.EvaluateResponseNode(data.ResponseNode);
                    }
                    // binder needed on dialogue user response? 
                    break;

                case GraphDialogueEventType.DialoguePrevious:
                    RouteEvaluatePrevious(data.PreviousNode);
                    break;
                case GraphDialogueEventType.DialogueUserRepeat:
                    break;

                case GraphDialogueEventType.DialogueEnd:
                    if (data.ExitNode!=null)
                    {
                        
                    }
                    break;
            }
        }
        protected void RouteEvaluateNext(RTFPNode node)
        {
            if (node == null) return;
            if (node is RTDialogueNode d)
            {
                //we need to go to the next event, if we can, and then notify the graph to do that
                if (mediator.EvaluateDialogueNode(d))
                {

                }
            }
            else if (node is RTResponseNode r)
            {
                mediator.EvaluateResponseNode(r);
            }
            else if (node is RTCombineNode c)
            {
                mediator.EvaluateCombination(c);
            }
            else if (node is RTEntryNode e)
            {
                mediator.EvaluateEntryNode(e);
            }
        }
        protected void RouteEvaluatePrevious(RTFPNode node)
        {

        }
        protected static bool TryGetStringMember(object obj, string name, out string value)
        {
            value = null;
            var t = obj.GetType();
            // Field
           
            return false;
        }
    }
}
