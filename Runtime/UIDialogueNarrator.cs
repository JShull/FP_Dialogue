namespace FuzzPhyte.Dialogue
{
    using FuzzPhyte.Utility;
    using UnityEngine;
    using UnityEngine.Events;
    /// <summary>
    /// Assumption is a linear dialogue system
    /// we use the data to generate a time based approach to the narration
    /// and we display the text from the underlying system to our renderers
    /// </summary>
    public class UIDialogueNarrator : MonoBehaviour
    {
        public bool DontDestroy = false;
        public bool SetupOnStart = false;
        protected bool isSetup = false;
        public bool IsSetup { get { return isSetup; } }
        public string userID = "123456789";
        public static UIDialogueNarrator Instance { get; private set; }
        [Tooltip("The core data for this narrator to use in populating the narrator system")]
        public DialogueBase NarratorData;
        [SerializeField] protected SequenceStatus dialogueStatus;
        [SerializeField] protected float expectedDuration;
        [SerializeField] protected float currentDialogueTime;
        [SerializeField] protected int DialogueIndex = 0;
        public GameObject UINarratorPrefab;
        protected UINarratorBase uiNarratorRef;
        public RectTransform NarratorContainer;// basically a text box with no text in it to get my rect transform that I need
        [Tooltip("Any other visuals we need to manage?")]
        public GameObject OtherNarratorContainer;
        [Space]
        [Header("Unity Events Tied to Text Display Setup or Not")]
        public UnityEvent OnNarratorSetupWithText;
        public UnityEvent OnNarratorSetupWithoutText;
        //
        #region Delegate Setup
        public delegate void NarratorEventHandler(NarratorEventData eventData);
        public event NarratorEventHandler OnNarratorSetup;
        public event NarratorEventHandler OnNarratorStart;
        public event NarratorEventHandler OnNarratorNext;
        public event NarratorEventHandler OnNarratorEnd;
        #endregion
        //
        public virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                dialogueStatus = SequenceStatus.None;
                if (DontDestroy)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        public virtual void Start()
        {
            if (SetupOnStart)
            {
                if (NarratorData != null)
                {
                    expectedDuration = BuildEstimatedDurationTime(NarratorData, true);
                }
                if (NarratorData != null)
                {
                    SetupNarrator(userID);
                }
            }
        }
       
        /// <summary>
        /// builds an estimate of time based on overlay duration information
        /// it's better to useAudioDuration if you have it
        /// </summary>
        /// <param name="dialogueData"></param>
        protected virtual float BuildEstimatedDurationTime(DialogueBase dialogueData, bool useAudioDuration=true)
        {
            float totalExpectedDuration = 0;
            for(int i=0;i< dialogueData.ConversationData.Count; i++)
            {
                var dialogueBlock = dialogueData.ConversationData[i];
                float originalAudio = 0;
                float translatedAudio = 0;
                int numAvg = 0;
                float avgDialogueAudio = 0;
                if (useAudioDuration)
                {
                    if (dialogueBlock.OriginalLanguage.AudioText.AudioClip)
                    {
                        originalAudio = dialogueBlock.OriginalLanguage.AudioText.AudioClip.length;
                        numAvg++;
                    }
                    if (dialogueBlock.TranslatedLanguage.AudioText.AudioClip)
                    {
                        translatedAudio = dialogueBlock.TranslatedLanguage.AudioText.AudioClip.length;
                        numAvg++;
                    }
                    if (numAvg > 0)
                    {
                        avgDialogueAudio = (originalAudio + translatedAudio) / numAvg * 1f;
                    }
                    else
                    {
                        avgDialogueAudio = 0;
                    }
                }
                else
                {
                    //use text overlay duration
                    avgDialogueAudio = dialogueBlock.OverlayDuration;
                }
                totalExpectedDuration += avgDialogueAudio;
            }
            return totalExpectedDuration;
        }
        /// <summary>
        /// This is a stubout for when we want to change the narrator data
        /// goes through generic setup, can be changed to do more if needed
        /// </summary>
        /// <param name="newNarratorData"></param>
        /// <returns></returns>
        public virtual bool NewNarratorData(DialogueBase newNarratorData, string updatedUserID = "123456789")
        {
            //we aren't in the middle of playing are we?
            //if we are, throw it to our timer and queue it up
            if (dialogueStatus != SequenceStatus.Active)
            {
                //we aren't in an active state
                userID = updatedUserID;
                NarratorData = newNarratorData;
                expectedDuration = BuildEstimatedDurationTime(NarratorData, true);
                SetupNarrator(userID);
                dialogueStatus = SequenceStatus.None;
                return true;
            }
            return false;
        }
        [ContextMenu("Setup testing with USERID")]
        public virtual void TestSetupNarrator()
        {
            SetupNarrator(userID);
        }
        public virtual void SetupNarrator(string userID)
        {
            DialogueIndex = 0;
            if (NarratorContainer == null) { Debug.LogError($"Missing a Rect Transform on our Main Narrator Container");return; }
            //spawn my initial UI item and populate it with the first batch of data using the DialogueBase object data and then turn it off as we aren't activated yet  
            var blockUI = Instantiate(UINarratorPrefab, NarratorContainer);
            //this blockUI should be the full size of the canvas via rectTransform adjustments
            //adjust rectTransform to fit the canvas

            blockUI.GetComponent<RectTransform>().localPosition = Vector3.zero;
            blockUI.GetComponent<RectTransform>().localRotation = Quaternion.identity;
            blockUI.GetComponent<RectTransform>().localScale = Vector3.one;
            blockUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            blockUI.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            blockUI.GetComponent<RectTransform>().anchorMax = Vector2.one;
            blockUI.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

            if (!blockUI.GetComponent<UINarratorBase>())
            {
                Debug.LogError($"We are missing a major component for the UIDialoguePrefab: {UINarratorPrefab.name}");
                return;
            }
            uiNarratorRef = blockUI.GetComponent<UINarratorBase>();
            uiNarratorRef.SetupTextPanel(NarratorData.Character, NarratorData.ConversationData[DialogueIndex], this, NarratorData.AutoScrollConversation, NarratorData.UseJustDialoguePanel);

            InternalNarratorPanelSetup(false);
            //internal event invoke
            OnNarratorSetup?.Invoke(new NarratorEventData()
            {
                UserID = userID,
                DialogueDataRef = NarratorData,
                DialogueBlockDataRef = NarratorData.ConversationData[DialogueIndex]
            });
            //unity events based on text display or not
            if (NarratorData.ConversationData[DialogueIndex].DisplayTextPanels)
            {
                OnNarratorSetupWithText.Invoke();
            }
            else
            {
                OnNarratorSetupWithoutText.Invoke();
            }
            isSetup = true;
        }
        /// <summary>
        /// Stubout for interuption to break/stop the current narration
        /// </summary>
        public virtual void NarratorInterrupt()
        {

        }

        /// <summary>
        /// Stubout for starting the narration - assuming we have data already in
        /// </summary>
        [ContextMenu("Test Narrator, Start Narrator")]
        public virtual void StartNarrator()
        {
            if(NarratorData == null || uiNarratorRef==null)
            {
                Debug.LogError($"Either No Narrator Data to Narrate or we are missing a reference to uiNarratorRef");
                return;
            }
            /// <summary>
            /// called via some external event based maybe on proximity and/or the manager
            /// </summary>

            InternalNarratorPanelSetup(true);
            dialogueStatus = SequenceStatus.Active;
            
            OnNarratorStart?.Invoke(new NarratorEventData()
            {
                UserID = userID,
                DialogueDataRef = NarratorData,
                DialogueBlockDataRef = NarratorData.ConversationData[DialogueIndex]
            });
            uiNarratorRef.PlayDialogueBlock();
        }

        public virtual void EndNarrator()
        {
            OnNarratorEnd?.Invoke(new NarratorEventData()
            {
                UserID = userID,
                DialogueDataRef = NarratorData,
                DialogueBlockDataRef = NarratorData.ConversationData[DialogueIndex]
            });
            dialogueStatus = SequenceStatus.Finished;
            if (NarratorContainer != null)
            {
                NarratorContainer.gameObject.SetActive(false);
            }
        }
        public virtual void UINextDialogueAction()
        {
            if (NextDialogueAvailable())
            {
                DialogueIndex++;
                uiNarratorRef.SetupTextPanel(NarratorData.Character, NarratorData.ConversationData[DialogueIndex], this, NarratorData.AutoScrollConversation, NarratorData.UseJustDialoguePanel);
                InternalNarratorPanelSetup(true);
                OnNarratorNext?.Invoke(new NarratorEventData()
                {
                    UserID = userID,
                    DialogueDataRef = NarratorData,
                    DialogueBlockDataRef = NarratorData.ConversationData[DialogueIndex]
                });
                uiNarratorRef.PlayDialogueBlock();
            }
            else
            {
                //end of the road
                EndNarrator();
            }
        }
        public virtual bool NextDialogueAvailable()
        {
            return DialogueIndex < NarratorData.ConversationData.Count - 1;
        }
        /// <summary>
        /// Core setup functionality that is the same for all situations - assumes data was already setup prior
        /// </summary>
        protected virtual void InternalNarratorPanelSetup(bool showMainNarratorContainer)
        {
            //show the main UI canvas object
            if (NarratorContainer != null)
            {
                NarratorContainer.gameObject.SetActive(showMainNarratorContainer);
            }
            // grab details on if we are showing the panel or not
            var displayText = NarratorData.ConversationData[DialogueIndex].DisplayTextPanels;
            if (OtherNarratorContainer != null)
            {
                OtherNarratorContainer.gameObject.SetActive(displayText);
            }
        }
    }
}
