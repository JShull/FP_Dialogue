namespace FuzzPhyte.Dialogue
{
    using UnityEngine.UI;
    using FuzzPhyte.Utility;

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
            if(dialogueBase != null)
            {
                userPromptButton = true;
                userDataResponse = userResponse;
                dialogueDataBase = dialogueBase;
                TheButton.onClick.RemoveAllListeners();
                TheButton.onClick.AddListener(UserResponseAction);
            }
        }
        public void SetupNextButton(UIDialogueBase dialogueBase = null)
        {
            if (dialogueBase != null)
            {
                nextButton = true;
                dialogueDataBase = dialogueBase;
                TheButton.onClick.RemoveAllListeners();
                TheButton.onClick.AddListener(RefNextButton);
            }
        }

        public void SetupPreviousButton(UIDialogueBase dialogueBase = null)
        {
            if (dialogueBase != null)
            {
                previousButton = true;
                dialogueDataBase = dialogueBase;
                TheButton.onClick.RemoveAllListeners();
                TheButton.onClick.AddListener(RefPreviousButton);
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
        private void RefNextButton()
        {
            if(dialogueDataBase != null)
            {
                dialogueDataBase.NextButtonAction();
            }
        }
        private void RefPreviousButton()
        {
            if(dialogueDataBase != null)
            {
                dialogueDataBase.PreviousButtonAction();
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
                    TheButton.onClick.RemoveListener(RefNextButton);
                }
                if (previousButton)
                {
                    TheButton.onClick.RemoveListener(RefPreviousButton);
                }
                if (finishButton)
                {
                    TheButton.onClick.RemoveListener(RefFinishButton);
                }
                if (userPromptButton)
                {
                    TheButton.onClick.RemoveListener(UserResponseAction);
                }
            }
        }

        
    }
}
