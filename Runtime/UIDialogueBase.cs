using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using FuzzPhyte.Utility;
using TMPro;
namespace FuzzPhyte.Dialogue
{
    public class UIDialogueBase : MonoBehaviour
    {
        public Button NextButton;
        public Button PreviousButton;
        public Button FinishButton;
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
       
        public void Setup(FP_Character character, DialogueBlock conversationBlock,DialogueUnity fullDialogueData)
        {
            dialogueLocalManager = fullDialogueData;
            //resize if we need to here
            //use the character theme to set the colors and fonts
            // header 1 = DialogueTextContainer
            // query through the font settings to find the right one that matches the header 1

            var header1Font = character.CharacterTheme.FontSettings.FirstOrDefault(x => x.Label == FontSettingLabel.HeaderOne);
            var header2Font = character.CharacterTheme.FontSettings.FirstOrDefault(x => x.Label == FontSettingLabel.HeaderTwo);
            var header3Font = character.CharacterTheme.FontSettings.FirstOrDefault(x => x.Label == FontSettingLabel.HeaderThree);
            var paragraphFont = character.CharacterTheme.FontSettings.FirstOrDefault(x => x.Label == FontSettingLabel.Paragraph);

            //header & Dialogue Text
            DialogueTextContainer.UpdateHeaderTextFormat(header1Font);
            DialogueTextContainer.UpdateReferenceTextFormat(paragraphFont);
            DialogueTextContainer.UpdateReferenceText(conversationBlock.DialogueText);
            if (conversationBlock.DialogueHeader != string.Empty)
            {
                DialogueTextContainer.UpdateHeaderText(conversationBlock.DialogueHeader);
            }
            
            //character
            CharacterContainer.UpdateReferenceTextFormat(header2Font);
            CharacterContainer.UpdateReferenceText(character.CharacterName);
            CharacterContainer.UpdateBackdropColor(character.CharacterTheme.MainColor);
            CharacterContainer.UpdateRefIconSprite(character.CharacterTheme.Icon);

            //default setup - we assume we know nothing and there's a next action
            ChangeNormalResponseFormat();
            // if we have a previous action - we should turn it on/off based on if it exists
            PreviousActionAvailability(dialogueLocalManager.PreviousDialogueAvailable());
            // are we the last and/or user response?
            NextActionAvailability(dialogueLocalManager.NextDialogueAvailable());

            //if it has a user response
            if (conversationBlock.PossibleUserResponses.Count > 0)
            {
                ChangeUserResponseFormat();
                for(int i = 0; i < conversationBlock.PossibleUserResponses.Count; i++)
                {
                    var response = conversationBlock.PossibleUserResponses[i];
                    var userPrompt = Instantiate(UserPromptButtonPrefab.gameObject, MainContainer.transform);
                    if (userPrompt.GetComponent<UIDialogueButton>())
                    {
                        var setupCode = userPrompt.GetComponent<UIDialogueButton>();
                        setupCode.UpdateReferenceText(response.ResponseText);
                        setupCode.UpdateRefIconSprite(response.ResponseIcon);
                        //need to create the button onClick event and probably notify DialogueUnity here by creating and passing a DialogueEventData object

                    }
                }
            }
            else
            {
                
            }

            //audio clip setup
            if (DialogueAudioSource.isPlaying)
            {
                DialogueAudioSource.Stop();
            }
            DialogueAudioSource.clip = conversationBlock.DialogueAudio.AudioClip;
        }
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
            PreviousButton.gameObject.SetActive(status);
        }
        private void NextActionAvailability(bool status)
        {
            NextButton.gameObject.SetActive(status);
            FinishButton.gameObject.SetActive(!status);
        }
    }
}
