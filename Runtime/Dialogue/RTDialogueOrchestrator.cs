namespace FuzzPhyte.Dialogue
{
    using System;
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
        //[SerializeField] protected RTDialogueDirector director;
        [SerializeField]
        [Tooltip("spawned prefab for nested visual information")]
        protected GameObject activeNodeVisual;
        [Space]
        [Tooltip("World Location Relative the Player to spawn UI")]
        public Transform DefaultDialoguePosition;
        protected void Awake()
        {
            if (!handler) handler = RTGraphDialogueEventHandler.Instance;
            if (!mediator) mediator = GetComponent<RTDialogueMediator>();
            if (!binder) binder = GetComponent<RTExposedBinder>();
            //if(!director) director = GetComponent<RTDialogueDirector>();

            if(handler == null || mediator == null || binder == null )
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
        /// <summary>
        /// Repeated events = next events (this system doesn't need to know)
        /// </summary>
        /// <param name="data"></param>
        protected void Handle(GraphEventData data)
        {
            switch (data.EventType)
            {

                case GraphDialogueEventType.DialogueSetup:
                    // Populate binder or init transient state if needed
                    // need binder on setup?
                    // anything we need to do only once? (before we start our timer etc?)
                    Debug.Log($"Dialogue: Setup!");
                    break;
                case GraphDialogueEventType.DialogueStart:
                    // Drive any entry logic (e.g., play a timeline, fade-in, spawn an NPC, etc.)
                    if (data.EntryNode != null) 
                    {
                        if (mediator.EvaluateEntryNode(data.EntryNode))
                        {
                            //valid
                            Debug.Log($"Dialogue Start!");
                        }
                    }
                    //binder needed on dialogue starting? (entry node? = no)
                    break;
                case GraphDialogueEventType.DialogueUserNext:
                    if (data.DialogueNode != null)
                    {
                        if (mediator.EvaluateDialogueNode(data.DialogueNode))
                        {
                            //valid
                            Debug.Log($"Dialogue: {data.DialogueNode.mainDialogue.dialogueText}");
                            if (activeNodeVisual!=null)
                            {
                                ClearActiveVisual();
                            }
                            DrawDialogueVisual(data.DialogueNode);
                        }
                    }
                    //binder needed on dialogue next? (maybe) as this action is between node types (could be user prompt next, could be dialogue, could be coming out of a one way?)
                    break;
                case GraphDialogueEventType.DialogueUserResponseNext:
                    if(data.ResponseNode != null)
                    {
                        if (mediator.EvaluateResponseNode(data.ResponseNode))
                        {
                            //valid
                            Debug.Log($"Number Response options: {data.ResponseNode.userIncomingPrompts.Count}");
                            if (activeNodeVisual != null)
                            {
                                ClearActiveVisual();
                            }
                            DrawResponseVisual(data.ResponseNode);
                        }
                    }
                    break;
                case GraphDialogueEventType.DialogueUserResponseCollected:
                    if (data.ResponseNode != null)
                    {
                        if (mediator.EvaluateResponseNode(data.ResponseNode))
                        {
                            //valid
                            Debug.Log($"User Response Index picked: {data.UserResponsePromptIndex}");
                            if (activeNodeVisual != null)
                            {
                                ClearActiveVisual();
                            }
                        }
                    }
                    // binder needed on dialogue user response? 
                    break;
                case GraphDialogueEventType.DialogueUserPrevious:
                    if (data.DialogueNode != null)
                    {
                        //relaunch visual system with the data.DialogueNode
                        if (mediator.EvaluateDialogueNode(data.DialogueNode))
                        {
                            //valid
                            Debug.Log($"Dialogue User Previous: {data.DialogueNode.mainDialogue.dialogueText}");
                            if (activeNodeVisual != null)
                            {
                                ClearActiveVisual();
                            }
                            DrawDialogueVisual(data.DialogueNode);
                        }
                    }
                    break;
                case GraphDialogueEventType.DialogueUserResponsePrevious:
                    if (data.ResponseNode != null)
                    {
                        //relaunch the visual/user input prompt system with data.ResponseNode
                        if (mediator.EvaluateResponseNode(data.ResponseNode))
                        {
                            //valid
                            Debug.Log($"User Response: {data.ResponseNode.userIncomingPrompts.Count}");
                            for(int i = 0; i < data.ResponseNode.userIncomingPrompts.Count; i++)
                            {
                                Debug.Log($"Response {i}: node index: {data.ResponseNode.userIncomingPrompts[i].Index}");
                                var aNode = data.ResponseNode.userIncomingPrompts[i].mainDialogue;
                                //should be a RTSinglePrompt
                                Debug.Log($"Response Text:{aNode.dialogueText}");
                            }
                            if (activeNodeVisual != null)
                            {
                                ClearActiveVisual();
                            }
                            DrawResponseVisual(data.ResponseNode);
                        }
                    }
                    break;
                case GraphDialogueEventType.DialogueEnd:
                    if (data.ExitNode!=null)
                    {
                        if (mediator.EvaluateExitNode(data.ExitNode))
                        {
                            Debug.Log($"Exit Node!");
                            if (activeNodeVisual != null)
                            {
                                ClearActiveVisual();
                            }
                        }
                    }
                    break;
            }
        }
        
        protected static bool TryGetStringMember(object obj, string name, out string value)
        {
            value = null;
            var t = obj.GetType();
            // Field
           
            return false;
        }

        #region Unity Visuals
        protected void DrawDialogueVisual(RTDialogueNode nodeData)
        {

        }
        protected void DrawResponseVisual(RTResponseNode nodeData)
        {

        }
        protected void ClearActiveVisual()
        {

        }
        #endregion
    }
}
