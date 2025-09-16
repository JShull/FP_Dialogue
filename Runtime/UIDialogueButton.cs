namespace FuzzPhyte.Dialogue
{
    using UnityEngine.UI;
    using FuzzPhyte.Utility;
    using UnityEngine;

    //class primarily handles button actions for the dialogue system

    public class UIDialogueButton : UIDialogueContainer, IDialogueUserResponseButton
    {
        public Button TheButton;
        private bool nextButton = false;
        private bool previousButton = false;
        private bool finishButton = false;
        private bool userPromptButton = false;
        private DialogueResponse userDataResponse;
        private UIDialogueBase dialogueDataBase;

        #region Interface Implementation
        public void SetupUserResponse(DialogueResponse userResponse, UIDialogueBase dialogueBase = null)
        {
            if (dialogueBase != null)
            {
                userPromptButton = true;
                userDataResponse = userResponse;
                dialogueDataBase = dialogueBase;
                TheButton.onClick.RemoveAllListeners();
                TheButton.onClick.AddListener(UserResponseAction);
            }
        }
        public void SetupUserResponse(int index, RTDialogueDirector directorRef=null,string textDisplay="",AudioClip clipToPlay=null,AudioSource clipSource=null)
        {
            userPromptButton = true;
            userDataResponse = null;
            TheButton.onClick.RemoveAllListeners();
            RefText.text = textDisplay;
            if (directorRef != null)
            {
                
                if(clipToPlay!=null &&clipSource!=null)
                {
                    //slight delay plus clip length
                    TheButton.onClick.AddListener(() => directorRef.UserPromptResponse(index,true,clipToPlay.length+1.25f));
                    TheButton.onClick.AddListener(() => clipSource.PlayOneShot(clipToPlay));
                }
                else
                {
                    TheButton.onClick.AddListener(() => directorRef.UserPromptResponse(index));
                }
            }
           
        }
        public void SetupNextButton(UIDialogueBase dialogueBase = null, bool useGraph = false)
        {
            if (dialogueBase != null)
            {
                nextButton = true;
                dialogueDataBase = dialogueBase;
                TheButton.onClick.RemoveAllListeners();
                
                if (useGraph)
                {
                    TheButton.interactable = true;
                    TheButton.onClick.AddListener(() => RefNextButton(true));

                }
                else
                {
                    TheButton.onClick.AddListener(()=>RefNextButton(false));
                }
            }
        }

        public void SetupPreviousButton(UIDialogueBase dialogueBase = null, bool useGraph=false)
        {
            if (dialogueBase != null)
            {
                previousButton = true;
                dialogueDataBase = dialogueBase;
                TheButton.onClick.RemoveAllListeners();
                
                if (useGraph)
                {
                    TheButton.interactable = true;
                    TheButton.onClick.AddListener(() => RefPreviousButton(true));
                }
                else
                {
                    TheButton.onClick.AddListener(()=>RefPreviousButton(false));
                }
                
            }
        }
        public void SetupFinishButton(UIDialogueBase dialogueBase = null)
        {
            if (dialogueBase != null)
            {
                finishButton = true;
                dialogueDataBase = dialogueBase;
                TheButton.onClick.RemoveAllListeners();
                TheButton.onClick.AddListener(RefFinishButton);
            }
        }
        #endregion
        private void RefNextButton(bool useGraph=false)
        {
            if(dialogueDataBase != null)
            {
                dialogueDataBase.NextButtonAction();
                if (useGraph)
                {
                    TheButton.interactable = false;
                    if (dialogueDataBase != null)
                    {
                        if (dialogueDataBase.PreviousButton != null)
                        {
                            dialogueDataBase.PreviousButton.TheButton.interactable = false;
                        }
                    }
                }
            }
        }
        private void RefPreviousButton(bool useGraph = false)
        {
            if(dialogueDataBase != null)
            {
                dialogueDataBase.PreviousButtonAction();
                if (useGraph)
                {
                    TheButton.interactable = false;
                    if (dialogueDataBase != null)
                    {
                        if (dialogueDataBase.NextButton != null)
                        {
                            dialogueDataBase.NextButton.TheButton.interactable = false;
                        }
                    }
                }
            }
        }
        private void RefFinishButton()
        {
            if(dialogueDataBase != null)
            {
                dialogueDataBase.FinishButtonAction();
            }
        }
        private void UserResponseAction()
        {
            if (dialogueDataBase != null)
            {
                dialogueDataBase.UserButtonAction(userDataResponse);
            }
        }
        public void OnDestroy()
        {
            if (dialogueDataBase != null)
            {
                if (nextButton)
                {
                  TheButton.onClick.RemoveAllListeners();
                }
                if (previousButton)
                {
                    TheButton.onClick.RemoveAllListeners();
                }
                if (finishButton)
                {
                    TheButton.onClick.RemoveAllListeners();
                }
                if (userPromptButton)
                {
                    TheButton.onClick.RemoveAllListeners();
                }
            }
        }

        
    }
}
