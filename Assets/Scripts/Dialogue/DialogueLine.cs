using UnityEngine;

namespace WrongSuspect.Dialogue
{
    /// <summary>
    /// A line of speech for the dialogue box: who says it, their portrait and the text.
    /// Seconds = how long it stays (0 = until the next line). An empty text hides the box.
    /// </summary>
    public class DialogueLine
    {
        public DialogueLine(string speaker, Sprite portrait, string text, float seconds = 0f)
        {
            Speaker = speaker;
            Portrait = portrait;
            Text = text;
            Seconds = seconds;
        }

        public string Speaker { get; }
        public Sprite Portrait { get; }
        public string Text { get; }
        public float Seconds { get; }

        public static DialogueLine Hide => new DialogueLine("", null, "");
    }
}
