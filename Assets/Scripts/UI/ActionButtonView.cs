using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WrongSuspect.UI
{
    /// <summary>One row of the action menu: icon, label with time cost, and the click callback.</summary>
    public class ActionButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Text label;

        public void Bind(string text, Sprite iconSprite, float seconds, UnityAction onClick)
        {
            label.text = seconds > 0f ? $"{text}   {seconds:0}s" : text;
            icon.sprite = iconSprite;
            icon.enabled = iconSprite != null;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick);
        }
    }
}
