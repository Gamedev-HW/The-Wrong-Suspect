using UnityEngine;
using UnityEngine.UI;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>Creates the basic uGUI pieces (rects, images, texts, buttons) used by the UI prefab builders.</summary>
    public static class UiFactory
    {
        private static Font font;

        public static Font DefaultFont
        {
            get
            {
                if (font == null)
                {
                    font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                return font;
            }
        }

        public static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        /// <summary>A rect that fills its parent, minus the given margins.</summary>
        public static RectTransform Stretch(string name, Transform parent, float left = 0f, float right = 0f)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, 0f);
            rect.offsetMax = new Vector2(-right, 0f);
            return rect;
        }

        public static Image AddImage(RectTransform rect, Sprite sprite, bool sliced, bool blocksClicks = false)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            image.preserveAspect = !sliced;
            image.raycastTarget = blocksClicks;
            return image;
        }

        public static Text AddText(RectTransform rect, string content, int size, Color color, TextAnchor alignment, bool bold = false)
        {
            var text = rect.gameObject.AddComponent<Text>();
            text.font = DefaultFont;
            text.text = content;
            text.fontSize = size;
            text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        public static Button AddButton(RectTransform rect, Sprite normal, Sprite hover, Sprite pressed, bool sliced = true)
        {
            Image image = AddImage(rect, normal, sliced, true);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = hover,
                pressedSprite = pressed,
                selectedSprite = normal,
                disabledSprite = normal,
            };
            return button;
        }
    }
}
