using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using FuzzPhyte.Utility;
using FuzzPhyte.Utility.EDU;
using FuzzPhyte.Utility.Notification;
//using FuzzPhyte.InputSystem;
//using FuzzPhyte.Dialogue;
//using FuzzPhyte.Standards.VA.CS;
using FuzzPhyte.UI;
namespace FuzzPhyte.Dialogue.UI
{
    /// <summary>
    /// Overlay script for the UIToolkit and Unity
    /// </summary>
    public class FP_UI_Overlay : FP_UI
    {
        public string OverlayContainerName = "OverlayContainer";
        public string OverlayPopUpContainerName = "OverlayUIPopUp";
        public string ObjectiveGroupBoxName = "ObjectiveGroupBox";
        public string ObjectiveUIPopUpLabel = "PopUpInformation";
 
        public string HideClass = "hide";
        public string FlexHideClass = "flexHide";
        public string DialogueTextClass = "dialogueLabel";
        public string DialogueTextClassBold = "dialogueLabelBold";
        public string ToggleFontClass = "cocoFont";
        public string ToggleFontClassList = "list";
        private VisualElement OverlayContainer;
        private VisualElement OverlayPopUpContainer;
        private GroupBox ObjectiveGroupBox;
        private Label OverlayPopUpInformationLabel;
        
        #region Dialogue Details
        [Header("Dialogue Details")]
        public string OverlayDialogueName = "OverlayDialogueContainer";
        public string DialogueContainerName = "DialogueList";
        public string DialogueButtonsContainerName = "DialogueButtons";
        public string OverlayDialogueNextButtonName = "DialogueNextButton";
        public string OverlayDialoguePreviousButtonName = "DialoguePreviousButton";
        public string DialogueIconContainerName = "CharacterIcon";
        public string DialogueCharacterLabelName = "CharacterName";
        public string DialogueProgressContainer = "DialogueProgressBar";
        public string DialogueProgessValueName = "DialogueProgressValue";
        [Space]
        public UnityEvent NextButtonPushed;
        public UnityEvent PreviousButtonPushed;
        public UnityEvent DialogueFinishedEvent;
        public AudioSource UIAudioPlayer;
        //Parameters for Dialogue
        private VisualElement OverlayDialogueContainer;
        private VisualElement DialogueContainer;
        private VisualElement CharacterIconContainer;
        private VisualElement DialogueButtonsContainer;
        private Label CharacterName;
        private VisualElement DialogueProgressBarContainer;
        private VisualElement DialogueProgressValueContainer;
        private Button OverlayDialogueNextButton;
        private Button OverlayDialoguePreviousButton;
        private FP_Dialogue _currentDialogue;
        private bool _continuousDialogueActive; //check to use Update loop to run dialogue through timing system
        private float _dialogueTimer; //timer for dialogue
        private float _dialogueRunningTimer;//keep track of delta time
        private int _dialogueIndex; //keeping tabs on where we are in the conversation index
        #endregion
        private List<VisualElement> DialoguePlaceholderList = new List<VisualElement>();
        private List<FP_Vocab> _moduleVocabReference = new List<FP_Vocab>();
        private Dictionary<string, Toggle> objectiveList = new Dictionary<string, Toggle>();
        private List<GameObject> spawnedVisuals = new List<GameObject>();
        //private List<Vector3> spawnedVisualLocations = new List<Vector3>();
        private List<FP_OverlayNotification> overlayPopUpUI = new List<FP_OverlayNotification>();
        private FP_OverlayNotification currentOverlay;
        public Camera PlayerCamera;
        private float _runTime;
        [Header("Debug Related")]
        public bool DebugOn;
        public string DebugOverlayContainerName = "OverlayDebug";
        public string DebugMouseLabel = "MouseDebugLabel";
        private VisualElement DebugContainer;
        private Label MouseLabelDebug;
        //delegate for establishing a way for other objects to get notifications via dialogue updates
        public delegate void DialogueDelegate(FP_OverlayNotification dialogueParameter );
        
        // Define the event using the delegate
        public event DialogueDelegate DialogueNextButtonEvent;
        public event DialogueDelegate DialoguePreviousButtonEvent;
        public event DialogueDelegate DelegateDialogueFinishedEvent;
        public event DialogueDelegate DelegateDialogueStartEvent;

        //cached passedEvent to hand-off as needed
        private FP_PassedEvent _passedEvent;
        
        //Interface needs
        //Setup
        //Update Vocabulary
        //Start Dialogue Model Type 1
        //Start Dialogue Model Type 2
        //Reset Dialogue Model
        //Next Dialogue Button
        //Previous Dialogue Button
        //User Input Selection
        //Hide Overlay
        //Unhide Overlay

        public override void Awake()
        {
            base.Awake();
            OverlayContainer = RootContainer.Q<VisualElement>(OverlayContainerName);
            OverlayPopUpContainer = RootContainer.Q<VisualElement>(OverlayPopUpContainerName);
            //get Group Box
            ObjectiveGroupBox = OverlayContainer.Q<GroupBox>(ObjectiveGroupBoxName);
            //label for overlay pop up
            OverlayPopUpInformationLabel=OverlayPopUpContainer.Q<Label>(ObjectiveUIPopUpLabel);
            //get dialogue information
            OverlayDialogueContainer = RootContainer.Q<VisualElement>(OverlayDialogueName);
            DialogueContainer = OverlayDialogueContainer.Q<VisualElement>(DialogueContainerName);
            CharacterIconContainer = OverlayDialogueContainer.Q<VisualElement>(DialogueIconContainerName);
            CharacterName = CharacterIconContainer.Q<Label>(DialogueCharacterLabelName);
            OverlayDialogueNextButton = OverlayDialogueContainer.Q<Button>(OverlayDialogueNextButtonName);
            OverlayDialoguePreviousButton = OverlayDialogueContainer.Q<Button>(OverlayDialoguePreviousButtonName);
            DialogueButtonsContainer = OverlayDialogueContainer.Q<VisualElement>(DialogueButtonsContainerName);
            DialogueProgressBarContainer = OverlayDialogueContainer.Q<VisualElement>(DialogueProgressContainer);
            DialogueProgressValueContainer = DialogueProgressBarContainer.Q<VisualElement>(DialogueProgessValueName);

            //debug
            DebugContainer = RootContainer.Q<VisualElement>(DebugOverlayContainerName);
            MouseLabelDebug = DebugContainer.Q<Label>(DebugMouseLabel);
            if (DebugOn)
            {
                DebugContainer.style.visibility = Visibility.Visible;
                DebugContainer.style.display = DisplayStyle.Flex;
            }
            else
            {
                DebugContainer.style.visibility = Visibility.Hidden;
                DebugContainer.style.display = DisplayStyle.None;
            }
            OverlayDialogueNextButton.clicked +=()=> DialogueNextButtonAction();
            OverlayDialoguePreviousButton.clicked += () => DialoguePreviousButtonAction();
            
            AddNewStyleToVisualElement(OverlayContainer,HideClass);
            AddNewStyleToVisualElement(OverlayPopUpContainer,HideClass);
            AddNewStyleToVisualElement(OverlayDialogueContainer,HideClass);
        }
        
        public void UpdateVocabList(FP_Vocab aWord)
        {
            if (!_moduleVocabReference.Contains(aWord))
            {
                _moduleVocabReference.Add(aWord);
            }
        }
        
        public void StartVocabDialogueConversation(FP_Vocab VocabDetails, FP_Theme displayDetails,float individualDuration=4)
        {
            var conversationData = new List<FP_OverlayNotification>();
            FP_OverlayNotification tempNotification = ScriptableObject.CreateInstance<FP_OverlayNotification>();

            tempNotification.OverlayStatus = false;
            tempNotification.OverlayDuration = individualDuration;
            tempNotification.OverlayObjective = "Computer Science Vocabulary Word: " + VocabDetails.Word;
            tempNotification.OverlayTaskType = OverlayType.Vocabulary;


            conversationData.Add(tempNotification);
            var vocabRoot = VocabDetails.Word;
            var vocabDesc = VocabDetails.Word;
            if (VocabDetails.Details.Count > 0)
            {
                vocabDesc = string.Concat(new string[] { VocabDetails.Word, ":", " ",VocabDetails.Details[0].Definition });
                FP_OverlayNotification internalNotification = ScriptableObject.CreateInstance<FP_OverlayNotification>();
                internalNotification.OverlayStatus = false;
                internalNotification.OverlayDuration = 4;
                internalNotification.OverlayObjective= vocabDesc;
                internalNotification.OverlayTaskType= OverlayType.Vocabulary;
                conversationData.Add(internalNotification);
            }
            FP_Dialogue tempDialogue = ScriptableObject.CreateInstance<FP_Dialogue>();
            FP_Character tempChar = ScriptableObject.CreateInstance<FP_Character>();
            tempDialogue.AutoScrollConversation = true;
            tempChar.CharacterGender = CC_Gender.Robot;
            tempChar.CharacterTheme = displayDetails;
            tempChar.CharacterName = "Vocab Word";
            tempDialogue.Character = tempChar;
            tempDialogue.TheDialogueType = FP_DialogueType.Linear;
            tempDialogue.ConversationData = conversationData;
            
            StartDialogueModel(tempDialogue);
        }
        /*
        /// <summary>
        /// Using the base dialogue block we can take the card data and present it here in a front/back scenario
        /// </summary>
        /// <param name="CardDetails"></param>
        public void StartCardDialogueConversion(FP_CoderCards CardDetails)
        {
            //convert to a CC_Dialogue instance via procedural and just following standard 'conversation details'
            
            var conversationData = new List<FP_OverlayNotification>();
            FP_OverlayNotification tempNotificationFront = ScriptableObject.CreateInstance<FP_OverlayNotification>();
            tempNotificationFront.OverlayStatus = false;
            tempNotificationFront.OverlayDuration = 3f;
            tempNotificationFront.OverlayObjective = CardDetails.Description;
            tempNotificationFront.OverlayTaskType = OverlayType.CardDetails;
            tempNotificationFront.OverlayVisualData = CardDetails.CardPrefabRef;

            conversationData.Add(tempNotificationFront);

            FP_OverlayNotification tempNotificationBack = ScriptableObject.CreateInstance<FP_OverlayNotification>();
            tempNotificationBack.OverlayStatus = false;
            tempNotificationBack.OverlayDuration = 3f;
            tempNotificationBack.OverlayObjective = CardDetails.BackCardDescription;
            tempNotificationBack.OverlayTaskType = OverlayType.CardDetails;
            tempNotificationBack.OverlayVisualData = CardDetails.CardPrefabRef;

            conversationData.Add(tempNotificationBack);
            
            Camera camera = PlayerCamera;
            FP_Dialogue tempDialogue = ScriptableObject.CreateInstance<FP_Dialogue>();
            FP_Character tempChar = ScriptableObject.CreateInstance<FP_Character>();
            tempDialogue.AutoScrollConversation = false;
            tempChar.CharacterGender = CC_Gender.Robot;
            tempChar.CharacterTheme = CardDetails.CardTheme;
            tempChar.CharacterName = CardDetails.NameOfCard;
            tempDialogue.Character = tempChar;
            tempDialogue.TheDialogueType = FP_DialogueType.Linear;
            tempDialogue.ConversationData = conversationData;
            //https://docs.unity3d.com/ScriptReference/Camera.ViewportToWorldPoint.html
            FP_Location aLoc = new FP_Location();
            aLoc.WorldLocation = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, camera.nearClipPlane+.3f))+new Vector3(0,0.04f,0) ;
            var lookDirection = camera.transform.forward;
            var quaternionRotation = Quaternion.LookRotation(lookDirection);
            aLoc.EulerRotation = quaternionRotation.eulerAngles;
            tempDialogue.ItemPositionSpawn = aLoc;
            
            StartDialogueModel(tempDialogue);
        }
        */
        /// <summary>
        /// A method to compare all vocabulary by all modules
        /// We then replace the dialogue conversation bits with underline matching vocab words
        /// 
        /// </summary>
        /// <param name="convoDetails"></param>
        private void CheckVocabulary(ref FP_Dialogue convoDetails)
        {
            
            for(int i = 0; i < convoDetails.ConversationData.Count; i++)
            {
                var cDetails = convoDetails.ConversationData[i];
                string text = cDetails.OverlayObjective;
                string[] words = text.Split(' ');
                for(int a = 0; a < words.Length; a++)
                {
                    var word = words[a];
                    word = word.TrimEnd('s', ':', '.');
                    FP_Vocab matchingVocab = _moduleVocabReference.Find(vocab => vocab.Word.Equals(word, StringComparison.OrdinalIgnoreCase));
                    if (matchingVocab != null)
                    {
                        string pattern = @"\b" + word + @"s?:?(:|.)?\b";
                        text = Regex.Replace(text, pattern, "<u>$&</u>");
                        cDetails.OverlayObjective = text;
                    }
                }
                
            }
        }
        /// <summary>
        /// this will take a conversation Scriptable Object probably coming in from a sequence event associated with station progression. 
        /// Could also come in from a user event trigger and/or CC_Manager
        /// </summary>
        /// <param name="ConversationDetails"></param>
        public void StartDialogueModel(FP_Dialogue ConversationDetails) 
        {
            //reset
            ResetDialogueModel();
            //setup our conversation details for our character displays
            CheckVocabulary(ref ConversationDetails);
            _currentDialogue = ConversationDetails;
            DialogueButtonsContainer.style.backgroundColor = _currentDialogue.Character.CharacterTheme.MainColor;
            CharacterIconContainer.style.backgroundImage = new StyleBackground(_currentDialogue.Character.CharacterTheme.Icon);
            CharacterIconContainer.style.backgroundColor = _currentDialogue.Character.CharacterTheme.MainColor;
            CharacterIconContainer.style.borderBottomColor = _currentDialogue.Character.CharacterTheme.SecondaryColor;
            CharacterIconContainer.style.borderLeftColor = _currentDialogue.Character.CharacterTheme.SecondaryColor;
            CharacterIconContainer.style.borderTopColor = _currentDialogue.Character.CharacterTheme.SecondaryColor;
            CharacterIconContainer.style.borderRightColor = _currentDialogue.Character.CharacterTheme.SecondaryColor;
            CharacterName.style.backgroundColor = _currentDialogue.Character.CharacterTheme.MainColor;
            CharacterName.text = _currentDialogue.Character.CharacterName;
            CharacterName.style.color = _currentDialogue.Character.CharacterTheme.SecondaryColor;
            //setup progress conversation bar values and colors
            DialogueProgressBarContainer.style.backgroundColor = _currentDialogue.Character.CharacterTheme.SecondaryColor;
            DialogueProgressBarContainer.style.unityBackgroundImageTintColor = _currentDialogue.Character.CharacterTheme.MainColor;
            DialogueProgressValueContainer.style.backgroundColor = _currentDialogue.Character.CharacterTheme.MainColor;
            UpdateDialogueProgressBar(_dialogueIndex);

            //setup our nested group messages
            for (int i = 0; i < _currentDialogue.ConversationData.Count; i++)
            {
                var curData = _currentDialogue.ConversationData[i];
                VisualElement tempElement = new VisualElement();
                tempElement.style.backgroundColor = _currentDialogue.Character.CharacterTheme.MainColor;
                tempElement.AddToClassList(DialogueTextClass);
                tempElement.style.whiteSpace = WhiteSpace.Normal;
                var textLabel = new Label(curData.OverlayObjective);
                textLabel.AddToClassList(DialogueTextClassBold);
                //textLabel.style.flexWrap = Wrap.Wrap;
                //textLabel.style.textOverflow = TextOverflow.Ellipsis;
                textLabel.style.color = _currentDialogue.Character.CharacterTheme.SecondaryColor;
                textLabel.style.whiteSpace = WhiteSpace.NoWrap;
                textLabel.style.whiteSpace = WhiteSpace.Normal;
                tempElement.Add(textLabel);
                //hide it if it's not the first one
                if (i != 0)
                {
                    tempElement.AddToClassList(HideClass);
                }
                DialogueContainer.Add(tempElement);
                DialoguePlaceholderList.Add(tempElement);
                //if its a card we want to show the object if we have a reference to it in close
                if(curData.OverlayTaskType == OverlayType.CardDetails)
                {
                    if (curData.OverlayVisualData != null)
                    {
                        var referenceObject = GameObject.Instantiate(curData.OverlayVisualData, _currentDialogue.ItemPositionSpawn.WorldLocation, Quaternion.Euler(_currentDialogue.ItemPositionSpawn.EulerRotation));
                        if (i != 0)
                        {
                            referenceObject.SetActive(false);
                            var flipRotation = Quaternion.Euler(_currentDialogue.ItemPositionSpawn.EulerRotation) * Quaternion.Euler(new Vector3(0, 180, 0));
                            referenceObject.transform.rotation = flipRotation;
                        }
                        spawnedVisuals.Add(referenceObject);
                    }
                }
                //add in other items we want to spawn for conversation purposes - maybe like an audio file we want to play
                if (curData.OverlayTaskType == OverlayType.Conversation)
                {
                    if (curData.OverlayVisualData != null)
                    {
                        var referenceObject = GameObject.Instantiate(curData.OverlayVisualData, _currentDialogue.ItemPositionSpawn.WorldLocation, Quaternion.Euler(_currentDialogue.ItemPositionSpawn.EulerRotation));
                        referenceObject.SetActive(false);
                        spawnedVisuals.Add(referenceObject);
                    }
                }
            }
            
            
            if (_currentDialogue.AutoScrollConversation)
            {
                //hide all the buttons
                AddNewStyleToVisualElement(DialogueButtonsContainer, FlexHideClass);
                //AddNewStyleToVisualElement(OverlayDialoguePreviousButton.Q<VisualElement>(), FlexHideClass);
                //AddNewStyleToVisualElement(OverlayDialogueNextButton.Q<VisualElement>(), FlexHideClass);
                //reset our clock
                _dialogueRunningTimer = 0;
                //grab our first time event from our first conversation item in list
                _dialogueTimer = _currentDialogue.ConversationData[0].OverlayDuration;
                //need to adjust our absolute position information on the right edge to extend pass the buttons
                //could rework some of our absolute use cases and make this automatically 'work' but we would have to
                //have harder requirements on the contained items and how they flow style up
                DialogueContainer.style.right = Length.Percent(0);
                DialogueProgressBarContainer.style.right = Length.Percent(0);
                //start the sequence
                _continuousDialogueActive = true;
            }
            else
            {
                RemoveStyleToVisualElement(DialogueButtonsContainer, FlexHideClass);
                DialogueContainer.style.right = Length.Percent(20);
                DialogueProgressBarContainer.style.right = Length.Percent(20);
                //hide the previous button if we are using the buttons
                AddNewStyleToVisualElement(OverlayDialoguePreviousButton.Q<VisualElement>(), FlexHideClass);
                if (_currentDialogue.ConversationData[_dialogueIndex].OverlayTaskType == OverlayType.Conversation)
                {
                    if (spawnedVisuals.Count > _dialogueIndex)
                    {
                        var spawnedItem = spawnedVisuals[_dialogueIndex];
                        if (spawnedItem.GetComponent<AudioSource>())
                        {
                            spawnedItem.SetActive(true);
                            spawnedItem.GetComponent<AudioSource>().Play();
                        }
                    }
                }
            }
            DelegateDialogueStartEvent?.Invoke(_currentDialogue.ConversationData[0]);
        }
        /// <summary>
        /// Generic reset for all things data and dialogue
        /// </summary>
        private void ResetDialogueModel()
        {
            //clear out existing dialogue data if there is any
            for (int i = 0; i < DialoguePlaceholderList.Count; i++)
            {
                var VE = DialoguePlaceholderList[i];
                if (DialogueContainer.Contains(VE))
                {
                    DialogueContainer.Remove(VE);
                }
            }
            DialoguePlaceholderList.Clear();

            //have to hide the other overlays
            AddNewStyleToVisualElement(OverlayContainer, HideClass);
            AddNewStyleToVisualElement(OverlayPopUpContainer, HideClass);
            //we want to show the overlay for our dialogue
            RemoveStyleToVisualElement(OverlayDialogueContainer, HideClass);
            //reset buttons and show them both
            RemoveStyleToVisualElement(OverlayDialoguePreviousButton.Q<VisualElement>(), FlexHideClass);
            RemoveStyleToVisualElement(OverlayDialogueNextButton.Q<VisualElement>(), FlexHideClass);
            //have to activate our correct overlay
            _dialogueIndex = 0;
        }
            
        /// <summary>
        /// works for all 'next' dialogue actions and accounts for AutoScrolling situations
        /// </summary>
        public void DialogueNextButtonAction()
        {
            //hide current dialogue container and check the next one only if not auto scrolling
            if (_dialogueIndex == 0 && !_currentDialogue.AutoScrollConversation)
            {
                //this unhides the button
                RemoveStyleToVisualElement(OverlayDialoguePreviousButton.Q<VisualElement>(), FlexHideClass);
            }
            if (_dialogueIndex+1 < DialoguePlaceholderList.Count)
            {
                //3D spawned item
                if (_currentDialogue.ConversationData[_dialogueIndex].OverlayTaskType == OverlayType.CardDetails)
                {
                    spawnedVisuals[_dialogueIndex].SetActive(false);
                    spawnedVisuals[_dialogueIndex+1].SetActive(true);
                }
                if (_currentDialogue.ConversationData[_dialogueIndex].OverlayTaskType == OverlayType.Conversation)
                {
                    if (spawnedVisuals.Count > _dialogueIndex)
                    {
                        var spawnedItem = spawnedVisuals[_dialogueIndex+1];
                        var previousItem = spawnedVisuals[_dialogueIndex];
                        if (previousItem.GetComponent<AudioSource>())
                        {
                            previousItem.SetActive(false);
                        }
                        if (spawnedItem.GetComponent<AudioSource>())
                        {
                            spawnedItem.SetActive(true);
                            spawnedItem.GetComponent<AudioSource>().Play();
                        }
                    }
                }
                //hide the last dialogue container then update to the next one and show it off
                AddNewStyleToVisualElement(DialoguePlaceholderList[_dialogueIndex], HideClass);
                _dialogueIndex++;
                RemoveStyleToVisualElement(DialoguePlaceholderList[_dialogueIndex], HideClass);

                if (!_currentDialogue.AutoScrollConversation)
                {
                    //not autoscrolling
                    NextButtonPushed.Invoke();
                }
                else
                {
                    //autoscrolling
                    _dialogueRunningTimer = 0;
                    _dialogueTimer = _currentDialogue.ConversationData[_dialogueIndex].OverlayDuration;
                }
                UpdateDialogueProgressBar(_dialogueIndex);
                DialogueNextButtonEvent?.Invoke(_currentDialogue.ConversationData[_dialogueIndex]);
            }
            else
            {
                Debug.LogWarning($"Dialogue Finished!");
                //reset 3D visuals
                foreach (var anItem in spawnedVisuals)
                {
                    Destroy(anItem);
                }
                spawnedVisuals.Clear();

                _continuousDialogueActive = false;
                DialogueFinishedEvent.Invoke();
                _dialogueIndex = 0;
                //reset Dialogue and close it out and re-open our other overlays
                //only display our OverlayContainer if we have objectives we're working on
                if (objectiveList.Count > 0)
                {
                    RemoveStyleToVisualElement(OverlayContainer, HideClass);
                }
                //RemoveStyleToVisualElement(OverlayContainer, HideClass);
                AddNewStyleToVisualElement(OverlayDialogueContainer, HideClass);
                //callbacks can be processed here
                if (_passedEvent != null)
                {
                    _passedEvent.PassMyEvent();
                    _passedEvent = null;
                }
                DelegateDialogueFinishedEvent?.Invoke(_currentDialogue.ConversationData[_dialogueIndex]);
            }
            
            
        }
        
        public void PassEndDialogueEvents(FP_PassedEvent passedEvent)
        {
            _passedEvent = passedEvent;
        }
        
        public void DialoguePreviousButtonAction()
        {
            if (_dialogueIndex - 1 >= 0)
            {
                //3D spawned item
                if(_currentDialogue.ConversationData[_dialogueIndex].OverlayTaskType == OverlayType.CardDetails)
                {
                    spawnedVisuals[_dialogueIndex].SetActive(false);
                    spawnedVisuals[_dialogueIndex-1].SetActive(true);
                }
                if (_currentDialogue.ConversationData[_dialogueIndex].OverlayTaskType == OverlayType.Conversation)
                {
                    if ((spawnedVisuals.Count > _dialogueIndex) && (_dialogueIndex>0))
                    {
                        var spawnedItem = spawnedVisuals[_dialogueIndex - 1];
                        var previousItem = spawnedVisuals[_dialogueIndex];
                        if (spawnedItem.GetComponent<AudioSource>())
                        {
                            spawnedItem.SetActive(true);
                            spawnedItem.GetComponent<AudioSource>().Play();
                        }
                        if (previousItem.GetComponent<AudioSource>())
                        {
                            previousItem.SetActive(false);
                        }
                    }
                }
                //hide the last dialogue container
                AddNewStyleToVisualElement(DialoguePlaceholderList[_dialogueIndex], HideClass);
                _dialogueIndex--;
                RemoveStyleToVisualElement(DialoguePlaceholderList[_dialogueIndex], HideClass);
                
                PreviousButtonPushed.Invoke();
                DialoguePreviousButtonEvent?.Invoke(_currentDialogue.ConversationData[_dialogueIndex]);
            }
            else
            {
                //at the beginning
                _dialogueIndex = 0;  
            }
            //hide previous button
            if (_dialogueIndex == 0)
            {
                AddNewStyleToVisualElement(OverlayDialoguePreviousButton.Q<VisualElement>(), FlexHideClass);
            }
            //progress bar
            UpdateDialogueProgressBar(_dialogueIndex);
        }

        public void AddNewOverlayData(FP_OverlayNotification overlayDetails)
        {
            switch (overlayDetails.OverlayTaskType)
            {
                case OverlayType.TaskList:
                    if (!objectiveList.ContainsKey(overlayDetails.OverlayObjective))
                    {
                        Debug.LogWarning($"Task List has a new Item: {overlayDetails.OverlayObjective}");
                        Toggle newT = new Toggle(overlayDetails.OverlayObjective);
                        newT.AddToClassList(ToggleFontClass);
                        newT.AddToClassList(ToggleFontClassList);
                        objectiveList.Add(overlayDetails.OverlayObjective, newT);
                        ObjectiveGroupBox.Add(newT);
                    }
                    else
                    {
                        var allChildren = ObjectiveGroupBox.Children().ToList();
                        Debug.LogWarning($"Toggle Children in Objective Group Box Count:{allChildren.Count}");
                        for (int i = 0; i < allChildren.Count; i++)
                        {
                            Toggle curVisualELement = (Toggle)allChildren[i];
                            if(curVisualELement.label == overlayDetails.OverlayObjective)
                            {
                                Debug.LogWarning($"Found Toggle and changed the status:{overlayDetails.OverlayObjective}");
                                curVisualELement.value = true;
                            }
                        }
                    }
                    RemoveStyleToVisualElement(OverlayContainer,HideClass);
                    break;
                case OverlayType.Information:
                    ///only want to add these items if we don't have them in our list
                    ///highlight object could result in multiple events fast and we don't want to flood this
                    if (!overlayPopUpUI.Contains(overlayDetails))
                    {
                        if (overlayDetails.DelayBeforeOverlay > 0)
                        {
                            StartCoroutine(DelayOverlayPopUpUI(overlayDetails));
                        }
                        else
                        {
                            overlayPopUpUI.Add(overlayDetails);
                            RemoveStyleToVisualElement(OverlayPopUpContainer, HideClass);
                        }
                    }
                    //_runningOverlayPopUps = true;
                    break;
            }
        }
        IEnumerator DelayOverlayPopUpUI(FP_OverlayNotification overlayDetails)
        {
            yield return new WaitForSecondsRealtime(overlayDetails.DelayBeforeOverlay);
            overlayPopUpUI.Add(overlayDetails);
            RemoveStyleToVisualElement(OverlayPopUpContainer, HideClass);
        }
        /// <summary>
        /// public accessors for internal protected class
        /// thgis will add a style from an associated container
        /// </summary>
        public void HideOverlayUI()
        {
            AddNewStyleToVisualElement(OverlayContainer, HideClass);
        }
        /// <summary>
        /// public accessors for itnernal protected class
        /// this will remove a style from an associated container
        /// </summary>
        public void UnhideOverlayUI()
        {
            RemoveStyleToVisualElement(OverlayContainer, HideClass);
        }
        private void UpdateDialogueProgressBar(int indexValue)
        {
            float ratioConversation = (1 - ((indexValue + 1) / (_currentDialogue.ConversationData.Count * 1f)));
            DialogueProgressValueContainer.style.position = Position.Absolute;
            
            DialogueProgressValueContainer.style.left = Length.Percent(0);
            DialogueProgressValueContainer.style.top = Length.Percent(0);
            DialogueProgressValueContainer.style.bottom = Length.Percent(0);
            Debug.LogWarning($"Right Progress Bar value: {ratioConversation*100}");
            DialogueProgressValueContainer.style.right = Length.Percent(ratioConversation * 100); //this is the same as saying our progress bar to 1/n conversation details
        }
        /// <summary>
        /// Will remove all task/toggles from OverlayObjective
        /// </summary>
        public void ResetTaskList()
        {
            var allChildren = ObjectiveGroupBox.Children().ToList();
            foreach(var aChild in allChildren)
            {
                ObjectiveGroupBox.Remove(aChild);
            }
            objectiveList.Clear();
            
            AddNewStyleToVisualElement(OverlayContainer,HideClass);
        }
        /*
        public void DebugMousePositionUpdate(FP_InputUIDebugData debugData)
        {
            if (DebugOn)
            {
                MouseLabelDebug.text = "I:(" + debugData.Movement.x.ToString() + ", " + debugData.Movement.y.ToString() + "), M:(" +debugData.MouseCor.x.ToString("F2")+", "+debugData.MouseCor.y.ToString("F2")+")";
            }
        }
        */
        public void Update()
        {
            //overlay dialogue timer
            if (_continuousDialogueActive)
            {
                //run through timing loop and process
                if (_dialogueRunningTimer >= _dialogueTimer)
                {
                    DialogueNextButtonAction();
                }
                _dialogueRunningTimer += Time.deltaTime;
            }
            //overlay pop-up timer
            if (overlayPopUpUI.Count>0)
            {
                //pop one off
                var curPop = overlayPopUpUI[0];
                if (currentOverlay == null)
                {
                    currentOverlay = curPop;
                    OverlayPopUpInformationLabel.text = currentOverlay.OverlayObjective;
                }
                else
                {
                    if (currentOverlay == curPop)
                    {
                        //still on it
                        if (_runTime >= currentOverlay.OverlayDuration)
                        {
                            //remove it from the list
                            currentOverlay = null;
                            if (overlayPopUpUI.Count > 0)
                            {
                                overlayPopUpUI.RemoveAt(0);
                            }
                            OverlayPopUpInformationLabel.text = "";
                            if (overlayPopUpUI.Count == 0)
                            {
                                AddNewStyleToVisualElement(OverlayPopUpContainer,HideClass);
                            }
                            _runTime = 0;
                        }
                        else
                        {
                            _runTime += Time.deltaTime;
                        }
                        
                    }
                }
            }
        }
    }
}
