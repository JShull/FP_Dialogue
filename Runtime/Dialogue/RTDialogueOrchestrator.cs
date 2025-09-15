namespace FuzzPhyte.Dialogue
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
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
        [SerializeField] protected DialogueUnity dialogueUnity;
        [SerializeField] protected RTDialogueDirector director;
        [Space]
        public GameObject DialogueBaseUIPrefab;
        [Tooltip("Basically the same thing as the dialogue base prefab, but for user response options")]
        public GameObject UserResponseUIPrefabGroup;
        [Tooltip("Prefab for each individual user response option - in world")]
        public GameObject UserResponseUIPrefabItem;
        public Vector3 PrefabUserOffset = new Vector3(0, 1.5f, 0);
        public AudioSource UserPromptAudioSource;
        [Space]
        public string userID = "1234Test";
        public Canvas DialogueWorldCanvas;
        [SerializeField]
        [Tooltip("spawned prefab for nested visual information")]
        protected GameObject activeNodeVisual;
        List<GameObject> activeResponseVisuals = new List<GameObject>();

        [Space]
        [Tooltip("World Location Relative the Player to spawn UI")]
        public Transform DefaultDialoguePosition;
        protected void Awake()
        {
            if (!handler) handler = RTGraphDialogueEventHandler.Instance;
            if (!mediator) mediator = GetComponent<RTDialogueMediator>();
            if (!binder) binder = GetComponent<RTExposedBinder>();
            if(!dialogueUnity) dialogueUnity = GetComponent<DialogueUnity>();
            //if(!director) director = GetComponent<RTDialogueDirector>();

            if (handler == null || mediator == null || binder == null||dialogueUnity==null||director==null)
            {
                Debug.LogError($"Missing a critical reference!");
            }
        }
        protected void Start()
        {
            if (dialogueUnity != null)
            {
                if (director != null)
                {
                    dialogueUnity.SetupDialogue(director, DialogueWorldCanvas, userID);
                }
                else
                {
                    dialogueUnity.SetupDialogue(DialogueWorldCanvas, userID);
                }
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
                            dialogueUnity.ActivateDialogue();
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
                            ClearActiveVisual();
                            DrawDialogueVisual(data.DialogueNode,data.PreviousNode,data.NextNode);
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
                            ClearActiveVisual();
                            DrawResponseVisual(data.ResponseNode,data.PreviousNode,data.NextNode);
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
                            ClearActiveVisual();
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
                            ClearActiveVisual();
                            DrawDialogueVisual(data.DialogueNode,data.PreviousNode,data.NextNode);
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
                            ClearActiveVisual();
                            DrawResponseVisual(data.ResponseNode,data.PreviousNode,data.NextNode);
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
        protected void DrawDialogueVisual(RTDialogueNode nodeData, RTFPNode previousNode = null, RTFPNode nextNode = null)
        {
            Transform spawnLoc = this.DefaultDialoguePosition;
            if (nodeData.useWorldLoc)
            {
                //look up location in binder
                GameObject location = null;
                if (binder.TryGet<GameObject>(nodeData.WorldLocationSceneName, out location))
                {
                    spawnLoc= location.transform;
                }
            }
            //move my canvas to the location
            if (DialogueWorldCanvas != null && spawnLoc != null)
            {
                DialogueWorldCanvas.transform.position = spawnLoc.position;
                DialogueWorldCanvas.transform.rotation = spawnLoc.rotation;
            }
            else
            {
                Debug.LogError($"missing canvas or location");
                return;
            }
            // use the dialogue prefab
            if (DialogueBaseUIPrefab != null)
            {
                activeNodeVisual = Instantiate(DialogueBaseUIPrefab,DialogueWorldCanvas.transform);
                activeNodeVisual.transform.localPosition = Vector3.zero;
                activeNodeVisual.transform.localRotation = Quaternion.identity;

                if (activeNodeVisual.GetComponent<UIDialogueBase>())
                {
                    Debug.Log($"We have a UIDialogueBase");
                    activeNodeVisual.GetComponent<UIDialogueBase>().SetupDialoguePanel( dialogueUnity,nodeData,previousNode,nextNode);
                    StartCoroutine(WaitBeforeDialogueResponse(1.5f));
                    //activeNodeVisual.GetComponent<UIDialogueBase>().PlayDialogueBlock();
                }
                //setup data based on interface?
            }
            if (nodeData.useNames)
            {
                Debug.LogWarning($"This isn't implemented yet, but will need to use the binder to find the location of our items");
                //world location in binder
            }
            if (nodeData.usePrefabs)
            {
                //
                Debug.LogWarning($"This isn't implemented yet, but will use the prefabs for our options");
            }
        }
        IEnumerator WaitBeforeDialogueResponse(float genericTime)
        {
            yield return new WaitForSeconds(genericTime);
            activeNodeVisual.GetComponent<UIDialogueBase>().PlayDialogueBlock();
        }
        protected void DrawResponseVisual(RTResponseNode nodeData, RTFPNode previousNode = null, RTFPNode nextNode = null)
        {
            // draw the response visual - if we aren't using world locations - we can rely on the existing dialogue response system
            if (!nodeData.UseWorldLocations)
            {
                //old school method group block of responses
                if (UserResponseUIPrefabGroup != null)
                {
                    activeNodeVisual = Instantiate(UserResponseUIPrefabGroup, DialogueWorldCanvas.transform);
                    activeNodeVisual.transform.localPosition = Vector3.zero + PrefabUserOffset;
                    activeNodeVisual.transform.localRotation = Quaternion.identity;

                    if (activeNodeVisual.GetComponent<UIDialogueBase>())
                    {
                        Debug.Log($"We have a UIDialogueBase");
                        activeNodeVisual.GetComponent<UIDialogueBase>().SetupResponsePanel(dialogueUnity, nodeData, previousNode, nextNode);
                        StartCoroutine(WaitBeforeUserResponse(1.5f));
                    }
                    //setup data based on interface?
                }
            }
            else
            {
                if (nodeData.userIncomingPrompts.Count > 0)
                {
                    for (int i = 0; i < nodeData.userIncomingPrompts.Count; i++)
                    {
                        Debug.Log($"Response {i}: node index: {nodeData.userIncomingPrompts[i].Index}");
                        var locationName = nodeData.userIncomingPrompts[i].spawnLocationID;
                        binder.TryGet<GameObject>(locationName, out GameObject location);
                        if (UserResponseUIPrefabItem != null && location != null)
                        {
                            var aPromptButton = Instantiate(UserResponseUIPrefabItem, location.transform);
                            aPromptButton.transform.localPosition = Vector3.zero + PrefabUserOffset;
                            aPromptButton.transform.localRotation = Quaternion.identity;
                            if (aPromptButton.GetComponent<UIDialogueButton>())
                            {
                                aPromptButton.GetComponent<UIDialogueButton>().SetupUserResponse(i, this.director, nodeData.userIncomingPrompts[i].mainDialogue.textAudio, UserPromptAudioSource);
                                aPromptButton.GetComponent<UIDialogueButton>().UpdateRefIconSprite(nodeData.userIncomingPrompts[i].promptIcon);
                                activeResponseVisuals.Add(aPromptButton);
                            }
                        }
                        //we would generate i canvas buttons and place them in the world based on the binder locations
                    }
                }
            }

        }
        IEnumerator WaitBeforeUserResponse(float genericTime)
        {
            yield return new WaitForSeconds(genericTime);
            activeNodeVisual.GetComponent<UIDialogueBase>().PlayDialogueBlock();
        }
        protected void ClearActiveVisual()
        {
            if (activeNodeVisual != null)
            {
                Destroy(activeNodeVisual);
                activeNodeVisual = null;
            }
            if (activeResponseVisuals.Count > 0)
            {
                for (int i = 0; i < activeResponseVisuals.Count; i++)
                {
                    var go = activeResponseVisuals[i];
                    if (go != null)
                    {
                        Destroy(go);
                    }
                }

                activeResponseVisuals.Clear();
            }
        }
        #endregion
    }
}
