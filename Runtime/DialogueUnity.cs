using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

namespace FuzzPhyte.Dialogue
{
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

        public void Awake()
        {
            //testing
            
            if(TestingData&&MainDialogueData != null)
            {
                SetupDialogue(canvasRef, clientID);
            }
            
        }
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
            uiDialogueRef.Setup(MainDialogueData.Character, MainDialogueData.ConversationData[DialogueIndex], this);

            //

        }
        public void ActivateDialogue()
        {
            //show whatever UI based on the index
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

        //delegate functions here in this region?
        #region Delegate Functions
        
        #endregion

    }
}
