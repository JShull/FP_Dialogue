using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using FuzzPhyte.Utility.Notification;
using FuzzPhyte.Dialogue.UI;
namespace FuzzPhyte.Dialogue
{
    public class FP_DuringDialogueEvent : MonoBehaviour
    {
        //public CC_NPC RelatedNPC;
        public FP_Dialogue RelatedDialogue;
        public FP_OverlayNotification RelatedNotification;
        public FP_UI_Overlay DialogueUI;
        public Camera CamToAdjust;
        public Vector3 EulerRotation;
        [Tooltip("Do we want to only fire this once? aka user keeps going back and forth")]
        public bool FireOnce;
        private bool _fired;
        [Space]
        [Header("Related Events")]
        public UnityEvent DialogueEvent;
        public float DelayAmount;
        public UnityEvent DialogueDelayEvent;

        public void OnEnable()
        {
            if (DialogueUI == null)
            {
                //DialogueUI = CC_Manager.CCManager.OverlayMenu;
                Debug.LogError($"missing reference to the Dialogue UI");
                return;
            }
            if (DialogueUI != null)
            {
                DialogueUI.DialogueNextButtonEvent += OnDialogueNextListener;
                DialogueUI.DialoguePreviousButtonEvent += OnDialoguePreviousListener;
                DialogueUI.DelegateDialogueFinishedEvent += OnDialogueFinishedListener;
                DialogueUI.DelegateDialogueStartEvent += OnDialogueStartListener;
            }
        }
        public void OnDisable()
        {
            if (DialogueUI == null)
            {
                //DialogueUI = CC_Manager.CCManager.OverlayMenu;
                Debug.LogError($"Missing reference to the Dialogue UI");
                return;
            }
            
            if (DialogueUI != null)
            {
                DialogueUI.DialogueNextButtonEvent -= OnDialogueNextListener;
                DialogueUI.DialoguePreviousButtonEvent -= OnDialoguePreviousListener;
                DialogueUI.DelegateDialogueFinishedEvent -= OnDialogueFinishedListener;
                DialogueUI.DelegateDialogueStartEvent -= OnDialogueStartListener;
            }
        }

        private void OnDialogueNextListener(FP_OverlayNotification dialogueNotification)
        {
            if (dialogueNotification == RelatedNotification)
            {
                if (FireOnce)
                {
                    if (!_fired)
                    {
                        DialogueEvent.Invoke();
                        StartCoroutine(DelayEvent());
                    }
                }
                else
                {
                    DialogueEvent.Invoke();
                }
                _fired = true;
            }
        }
        private void OnDialoguePreviousListener(FP_OverlayNotification dialogueNotification)
        {
            if (dialogueNotification == RelatedNotification)
            {
                if (FireOnce)
                {
                    if (!_fired)
                    {
                        DialogueEvent.Invoke();
                        StartCoroutine(DelayEvent());
                    }
                }
                else
                {
                    DialogueEvent.Invoke();
                }
                _fired = true;
            }
        }
        private void OnDialogueFinishedListener(FP_OverlayNotification dialogueNotification)
        {
            if (dialogueNotification == RelatedNotification)
            {

            }
        }
        private void OnDialogueStartListener(FP_OverlayNotification dialogueNotification)
        {
            if (dialogueNotification == RelatedNotification)
            {
                if (FireOnce)
                {
                    if (!_fired)
                    {
                        DialogueEvent.Invoke();
                        StartCoroutine(DelayEvent());
                    }
                }
                else
                {
                    DialogueEvent.Invoke();
                }
                _fired = true;
            }
        }
        IEnumerator DelayEvent()
        {
            yield return new WaitForSecondsRealtime(DelayAmount);
            DialogueDelayEvent.Invoke();
        }
        public void AdjustRotation()
        {
            CamToAdjust.transform.rotation = Quaternion.Euler(EulerRotation);
        }
    }
}
