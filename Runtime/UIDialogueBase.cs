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
        [Tooltip("Generic GameObject for user prompts")]
        public GameObject UserPromptButtonPrefab;
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
       
        public void Setup(FP_Character character, DialogueBlock dialogeData)
        {
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
            DialogueTextContainer.UpdateReferenceText(dialogeData.DialogueText);
            if (dialogeData.DialogueHeader != string.Empty)
            {
                DialogueTextContainer.UpdateHeaderText(dialogeData.DialogueHeader);
            }
            
            //character
            CharacterContainer.UpdateReferenceTextFormat(header2Font);
            CharacterContainer.UpdateReferenceText(character.CharacterName);
            CharacterContainer.UpdateBackdropColor(character.CharacterTheme.MainColor);
            CharacterContainer.UpdateRefIconSprite(character.CharacterTheme.Icon);
            //any character color needs ?



           
            

           //if it has a user response
           if(dialogeData.PossibleUserResponses.Count > 0)
           {
                foreach (var response in dialogeData.PossibleUserResponses)
                {
                    //var userPrompt = Instantiate(UserPromptButtonPrefab, MainContainer.transform);
                    //var userPromptButton = userPrompt.GetComponent<Button>();
                    //var userPromptText = userPrompt.GetComponentInChildren<TextMeshProUGUI>();
                    //userPromptText.text = response.ResponseText;
                    //userPromptButton.onClick.AddListener(() => response.ResponseAction.Invoke());
                }
           }

            //audio clip setup
            if (DialogueAudioSource.isPlaying)
            {
                DialogueAudioSource.Stop();
            }
            DialogueAudioSource.clip = dialogeData.DialogueAudio.AudioClip;
        }
        private void ChangeUserResponseFormat()
        {

        }
        private void ChangeNormalResponseFormat()
        {

        }
    }
}
