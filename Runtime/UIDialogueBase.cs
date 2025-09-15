namespace FuzzPhyte.Dialogue
{ 
    using UnityEngine;
    using System.Linq;
    using FuzzPhyte.Utility;

    /// <summary>
    /// Mainly a class to connect runtime objects/data and generation of content based on that information
    /// </summary>
    public class UIDialogueBase :MonoBehaviour, IDialogueActions
    {
        public UIDialogueButton NextButton;
        public UIDialogueButton PreviousButton;
        public UIDialogueButton FinishButton;
        public UIDialogueButton HelpButton; //JOHN need to add this and have a way to pass an action to bind it based on 'help' data
        [Tooltip("Generic GameObject for user prompts")]
        public UIDialogueButton UserPromptButtonPrefab;
        public RectTransform UserPromptButtonParentContainer;
        
        [Space]
        [Header("Main Container")]
        [Tooltip("Main container for all the dialogue elements")]
        public UIDialogueContainer MainContainer;
        
        [Space]
        [Header("Other Containers")]
        public UIDialogueContainer ProgressBarContainer;
        #region Specific Containers for standard UI needs
        [Tooltip("Dialogue & Header text for the dialogue")]
        public UIDialogueContainer DialogueTextContainer;
        [Tooltip("Character image and text container")]
        public UIDialogueContainer CharacterContainer;
        
        [Space]
        [Header("Misc.")]
        public AudioSource DialogueAudioSource;
        #endregion
        private DialogueUnity dialogueLocalManager;

        public void SetupResponsePanel(DialogueUnity fullDialogueData,RTResponseNode responseNode,RTFPNode previousNode = null, RTFPNode nextNode = null)
        {
            dialogueLocalManager = fullDialogueData;
            if (responseNode.character != null)
            {
                if (responseNode.character.characterData != null)
                {
                    SetupCharacterContainer(responseNode.character.characterData);
                }
                else
                {
                    Debug.LogError($"Haven't implemented hard coded Character information yet - need to add a way to do this");
                }
            }
            // user prompt setup
            //if it has a user response
            if (responseNode.userIncomingPrompts.Count> 0)
            {
                ChangeUserResponseFormat();
                for (int i = 0; i < responseNode.userIncomingPrompts.Count; i++)
                {
                    var response = responseNode.userIncomingPrompts[i];
                    var userPrompt = Instantiate(UserPromptButtonPrefab.gameObject, UserPromptButtonParentContainer);
                    if (userPrompt.GetComponent<UIDialogueButton>())
                    {
                        var setupCode = userPrompt.GetComponent<UIDialogueButton>();
                        setupCode.SetupUserResponse(i,dialogueLocalManager.DialogueDirectorRef);
                        setupCode.UpdateReferenceText(responseNode.userIncomingPrompts[i].mainDialogue.dialogueText);
                        setupCode.UpdateRefIconSprite(responseNode.userIncomingPrompts[i].promptIcon);
                        //need to create the button onClick event and probably notify DialogueUnity here by creating and passing a DialogueEventData object

                    }
                }
            }
            if (previousNode == null)
            {
                PreviousActionAvailability(false);
            }
            else
            {
                if (previousNode is RTDialogueNode || previousNode is RTResponseNode)
                {
                    PreviousActionAvailability(true);
                }
                else
                {
                    PreviousActionAvailability(false);
                }
            }
            // turn off user forward/backward for now
            
            //audio setup
            //audio clip setup
            if (DialogueAudioSource.isPlaying)
            {
                DialogueAudioSource.Stop();
            }
        }
        public void SetupDialoguePanel(DialogueUnity fullDialogueData, RTDialogueNode nodeData, RTFPNode previousNode = null, RTFPNode nextNode = null)
        {
            dialogueLocalManager = fullDialogueData;
            if (nodeData.incomingCharacter != null)
            {
                if (nodeData.incomingCharacter.characterData != null)
                {
                    SetupCharacterContainer(nodeData.incomingCharacter.characterData);
                }
                else
                {
                    Debug.LogError($"Haven't implemented hard coded Character information yet - need to add a way to do this");
                }
            }
            // text setup
            DialogueTextContainer.UpdateReferenceText(nodeData.mainDialogue.dialogueText);
            if (nodeData.mainDialogue.headerText != string.Empty)
            {
                DialogueTextContainer.UpdateHeaderText(nodeData.mainDialogue.headerText);
            }
            // turn off user forward/backward for now
            ChangeUserResponseFormat();
            if (previousNode == null)
            {
                PreviousActionAvailability(false);
            }
            else
            {
                if (previousNode is RTDialogueNode || previousNode is RTResponseNode)
                {
                    PreviousActionAvailability(true);
                }
                else
                {
                    PreviousActionAvailability(false);
                }
            }
            if (nextNode == null)
            {
                if (nodeData.waitforUser)
                {
                    NextActionAvailability(true);
                }
                else
                {
                    NextActionAvailability(false);
                }
            }
            else
            {
                //what if we want to wait for our user?
                if (nodeData.waitforUser)
                {
                    NextActionAvailability(true);
                }
                else
                {
                    if (nextNode is RTDialogueNode || nextNode is RTResponseNode)
                    {
                        NextActionAvailability(true);
                    }
                    else
                    {
                        NextActionAvailability(false);
                    }
                }
            }
            //audio setup
            //audio clip setup
            if (DialogueAudioSource.isPlaying)
            {
                DialogueAudioSource.Stop();
            }
            // audio clip will have to be driven by the above chosen language as we have two areas to pick from - original /translation

            DialogueAudioSource.clip = nodeData.mainDialogue.textAudio;

        }
        /// <summary>
        /// Main entry poing for setting up the panel
        /// </summary>
        /// <param name="character">Character data</param>
        /// <param name="conversationBlock">block of conversation data</param>
        /// <param name="fullDialogueData">entire dialogue data ref</param>
        public void SetupDialoguePanel(FP_Character character, DialogueBlock conversationBlock, DialogueUnity fullDialogueData)
        {
            dialogueLocalManager = fullDialogueData;
            SetupCharacterContainer(character);

            // Text for Dialogue && Header
            DialogueTextContainer.UpdateReferenceText(conversationBlock.OriginalLanguage.Text);
            if (conversationBlock.OriginalLanguage.Header != string.Empty)
            {
                DialogueTextContainer.UpdateHeaderText(conversationBlock.OriginalLanguage.Header);
            }
            // progress bar
            ProgressBarContainer.UpdateBackdropColor(character.CharacterTheme.TertiaryColor);
            ProgressBarContainer.UpdateRefIconColor(character.CharacterTheme.SecondaryColor);
            var progressBarRatio = dialogueLocalManager.ProgressBarWrapper();
            ProgressBarContainer.UpdateFillAmount(progressBarRatio);
            // default setup - we assume we know nothing and there's a next action
            ChangeNormalResponseFormat();
            // if we have a previous action - we should turn it on/off based on if it exists
            PreviousActionAvailability(dialogueLocalManager.PreviousDialogueAvailable());
            // are we the last and/or user response?
            NextActionAvailability(dialogueLocalManager.NextDialogueAvailable());

            //if it has a user response
            if (conversationBlock.PossibleUserResponses.Count > 0)
            {
                ChangeUserResponseFormat();
                for (int i = 0; i < conversationBlock.PossibleUserResponses.Count; i++)
                {
                    var response = conversationBlock.PossibleUserResponses[i];
                    var userPrompt = Instantiate(UserPromptButtonPrefab.gameObject, UserPromptButtonParentContainer);
                    if (userPrompt.GetComponent<UIDialogueButton>())
                    {
                        var setupCode = userPrompt.GetComponent<UIDialogueButton>();
                        setupCode.SetupUserResponse(response, this);
                        setupCode.UpdateReferenceText(response.ResponseText);
                        setupCode.UpdateRefIconSprite(response.ResponseIcon);
                        //need to create the button onClick event and probably notify DialogueUnity here by creating and passing a DialogueEventData object

                    }
                }
            }
            //audio clip setup
            if (DialogueAudioSource.isPlaying)
            {
                DialogueAudioSource.Stop();
            }
            // audio clip will have to be driven by the above chosen language as we have two areas to pick from - original /translation

            DialogueAudioSource.clip = conversationBlock.OriginalLanguage.AudioText.AudioClip;
        }
        protected void SetupCharacterContainer(FP_Character character)
        {
            var header1Font = character.CharacterTheme.FontSettings.FirstOrDefault(x => x.Label == FontSettingLabel.HeaderOne);
            var header2Font = character.CharacterTheme.FontSettings.FirstOrDefault(x => x.Label == FontSettingLabel.HeaderTwo);
            var header3Font = character.CharacterTheme.FontSettings.FirstOrDefault(x => x.Label == FontSettingLabel.HeaderThree);
            var paragraphFont = character.CharacterTheme.FontSettings.FirstOrDefault(x => x.Label == FontSettingLabel.Paragraph);

            //header & Dialogue Text
            DialogueTextContainer.UpdateHeaderTextFormat(header1Font);
            DialogueTextContainer.UpdateReferenceTextFormat(paragraphFont);
            //will have to modify based on target language either using the original or translation text


            // main container updates
            MainContainer.UpdateBackdropColor(character.CharacterTheme.MainColor);
            // this is the masked area graphics that is usually white - we generally want to match this with our backdrop
            MainContainer.UpdateRefIconColor(character.CharacterTheme.MainColor);
            // character
            CharacterContainer.UpdateReferenceTextFormat(header2Font);
            CharacterContainer.UpdateReferenceText(character.CharacterName);
            CharacterContainer.UpdateBackdropColor(character.CharacterTheme.MainColor);
            CharacterContainer.UpdateRefIconSprite(character.CharacterTheme.Icon);

            // progress bar
            ProgressBarContainer.UpdateBackdropColor(character.CharacterTheme.TertiaryColor);
            ProgressBarContainer.UpdateRefIconColor(character.CharacterTheme.SecondaryColor);
        }
        private void ClearUserResponses()
        {
            if (UserPromptButtonParentContainer.childCount > 0)
            {
                foreach (Transform child in UserPromptButtonParentContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        #region Conversation States/Functions
        public void NextButtonAction()
        {
            ClearUserResponses();
            if (dialogueLocalManager != null)
            {
                Debug.Log($"Next Button Pressed");
                dialogueLocalManager.UINextDialogueAction();
                
            }
        }
        public void PreviousButtonAction()
        {
            ClearUserResponses();
            if (dialogueLocalManager != null)
            {
                Debug.Log($"Previous Button Pressed");
                dialogueLocalManager.UIPreviousDialogueAction();
            }
        }
        public void FinishButtonAction()
        {
            ClearUserResponses();
            if (dialogueLocalManager != null)
            {
                dialogueLocalManager.UIFinishDialogueAction();
            }
        }
        public void UserButtonAction(DialogueResponse userResponse)
        {
            if (dialogueLocalManager != null)
            {
                dialogueLocalManager.UIUserPromptAction(userResponse);
            }
        }
        public void UserButtonAction(int responseIndex)
        {
            if (dialogueLocalManager != null)
            {
                Debug.Log($"Null getting passed for user response - passing index {responseIndex}");
                dialogueLocalManager.UIUserPromptAction(null, responseIndex);
                
            }
        }
        public void PlayDialogueBlock()
        {
            DialogueAudioSource.Play();
        }
        
        #endregion
        private void ChangeUserResponseFormat()
        {
            UserPromptButtonParentContainer.gameObject.SetActive(true);
            NextButton.gameObject.SetActive(false);
            FinishButton.gameObject.SetActive(false);
        }
        private void ChangeNormalResponseFormat()
        {
            UserPromptButtonParentContainer.gameObject.SetActive(false);
            NextButton.gameObject.SetActive(true);
            FinishButton.gameObject.SetActive(false);
        }
        private void PreviousActionAvailability(bool status)
        {
            if (status)
            {
                PreviousButton.SetupPreviousButton(this);
            }
            PreviousButton.gameObject.SetActive(status);
        }
        private void NextActionAvailability(bool status)
        {
            if (status)
            {
                NextButton.SetupNextButton(this);
            }
            else
            {
                FinishButton.SetupFinishButton(this);
            }
            NextButton.gameObject.SetActive(status);
            FinishButton.gameObject.SetActive(!status);
        }
    }
}
