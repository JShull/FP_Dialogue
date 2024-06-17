using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

namespace FuzzPhyte.Dialogue
{
    //JOHN
    // still need to work up how we activate and deactivate
    // progress bar needs to be updated
    // parent of user response buttons isn't right
    // this will probably be by event/proximity
    // so if we leave from a conversation in the middle we can come back to that point
    // also need a way to activate the actions on the UIDialogueBase class to start playing audio etc.
    //Mono class to hold all of our data and various connections for runtime needs of passing that data around via the FP_Dialogue_Manager
    public class DialogueUnity : MonoBehaviour
    {
        [Tooltip("The core data for this dialogue")]
        public DialogueBase MainDialogueData;
        public bool TestingData;
        public int DialogueIndex = 0;
        public RectTransform DialogueContainer;
        [SerializeField]
        private Canvas canvasRef;
        [SerializeField]
        private string clientID;
        public string ClientID { get { return clientID; } }
        [Tooltip("The prefab to spawn for the UI dialogue block with content references as needed")]
        public GameObject UIDialoguePrefab;
        private UIDialogueBase uiDialogueRef;
        private bool dialogueActive = false;

        #region Delegate Setup
        public delegate void DialogueEventHandler(DialogueEventData eventData);
        public event DialogueEventHandler OnDialogueSetup;
        public event DialogueEventHandler OnDialogueStart;
        public event DialogueEventHandler OnDialogueNext;
        public event DialogueEventHandler OnDialoguePrevious;
        public event DialogueEventHandler OnDialogueUserPrompt;
        public event DialogueEventHandler OnDialogueEnd;
        #endregion

        #region Unity Methods - generally for Testing
        public void Awake()
        {
            //testing
            
            if(TestingData&&MainDialogueData != null)
            {
                SetupDialogue(canvasRef, clientID);
            }
            
        }
        public void Update()
        {
            if (TestingData&&Input.GetKeyUp(KeyCode.Space) && !dialogueActive)
            {
                dialogueActive = true;
                ActivateDialogue();
            }
        }
        #endregion
        //Assuming we are starting from the beginning of the conversation data block inside the DialogueBase object
        public void SetupDialogue(Canvas theCanvasToUse,string userID)
        {
            //setup all spawnable UI items and cache them
            if (!TestingData)
            {
                canvasRef = theCanvasToUse;
                clientID = userID;
            }
            DialogueIndex = 0;
            if (DialogueContainer == null) { DialogueContainer = canvasRef.GetComponent<RectTransform>();}
            //spawn my initial UI item and populate it with the first batch of data using the DialogueBase object data and then turn it off as we aren't activated yet  
            var blockUI = Instantiate(UIDialoguePrefab, DialogueContainer);
            //this blockUI should be the full size of the canvas via rectTransform adjustments
            //adjust rectTransform to fit the canvas

            blockUI.GetComponent<RectTransform>().localPosition = Vector3.zero;
            blockUI.GetComponent<RectTransform>().localRotation = Quaternion.identity;
            blockUI.GetComponent<RectTransform>().localScale = Vector3.one;
            blockUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            blockUI.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            blockUI.GetComponent<RectTransform>().anchorMax = Vector2.one;
            blockUI.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            
            if (!blockUI.GetComponent<UIDialogueBase>())
            {
                Debug.LogError($"We are missing a major component for the UIDialoguePrefab: {UIDialoguePrefab.name}");
                return;
            }
            uiDialogueRef = blockUI.GetComponent<UIDialogueBase>();
            uiDialogueRef.SetupDialoguePanel(MainDialogueData.Character, MainDialogueData.ConversationData[DialogueIndex], this);
            OnDialogueSetup?.Invoke(new DialogueEventData()
            {
                UserID = clientID,
                DialogueDataRef = MainDialogueData,
                DialogueBlockDataRef = MainDialogueData.ConversationData[DialogueIndex]
            });
            //turn off the UI pieces for now
            DialogueContainer.gameObject.SetActive(false);
            if (canvasRef != null)
            {
                canvasRef.enabled = false;
            }
            

        }
        /// <summary>
        /// called via some external event based maybe on proximity and/or the manager
        /// </summary>
        public void ActivateDialogue()
        {
            //show the UI panel and/or unhide it and then "start it"
            if (canvasRef != null)
            {
                canvasRef.enabled = true;
            }
            DialogueContainer.gameObject.SetActive(true);
            OnDialogueStart?.Invoke(new DialogueEventData()
            {
                UserID = clientID,
                DialogueDataRef = MainDialogueData,
                DialogueBlockDataRef = MainDialogueData.ConversationData[DialogueIndex]
            });
            uiDialogueRef.PlayDialogueBlock();
        }
        public void UINextDialogueAction()
        {
            if (NextDialogueAvailable())
            {
                DialogueIndex++;
                uiDialogueRef.SetupDialoguePanel(MainDialogueData.Character, MainDialogueData.ConversationData[DialogueIndex], this);
                OnDialogueNext?.Invoke(new DialogueEventData()
                {
                    UserID = clientID,
                    DialogueDataRef = MainDialogueData,
                    DialogueBlockDataRef = MainDialogueData.ConversationData[DialogueIndex]
                });
            }
            uiDialogueRef.PlayDialogueBlock();
        }
        public void UIPreviousDialogueAction()
        {
            if (PreviousDialogueAvailable())
            {
                DialogueIndex--;
                uiDialogueRef.SetupDialoguePanel(MainDialogueData.Character, MainDialogueData.ConversationData[DialogueIndex], this);
                OnDialoguePrevious?.Invoke(new DialogueEventData()
                {
                    UserID = clientID,
                    DialogueDataRef = MainDialogueData,
                    DialogueBlockDataRef = MainDialogueData.ConversationData[DialogueIndex]
                });
            }
            uiDialogueRef.PlayDialogueBlock();
        }
        public void UIFinishDialogueAction()
        {
            //turn off the UI and send a message to the manager that we are done
            
            DialogueContainer.gameObject.SetActive(false);
            if (canvasRef != null)
            {
                canvasRef.enabled = false;
            }
            OnDialogueEnd?.Invoke(new DialogueEventData()
            {
                UserID = clientID,
                DialogueDataRef = MainDialogueData,
                DialogueBlockDataRef = MainDialogueData.ConversationData[DialogueIndex]
            });
            //reset data parameters to loop back over based on testing
            if (TestingData)
            {
                DialogueIndex = 0;
            }
            dialogueActive = false;
        }
        public void UIUserPromptAction(DialogueUserResponse userResponse)
        {
            //this is a user prompt, we need to wait for a user input to continue
            //will have to filter through the data to see where to go next
            OnDialogueUserPrompt?.Invoke(new DialogueEventData()
            {
                UserID = clientID,
                DialogueDataRef = MainDialogueData,
                DialogueBlockDataRef = MainDialogueData.ConversationData[DialogueIndex],
                PotentialUserResponse = userResponse
            });
        }
        
        #region Stubouts for Dialogue Status and Navigation
        public bool PreviousDialogueAvailable()
        {
            return DialogueIndex > 0;
        }
        public bool NextDialogueAvailable()
        {
            return DialogueIndex < MainDialogueData.ConversationData.Count - 1;
        }
        public bool LastDialogue()
        {
            return DialogueIndex == MainDialogueData.ConversationData.Count - 1;
        }
        #endregion

    }
}
