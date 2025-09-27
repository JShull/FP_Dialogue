namespace FuzzPhyte.Dialogue
{
    using UnityEngine;

    #region Interfaces Setup for Dialogue Needs
    public interface IDialogueUserResponseButton
    {
        //interface to get some data passed to the component that will be handling the user response
        public void SetupUserResponse(DialogueResponse userResponse, UIDialogueBase dialogueBase = null);
        public void SetupNextButton(UIDialogueBase dialogueBase = null, bool useGraph = false);
        public void SetupPreviousButton(UIDialogueBase dialogueBase = null, bool useGraph = false);
        public void SetupFinishButton(UIDialogueBase dialogueBase = null);
        public void SetupTranslateButton(UIDialogueBase dialogueBase=null,bool useGraph = false);
    }
    /// <summary>
    /// Graph Interface Needs/Requirements for lower response "buttons"
    /// </summary>
    public interface IDialogueGraphResponseButton
    {
        public void SetupUserResponse(int index, IDialogueDirectorActions directorRef = null, string textDisplay = "", AudioClip clipToPlay = null, AudioSource clipSource = null);
        public void UpdateMainButtonSprite(Sprite newSprite);
        public void ShowUIVisuals(bool active);
    }
    /// <summary>
    /// Graph Interface for base UI class
    /// </summary>
    public interface IDialogueGraphBase
    {
        public void SetupDialoguePanel(DialogueUnity fullDialogueData, RTDialogueNode node, bool useTranslation = false, RTFPNode previousNode = null, RTFPNode nextNode = null);
        public void SetupResponsePanel(DialogueUnity fullDialogueData, RTResponseNode responseNode, RTFPNode previousNode = null, RTFPNode nextNode = null);
    }
    public interface IDialoguePanel
    {
        public void SetupDialoguePanel(FP_Character character, DialogueBlock conversationBlock, DialogueUnity fullDialogueData);
    }
    /// <summary>
    /// Generic actions for convo 'flow'
    /// </summary>
    public interface IDialogueActions
    {
        //interface that accompanies the UIDialogueBase to work with the IDialogueUserResponseButton interface
        public void NextButtonAction();
        public void PreviousButtonAction();
        public void FinishButtonAction();
        public void TranslateAction();
        public void ShowHideUIAction(bool status);
        public void UserButtonAction(DialogueResponse userResponse);
        public void PlayAudioDialogueBlock();
    }
    public interface IDialogueDirectorActions
    {
        public void StartConversation();
        public void UserPromptNext();
        public void UserPromptPrevious();
        public void UserPromptRepeat();
        public void UserPromptTranslate();
        public bool UserTranslateAvailable();
        public void UserPromptResponse(int promptIndex, bool useDelay = false, float delayOnAudio = 0);
        public void NewDialogueAdded(RTFPDialogueGraph newGraph);
        public string ReturnConversationID();
        public string ReturnGraphID();

    }
    
    /// <summary>
    /// Interface for various components/scripts that have additional timeline functionality requirements
    /// this assumes that there's something with a timeline playable director nearby
    /// almost like a wrapper for the PlayableDirector
    /// </summary>
    public interface IDialogueTimeline
    {
        /// <summary>
        /// General Playable director wrap for .Play()
        /// Play the timeline we've matched with
        /// </summary>
        /// <param name="singlePlay"></param>
        public void PlayTimeline(bool singlePlay=true);

        /// <summary>
        /// Set the Playable director with our timeline information/details
        /// </summary>
        /// <param name="timelineDetails"></param>
        public void SetupTimeline(RTTimelineDetails timelineDetails);

        /// <summary>
        /// Generic Playable director wrap for .Stop()
        /// </summary>
        public void StopTimeline();

        public void PauseTimeline();
        public void ResetTimeline(float startTime = 0);
        public void ResumeTimeline();
    }
    #endregion
}
