namespace FuzzPhyte.Dialogue
{
    using System;
    using FuzzPhyte.Utility.Animation;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Playables;

    /// <summary>
    /// our glue to the dialogue graph dialogue system
    /// answers the question of = "What needs to come together to interface with the conversation?"
    /// General Notes: only this class ever touches prefabs and scene objects. Mediator is in charge of confirming node info is valid. Director never knows about UI/UX presentation
    /// </summary>
    public class RTDialogueOrchestrator : MonoBehaviour
    {
        [SerializeField] protected string conversationID;
        [SerializeField] protected string graphID;
        [Space]
        [Header("Required References")]
        [SerializeField] protected RTGraphDialogueEventHandler handler;
        [SerializeField] protected RTDialogueMediator mediator;
        [SerializeField] protected RTExposedBinder binder;
        [SerializeField] protected DialogueUnity dialogueUnity;
        [SerializeField] protected RTDialogueDirector director;
        protected IDialogueDirectorActions directorActions;

        [Space]
        [Header("Dialogue Generic Prefabs")]
        [Tooltip("The base UI object for a dialogue- should be utilizing IDialogue")]
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
        #region Unity Methods
        protected virtual void Awake()
        {
            if (!handler) handler = RTGraphDialogueEventHandler.Instance;
            if (!mediator) mediator = GetComponent<RTDialogueMediator>();
            if (!binder) binder = GetComponent<RTExposedBinder>();
            if (!dialogueUnity) dialogueUnity = GetComponent<DialogueUnity>();
            if (!director) director = GetComponent<RTDialogueDirector>();

            if (handler == null || mediator == null || binder == null||dialogueUnity==null||director==null)
            {
                Debug.LogError($"Missing a critical reference!");
            }
            directorActions = director.GetComponent<IDialogueDirectorActions>();
            if (directorActions == null)
            {
                Debug.LogError($"Your director isn't implementing the IDialogueDirectorActions interface");
            }

        }
        protected virtual void Start()
        {
            if (dialogueUnity != null)
            {
                if (director != null)
                {
                    conversationID = directorActions.ReturnConversationID();
                    graphID = directorActions.ReturnGraphID();
                    dialogueUnity.SetupDialogue(directorActions, DialogueWorldCanvas, userID);
                }
                else
                {
                    dialogueUnity.SetupDialogue(DialogueWorldCanvas, userID);
                }
            }  
        }
        protected virtual void OnEnable()
        {
            if (handler != null)
            {
                handler.OnGraphDialogueEvent += Handle;
            }
        }
        protected virtual void OnDisable() 
        {
            if (handler != null)
            {
                handler.OnGraphDialogueEvent -= Handle;
            }
        }
        #endregion
        /// <summary>
        /// Repeated events = next events (this system doesn't need to know)
        /// </summary>
        /// <param name="data"></param>
        protected virtual void Handle(GraphEventData data)
        {
            Debug.LogWarning($"Handle incoming data!!, GraphID: {data.GraphId}, ConvoID: {data.ConversationId}, with event type {data.EventType.ToString()}");
            if(data.GraphId != graphID && data.ConversationId != conversationID)
            {
                return;
            }
            switch (data.EventType)
            {
                // event that occurs at the end of a dialogue node that contains timeline details
                case GraphDialogueEventType.DialogueTimeline:
                case GraphDialogueEventType.DialogueExitTimeline:
                
                    bool proceedWithProcess = false;
                    if (data.EventType == GraphDialogueEventType.DialogueExitTimeline)
                    {
                        proceedWithProcess = mediator.EvaluateExitNode(data.ExitNode);
                    }
                    else
                    {
                        proceedWithProcess = mediator.EvaluateDialogueNode(data.DialogueNode);
                    }
                    if (proceedWithProcess)
                    {
                        RTTimelineDetails timelineData = data.Payload as RTTimelineDetails;
                        if (mediator.EvaluateTimelineDetails(timelineData))
                        {
                            Debug.Log($"Timeline details confirmed! Looking for a binder: [{timelineData.BinderDirectorLookUpName}]");
                            switch (timelineData.TAction)
                            {
                                case TimelineAction.Play:
                                    RunDialogueTimeline(timelineData);
                                    break;
                                case TimelineAction.Pause:
                                    PauseDialogueTimeline(timelineData);
                                    break;
                                case TimelineAction.Stop:
                                    StopDialogueTimeline(timelineData);
                                    break;
                                case TimelineAction.Resume:
                                    ResumeDialogueTimeline(timelineData);
                                    break;
                                case TimelineAction.Reset:
                                    ResetDialogueTimeline(timelineData, (float)timelineData.StartTime);
                                    break;
                                case TimelineAction.Setup:
                                    SetupDialogueTimeline(timelineData);
                                    break;
                                case TimelineAction.SetupAndPlay:
                                    SetupAndRunDialogueTimeline(timelineData);
                                    break;
                            }
                        }
                        else
                        {
                            Debug.LogError($"Timeline Data Details failed evaluation");
                        }
                    }
                    else
                    {
                        Debug.LogError($"Dialogue and Exit information were incorrect!");
                    }
                    break;
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
                            DrawDialogueVisual(data.DialogueNode,false, data.PreviousNode, data.NextNode, 1.5f);
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
                            if(data.UserDelayPaddedTime > 0)
                            {
                                StartCoroutine(DelayResponseVisuals(data));
                            }
                            else
                            {
                                ClearActiveVisual();
                                DrawResponseVisual(data.ResponseNode, data.PreviousNode, data.NextNode);
                            }
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
                            DrawDialogueVisual(data.DialogueNode,false, data.PreviousNode, data.NextNode);
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
                            
                            if (data.ExitNode.PlayableDirectorRef != string.Empty)
                            {
                                //try to find it
                                SetupTimelineExit(data.ExitNode);
                            }
                            
                            if (activeNodeVisual != null)
                            {
                                ClearActiveVisual();
                            }
                        }
                    }
                    break;
                case GraphDialogueEventType.DialogueUserTranslate:
                    if (data.DialogueNode != null)
                    {
                        if (mediator.EvaluateDialogueNode(data.DialogueNode))
                        {
                            //valid
                            Debug.Log($"Dialogue/Translate: {data.DialogueNode.translatedDialogue.dialogueText}");
                            ClearActiveVisual();
                            DrawDialogueVisual(data.DialogueNode, true,data.PreviousNode, data.NextNode, 1.5f);
                        }
                    }
                    break;
            }
        }
        
        IEnumerator DelayResponseVisuals(GraphEventData someData)
        {
            yield return new WaitForSecondsRealtime(someData.UserDelayPaddedTime);
            ClearActiveVisual();
            DrawResponseVisual(someData.ResponseNode, someData.PreviousNode, someData.NextNode);
        }
        #region Timelines Related Functions
        
        /// <summary>
        /// Run command for timeline
        /// </summary>
        /// <param name="details"></param>
        protected void RunDialogueTimeline(RTTimelineDetails details)
        {
            WithTimeline(details, tl =>
            {
                tl.PlayTimeline(details.PlayTimelineOnce);
            });
        }
        /// <summary>
        /// Stop command for timeline
        /// </summary>
        /// <param name="details"></param>
        protected void StopDialogueTimeline(RTTimelineDetails details)
        {
            WithTimeline(details, tl => tl.StopTimeline());
        }
        /// <summary>
        /// Pause command for timeline
        /// </summary>
        /// <param name="details"></param>
        protected void PauseDialogueTimeline(RTTimelineDetails details)
        {
            WithTimeline(details, tl => tl.PauseTimeline());
        }
        /// <summary>
        /// Resume command for timeline
        /// </summary>
        /// <param name="details"></param>
        protected void ResumeDialogueTimeline(RTTimelineDetails details)
        {
            WithTimeline(details, tl => tl.ResumeTimeline());
        }
        /// <summary>
        /// Reset commmand for timeline
        /// </summary>
        /// <param name="details"></param>
        /// <param name="startTime"></param>
        protected void ResetDialogueTimeline(RTTimelineDetails details, float startTime = 0f)
        {
            WithTimeline(details, tl => tl.ResetTimeline(startTime));
        }
        /// <summary>
        /// Setup command for timeline
        /// </summary>
        /// <param name="details"></param>
        protected void SetupDialogueTimeline(RTTimelineDetails details)
        {
            WithTimeline(details, tl =>
            {
                tl.SetupTimeline(details);
            });
        }
        /// <summary>
        /// Setup and run command for timeline
        /// </summary>
        /// <param name="details"></param>
        protected void SetupAndRunDialogueTimeline(RTTimelineDetails details)
        {
            WithTimeline(details, tl =>
            {
                tl.SetupTimeline(details);
                tl.PlayTimeline(details.PlayTimelineOnce);
            });
        }
        
        /// <summary>
        /// internal call to handle actions associated with details
        /// </summary>
        /// <param name="details"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private bool WithTimeline(RTTimelineDetails details, System.Action<IDialogueTimeline> action)
        {
            var go = ReturnBinderObject(details.BinderDirectorLookUpName);
            if (go != null && go.TryGetComponent<IDialogueTimeline>(out var tl))
            {
                action?.Invoke(tl);
                return true;
            }

            Debug.LogWarning($"[Timeline] Could not resolve IDialogueTimeline for '{details.BinderDirectorLookUpName}'.");
            return false;
        }
        #endregion
        #region Unity Visuals
        protected virtual void SetupTimelineExit(RTExitNode nodeData)
        {
            GameObject timelineOBJ = ReturnBinderObject(nodeData.PlayableDirectorRef);
            if (timelineOBJ!=null)
            {
                if (timelineOBJ.GetComponent<PlayableDirector>())
                {
                    timelineOBJ.GetComponent<PlayableDirector>().Play();
                }
            }
        }
        /// <summary>
        /// method to draw our dialogue visual UI item
        /// </summary>
        /// <param name="nodeData"></param>
        /// <param name="previousNode"></param>
        /// <param name="nextNode"></param>
        /// <param name="delayTime"></param>
        protected virtual void DrawDialogueVisual(RTDialogueNode nodeData, bool useTranslation=false, RTFPNode previousNode = null, RTFPNode nextNode = null, float delayTime=1.5f)
        {
            Transform spawnLoc = null;
            if (DefaultDialoguePosition != null)
            {
                spawnLoc = this.DefaultDialoguePosition;
            }
            
            if (nodeData.useWorldLoc)
            {
                //look up location in binder
                GameObject location = ReturnBinderObject(nodeData.WorldLocationSceneName);
                if (location!=null)
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
                Debug.LogError($"Missing Canvas or spawn location");
                return;
            }
            if (nodeData.usePrefabs)
            {
                if (nodeData.UIPanelPrefab != null)
                {
                    activeNodeVisual = Instantiate(nodeData.UIPanelPrefab, DialogueWorldCanvas.transform);
                    NodeVisualSetup(activeNodeVisual, dialogueUnity,nodeData, useTranslation,previousNode, nextNode);
                }
            }else if (nodeData.useNames)
            {
                activeNodeVisual = ReturnBinderObject(nodeData.WorldObjectPanelName);
                NodeVisualSetup(activeNodeVisual, dialogueUnity,nodeData, useTranslation,previousNode, nextNode);
            }
            else if (DialogueBaseUIPrefab != null)
            {
                activeNodeVisual = Instantiate(DialogueBaseUIPrefab, DialogueWorldCanvas.transform);
                NodeVisualSetup(activeNodeVisual, dialogueUnity,nodeData, useTranslation,previousNode, nextNode);
            }
            
            //if we have a blend shape? and facial animation to go with it?
            if (nodeData.incomingCharacter.characterBlendShapeName != string.Empty && nodeData.mainDialogue.faceAnimation!=null)
            {
                //can we find it?
                GameObject characterFace = ReturnBinderObject(nodeData.incomingCharacter.characterBlendShapeName);
                
                //Debug.LogWarning($"Face Animation?!");
                if (characterFace != null&&nodeData.mainDialogue.faceAnimation!=null) 
                {
                    //Debug.LogWarning($"We found a face, and a clip!");
                    //blend shape here?
                    IAnimInjection injection = characterFace.GetComponent<IAnimInjection>();
                    if (injection!=null)
                    {
                        injection.PlayClip(nodeData.mainDialogue.faceAnimation);
                    }
                }
            }
            if (nodeData.incomingCharacter.characterObjectName != string.Empty&&nodeData.mainDialogue.bodyAnimation!=null)
            {
                GameObject characterBody = ReturnBinderObject(nodeData.incomingCharacter.characterObjectName);
                if (characterBody != null)
                {
                    //Debug.LogWarning($"We found a body, and there's a clip");
                    IAnimInjection injection = characterBody.GetComponent<IAnimInjection>();
                    if (injection!=null)
                    {
                        injection.PlayClip(nodeData.mainDialogue.bodyAnimation);
                    }
                }
            }
        }
        /// <summary>
        /// Visual internal setup for our UI prefab
        /// </summary>
        /// <param name="nodeData"></param>
        /// <param name="previousNode"></param>
        /// <param name="nextNode"></param>
        private void NodeVisualSetup(GameObject theSpawnedPrefab, DialogueUnity dialUnity, RTDialogueNode nodeData, bool useTranslation=false, RTFPNode previousNode = null, RTFPNode nextNode = null)
        {
            theSpawnedPrefab.transform.localPosition = Vector3.zero;
            theSpawnedPrefab.transform.localRotation = Quaternion.identity;

            IDialogueGraphBase graphBase = theSpawnedPrefab.GetComponent<IDialogueGraphBase>();
            if (graphBase != null)
            {
                graphBase.SetupDialoguePanel(dialUnity, nodeData, useTranslation,previousNode, nextNode);
            }
            else
            {
                Debug.LogError($"The {theSpawnedPrefab.name} prefab isn't utilizing the IDialogueGraphBase interface!");
            }
            IDialogueActions graphAction = theSpawnedPrefab.GetComponent<IDialogueActions>();
            if (graphAction != null)
            {
                graphAction.PlayAudioDialogueBlock();
            }
            else
            {
                Debug.LogError($"The {theSpawnedPrefab.name} prefab isn't utilizing the IDialogueActions interface!");
            }
        }
        IEnumerator WaitBeforeDialogueResponse(float genericTime)
        {
            yield return new WaitForSeconds(genericTime);
            IDialogueActions dialogueAction = activeNodeVisual.GetComponent<IDialogueActions>();
            if (dialogueAction != null)
            {
                dialogueAction.PlayAudioDialogueBlock();
            }
        }
        protected virtual void DrawResponseVisual(RTResponseNode nodeData, RTFPNode previousNode = null, RTFPNode nextNode = null)
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
                    IDialogueGraphBase baseGraphInterface = activeNodeVisual.GetComponent<IDialogueGraphBase>();
                    if (baseGraphInterface!=null)
                    {
                        baseGraphInterface.SetupResponsePanel(dialogueUnity, nodeData, previousNode, nextNode);
                        StartCoroutine(WaitBeforeUserResponse(1.5f));
                    }
                    else
                    {
                        Debug.LogError($"Your {UserResponseUIPrefabGroup.name} prefab isn't utilizing the IDialogueGraphBase interface!");
                    }
                }
            }
            else
            {
                if (nodeData.userIncomingPrompts.Count > 0)
                {
                    for (int i = 0; i < nodeData.userIncomingPrompts.Count; i++)
                    {
                        //Debug.Log($"Response {i}: node index: {nodeData.userIncomingPrompts[i].Index}");
                        var locationName = nodeData.userIncomingPrompts[i].spawnLocationID;
                        GameObject location = ReturnBinderObject(locationName);
                        if (UserResponseUIPrefabItem != null && location != null)
                        {
                            var aPromptButton = Instantiate(UserResponseUIPrefabItem, location.transform);
                            aPromptButton.transform.localPosition = Vector3.zero + PrefabUserOffset;
                            aPromptButton.transform.localRotation = Quaternion.identity;
                            IDialogueGraphResponseButton graphInterface = aPromptButton.GetComponent<IDialogueGraphResponseButton>();
                            if (graphInterface!=null)
                            {
                                graphInterface.SetupUserResponse(i, this.directorActions, nodeData.userIncomingPrompts[i].mainDialogue.dialogueText,nodeData.userIncomingPrompts[i].mainDialogue.textAudio, UserPromptAudioSource);
                                graphInterface.UpdateMainButtonSprite(nodeData.userIncomingPrompts[i].promptIcon);
                                activeResponseVisuals.Add(aPromptButton);
                            }
                            else
                            {
                                Debug.LogError($"Your {UserResponseUIPrefabItem.name} prefab isn't utilizing the IDialogueGraphResponseButton interface!");
                            }
                        }
                    }
                }
            }
        }
        IEnumerator WaitBeforeUserResponse(float genericTime)
        {
            yield return new WaitForSeconds(genericTime);
            IDialogueActions actionsDialogue = activeNodeVisual.GetComponent<IDialogueActions>();
            if (actionsDialogue!=null)
            {
                actionsDialogue.PlayAudioDialogueBlock();
            }
        }
        protected virtual void ClearActiveVisual()
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
        /// <summary>
        /// Quick return gameobject from the binder based on a name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private GameObject ReturnBinderObject(string name)
        {
            GameObject objBinder = null;
            binder.TryGet<GameObject>(name, out objBinder);
            return objBinder;
        }
    }
}
