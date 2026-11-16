using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using WrongSuspect.Dialogue;
using WrongSuspect.Events;
using WrongSuspect.Investigation;
using WrongSuspect.UI;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>
    /// Adds the end-of-level screens to the HUD: Sherlock's dialogue box, the alibi cards and the results screen,
    /// each wired to its event assets.
    /// </summary>
    public static class EndOfLevelUiBuilder
    {
        private const string Sprites = "Assets/Art/Sprites/";
        private static readonly Color Paper = new Color(0.98f, 0.95f, 0.86f);
        private static readonly Color Ink = new Color(0.12f, 0.08f, 0.10f);
        private static readonly Color Claret = new Color(0.45f, 0.10f, 0.12f);
        private static readonly Color Dim = new Color(0f, 0f, 0f, 0.65f);

        public static void AddTo(Transform hud)
        {
            AddDialogue(hud);
            AddAlibi(hud);
            AddResults(hud);
        }

        private static void AddDialogue(Transform hud)
        {
            RectTransform root = UiFactory.Stretch("Dialogue", hud);
            RectTransform panel = UiFactory.Rect("Panel", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(1240f, 190f));
            UiFactory.AddImage(panel, Load("UI/Panels/Panel_Parchment"), true);
            RectTransform frame = UiFactory.Rect("PortraitFrame", panel, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(150f, 150f));
            UiFactory.AddImage(frame, Load("UI/Panels/PortraitBackground"), false);
            Image portrait = UiFactory.AddImage(UiFactory.Stretch("Portrait", frame), null, false);
            UiFactory.AddImage(UiFactory.Stretch("Frame", frame), Load("UI/Panels/PortraitFrame"), true);
            Text speaker = UiFactory.AddText(UiFactory.Rect("Speaker", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(196f, -18f), new Vector2(900f, 34f)),
                "", 26, Claret, TextAnchor.UpperLeft, true);
            Text line = UiFactory.AddText(UiFactory.Rect("Line", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(196f, -58f), new Vector2(1010f, 120f)),
                "", 27, Ink, TextAnchor.UpperLeft);

            var view = root.gameObject.AddComponent<DialogueView>();
            PrefabParts.SetReference(view, "dialogue", DataAssetBuilder.LoadEvent<DialogueEvent>(EventNames.DialogueLine));
            PrefabParts.SetReference(view, "panel", panel.gameObject);
            PrefabParts.SetReference(view, "portrait", portrait);
            PrefabParts.SetReference(view, "speaker", speaker);
            PrefabParts.SetReference(view, "line", line);
        }

        private static void AddAlibi(Transform hud)
        {
            RectTransform root = UiFactory.Stretch("Alibi", hud);
            RectTransform panel = UiFactory.Stretch("Panel", root);
            UiFactory.AddImage(panel, null, false, true).color = Dim;
            RectTransform box = UiFactory.Rect("Box", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1260f, 540f));
            UiFactory.AddImage(box, Load("UI/Panels/Panel_Wood"), true, true);
            UiFactory.AddText(UiFactory.Rect("Title", box, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1100f, 50f)),
                "Sherlock is at the door. Choose your alibi:", 34, Paper, TextAnchor.UpperCenter, true);

            RectTransform row = UiFactory.Rect("Cards", box, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(1180f, 380f));
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 30f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            RectTransform card = UiFactory.Rect("Card", row, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(360f, 380f));
            Button template = UiFactory.AddButton(card, Load("UI/Panels/Card_Alibi"), Load("UI/Panels/Card_Alibi"), Load("UI/Panels/Card_Alibi"));
            template.transition = Selectable.Transition.ColorTint;
            UiFactory.AddText(UiFactory.Stretch("Text", card, 34f, 34f), "", 28, Ink, TextAnchor.MiddleCenter);

            var view = root.gameObject.AddComponent<AlibiView>();
            PrefabParts.SetReference(view, "alibiRequested", DataAssetBuilder.LoadEvent<AlibiRequestEvent>(EventNames.AlibiRequested));
            PrefabParts.SetReference(view, "alibiChosen", DataAssetBuilder.LoadEvent<IntEvent>(EventNames.AlibiChosen));
            PrefabParts.SetReference(view, "panel", panel.gameObject);
            PrefabParts.SetReference(view, "cardTemplate", template);
        }

        private static void AddResults(Transform hud)
        {
            RectTransform root = UiFactory.Stretch("Results", hud);
            RectTransform panel = UiFactory.Stretch("Panel", root);
            UiFactory.AddImage(panel, null, false, true).color = Dim;
            RectTransform card = UiFactory.Rect("Card", panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1180f, 700f));
            UiFactory.AddImage(card, Load("UI/Panels/Panel_Parchment"), true, true);

            Image medal = UiFactory.AddImage(UiFactory.Rect("Medal", card, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(36f, -30f), new Vector2(110f, 110f)), null, false);
            Text headline = UiFactory.AddText(UiFactory.Rect("Headline", card, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(170f, -44f), new Vector2(960f, 90f)),
                "", 36, Ink, TextAnchor.UpperLeft, true);

            RectTransform frame = UiFactory.Rect("PortraitFrame", card, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60f, -170f), new Vector2(270f, 270f));
            UiFactory.AddImage(frame, Load("UI/Panels/PortraitBackground"), false);
            Image portrait = UiFactory.AddImage(UiFactory.Stretch("Portrait", frame), null, false);
            UiFactory.AddImage(UiFactory.Stretch("Frame", frame), Load("UI/Panels/PortraitFrame"), true);

            RectTransform stampRect = UiFactory.Rect("Stamp", card, new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(230f, -430f), new Vector2(330f, 100f));
            stampRect.localEulerAngles = new Vector3(0f, 0f, -10f);
            Image stamp = UiFactory.AddImage(stampRect, null, false);
            stampRect.gameObject.AddComponent<CanvasGroup>();
            stampRect.gameObject.AddComponent<Animator>().runtimeAnimatorController = AnimatorControllerBuilder.Load("StampSlam");

            Text findings = UiFactory.AddText(UiFactory.Rect("Findings", card, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380f, -170f), new Vector2(740f, 400f)),
                "", 23, Ink, TextAnchor.UpperLeft);

            Button retry = ResultButton("TryAgainButton", card, "Try again", new Vector2(-290f, 36f));
            Button next = ResultButton("NextLevelButton", card, "Next level", new Vector2(-40f, 36f));
            UnityEventTools.AddPersistentListener(retry.onClick, DataAssetBuilder.LoadEvent<GameEvent>(EventNames.RestartRequested).Raise);
            UnityEventTools.AddPersistentListener(next.onClick, DataAssetBuilder.LoadEvent<GameEvent>(EventNames.NextLevelRequested).Raise);

            var view = root.gameObject.AddComponent<ResultsView>();
            PrefabParts.SetReference(view, "verdictReady", DataAssetBuilder.LoadEvent<VerdictEvent>(EventNames.VerdictReady));
            PrefabParts.SetReference(view, "panel", panel.gameObject);
            PrefabParts.SetReference(view, "portrait", portrait);
            PrefabParts.SetReference(view, "headline", headline);
            PrefabParts.SetReference(view, "findings", findings);
            PrefabParts.SetReference(view, "stamp", stamp);
            PrefabParts.SetReference(view, "medal", medal);
            PrefabParts.SetReference(view, "nextLevelButton", next.gameObject);
            PrefabParts.SetReference(view, "fullVictoryStamp", Load("UI/Stamps/Stamp_CaseClosed"));
            PrefabParts.SetReference(view, "fullVictoryMedal", Load("UI/Results/Medal_FullVictory"));
            PrefabParts.SetReference(view, "partialVictoryStamp", Load("UI/Stamps/Stamp_Suspect"));
            PrefabParts.SetReference(view, "partialVictoryMedal", Load("UI/Results/Medal_PartialVictory"));
            PrefabParts.SetReference(view, "failureStamp", Load("UI/Stamps/Stamp_Exposed"));
            PrefabParts.SetReference(view, "failureMedal", Load("UI/Results/Medal_Failure"));
        }

        private static Button ResultButton(string name, Transform parent, string label, Vector2 position)
        {
            RectTransform rect = UiFactory.Rect(name, parent, new Vector2(1f, 0f), new Vector2(1f, 0f), position, new Vector2(230f, 60f));
            Button button = UiFactory.AddButton(rect, Load("UI/Panels/Button_Normal"), Load("UI/Panels/Button_Hover"), Load("UI/Panels/Button_Pressed"));
            UiFactory.AddText(UiFactory.Stretch("Label", rect, 10f, 10f), label, 26, Paper, TextAnchor.MiddleCenter, true);
            return button;
        }

        private static Sprite Load(string path) => AssetDatabase.LoadAssetAtPath<Sprite>($"{Sprites}{path}.png");
    }
}
