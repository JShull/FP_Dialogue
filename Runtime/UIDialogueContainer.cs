namespace FuzzPhyte.Dialogue
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using FuzzPhyte.Utility;

    public class UIDialogueContainer : MonoBehaviour
    {
        public Image Background;
        public Image RefIcon;
        public TextMeshProUGUI RefHeader;
        public TextMeshProUGUI RefText;

        #region Public Modifiers
        public void UpdateHeaderText(string text)
        {
            if (!ReferenceTextIsNullOrEmpty(RefHeader))
            {
                RefHeader.text = text;
            }
        }
        public void UpdateReferenceText(string text)
        {
            if(!ReferenceTextIsNullOrEmpty(RefText))
            {
                RefText.text = text;
            }
        }
        public void UpdateBackdropSprite(Sprite sprite)
        {
            if (!ReferenceImageIsNullOrEmpty(Background))
            {
                Background.sprite = sprite;
            }
        }
        public void UpdateBackdropColor (Color color)
        {
            if (!ReferenceImageIsNullOrEmpty(Background))
            {
                Background.color = color;
            }
        }
        public void UpdateRefIconColor(Color color)
        {
            if (!ReferenceImageIsNullOrEmpty(RefIcon))
            {
                RefIcon.color = color;
            }
        }
        public void UpdateRefIconSprite(Sprite sprite)
        {
            if (!ReferenceImageIsNullOrEmpty(RefIcon))
            {
                RefIcon.sprite = sprite;
            }
        }
        public void UpdateHeaderTextFormat(FontSetting font)
        {
            if (!ReferenceTextIsNullOrEmpty(RefHeader))
            {
                UpdateFontFormat(RefHeader, font.Font, font.FontColor, font.MinSize, font.MaxSize, font.UseAutoSizing);
            }
        }
        public void UpdateReferenceTextFormat(FontSetting font)
        {
            if (!ReferenceTextIsNullOrEmpty(RefText))
            {
                UpdateFontFormat(RefText, font.Font, font.FontColor,font.MinSize, font.MaxSize, font.UseAutoSizing);
            }
        }
        public void UpdateFillAmount(float fillAmount)
        {
            if (!ReferenceImageIsNullOrEmpty(RefIcon))
            {
                RefIcon.fillAmount = fillAmount;
            }
        }
        #endregion
        private void UpdateFontFormat(TextMeshProUGUI fontRef, TMP_FontAsset font, Color fontColor,float minFont=18f,float maxFont = 72f,bool autoSizing=false )
        {
            fontRef.color = fontColor;
            fontRef.font = font;
            fontRef.enableAutoSizing = autoSizing;
            fontRef.fontSizeMin = minFont;
            fontRef.fontSizeMax = maxFont;
        }
        private bool ReferenceTextIsNullOrEmpty(TextMeshProUGUI text)
        {
            //single line return if text reference is null
            return text == null;
        }
        private bool ReferenceImageIsNullOrEmpty(Image image)
        {
            //single line return if image reference is null
            return image == null;
        }
    }
}
