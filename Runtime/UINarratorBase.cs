namespace FuzzPhyte.Dialogue
{
    using UnityEngine;
    using System.Linq;
    using FuzzPhyte.Utility;

    public class UINarratorBase : MonoBehaviour
    {
        [Space]
        [Header("Main Container")]
        [Tooltip("Main container for all the narrator elements")]
        public UINarratorContainer MainContainer;
        protected UIDialogueNarrator dialogueLocalManager;

        [Space]
        [Header("Other Containers")]
        [Tooltip("Dialogue & Header text for the dialogue")]
        public UINarratorContainer DialogueTextContainer;
        [Tooltip("Character image and text container")]
        [Header("Misc.")]
        
        public AudioSource DialogueAudioSource;
        [Tooltip("If we want to use just the dialogue text without the character image or other elements")]
        [SerializeField] protected bool useJustDialogue = true;
        protected bool autoScrollDialogue = false;
        [SerializeField] protected float delayBetweenAuto = 0.4f;
        public void SetupTextPanel(FP_Character character, DialogueBlock narratorBlock, UIDialogueNarrator fullDialogueData, bool autoScroll=false,bool useJustDialoguePanel=true)
        {
            dialogueLocalManager = fullDialogueData;
            useJustDialogue = useJustDialoguePanel;
            var header1Font = character.CharacterTheme.FontSettings.FirstOrDefault(x => x.Label == FontSettingLabel.HeaderOne);
            //var header2Font = character.CharacterTheme.FontSettings.FirstOrDefault(x => x.Label == FontSettingLabel.HeaderTwo);
            //var header3Font = character.CharacterTheme.FontSettings.FirstOrDefault(x => x.Label == FontSettingLabel.HeaderThree);
            var paragraphFont = character.CharacterTheme.FontSettings.FirstOrDefault(x => x.Label == FontSettingLabel.Paragraph);

            DialogueTextContainer.UpdateHeaderTextFormat(header1Font);
            DialogueTextContainer.UpdateReferenceTextFormat(paragraphFont);
            //will have to modify based on target language either using the original or translation text

            DialogueTextContainer.UpdateReferenceText(narratorBlock.OriginalLanguage.Text);
            if (narratorBlock.OriginalLanguage.Header != string.Empty)
            {
                DialogueTextContainer.UpdateHeaderText(narratorBlock.OriginalLanguage.Header);
            }

            // main container updates
            MainContainer.UpdateBackdropColor(character.CharacterTheme.MainColor);
            //this is the masked area graphics that is usually white - we generally want to match this with our backdrop
            MainContainer.UpdateRefIconColor(character.CharacterTheme.MainColor);

            //audio clip setup
            if (DialogueAudioSource.isPlaying)
            {
                DialogueAudioSource.Stop();
            }
            // audio clip will have to be driven by the above chosen language as we have two areas to pick from - original /translation

            DialogueAudioSource.clip = narratorBlock.OriginalLanguage.AudioText.AudioClip;
            if (useJustDialogue)
            {
                MainContainer.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                MainContainer.GetComponent<RectTransform>().anchorMax = Vector2.zero;
                MainContainer.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                DialogueTextContainer.GetComponent<RectTransform>().anchorMax = Vector2.one;
                DialogueTextContainer.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                DialogueTextContainer.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            }
            autoScrollDialogue = autoScroll;
        }
        
        public void PlayDialogueBlock()
        {
            DialogueAudioSource.Play();
            if (autoScrollDialogue)
            {
                //start the next dialogue block via our timer
                if (FP_Timer.CCTimer != null)
                {
                    FP_Timer.CCTimer.StartTimer(DialogueAudioSource.clip.length + delayBetweenAuto, dialogueLocalManager.UINextDialogueAction);
                }
            }
        }
    }
}
