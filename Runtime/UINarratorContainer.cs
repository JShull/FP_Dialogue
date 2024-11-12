namespace FuzzPhyte.Dialogue
{
    using UnityEngine;
    using UnityEngine.UI;
    using FuzzPhyte.Utility;
    using TMPro;

    public class UINarratorContainer : MonoBehaviour
    {
        public Image Background;
        public Image RefIcon;
        public TMP_Text RefHeader;
        public TMP_Text RefText;


        #region Public Modifiers
        public virtual void UpdateHeaderText(string text)
        {
            if (!ReferenceTextIsNullOrEmpty(RefHeader))
            {
                RefHeader.text = text;
            }
        }
        public virtual void UpdateReferenceText(string text)
        {
            if (!ReferenceTextIsNullOrEmpty(RefText))
            {
                RefText.text = text;
            }
        }
        public virtual void UpdateBackdropSprite(Sprite sprite)
        {
            if (!ReferenceImageIsNullOrEmpty(Background))
            {
                Background.sprite = sprite;
            }
        }
        public virtual void UpdateBackdropColor(Color color)
        {
            if (!ReferenceImageIsNullOrEmpty(Background))
            {
                Background.color = color;
            }
        }
        public virtual void UpdateRefIconColor(Color color)
        {
            if (!ReferenceImageIsNullOrEmpty(RefIcon))
            {
                RefIcon.color = color;
            }
        }
        public virtual void UpdateRefIconSprite(Sprite sprite)
        {
            if (!ReferenceImageIsNullOrEmpty(RefIcon))
            {
                RefIcon.sprite = sprite;
            }
        }
        public virtual void UpdateHeaderTextFormat(FontSetting font)
        {
            if (!ReferenceTextIsNullOrEmpty(RefHeader))
            {
                UpdateFontFormat(RefHeader, font.Font, font.FontColor, font.MinSize, font.MaxSize, font.UseAutoSizing);
            }
        }
        public virtual void UpdateReferenceTextFormat(FontSetting font)
        {
            if (!ReferenceTextIsNullOrEmpty(RefText))
            {
                UpdateFontFormat(RefText, font.Font, font.FontColor, font.MinSize, font.MaxSize, font.UseAutoSizing);
            }
        }
        public virtual void UpdateFillAmount(float fillAmount)
        {
            if (!ReferenceImageIsNullOrEmpty(RefIcon))
            {
                RefIcon.fillAmount = fillAmount;
            }
        }
        #endregion
        protected virtual void UpdateFontFormat(TMP_Text fontRef, TMP_FontAsset font, Color fontColor, float minFont = 18f, float maxFont = 72f, bool autoSizing = false)
        {
            fontRef.color = fontColor;
            fontRef.font = font;
            fontRef.enableAutoSizing = autoSizing;
            fontRef.fontSizeMin = minFont;
            fontRef.fontSizeMax = maxFont;
        }
        protected virtual bool ReferenceTextIsNullOrEmpty(TMP_Text text)
        {
            //single line return if text reference is null
            return text == null;
        }
        protected virtual bool ReferenceImageIsNullOrEmpty(Image image)
        {
            //single line return if image reference is null
            return image == null;
        }
    }
}
