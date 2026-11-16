using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using WrongSuspect.Actions;
using WrongSuspect.Events;
using WrongSuspect.Interactables;
using WrongSuspect.UI;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>
    /// Builds Prefabs/UI/GameHUD: one canvas with the pocket watch, the "End preparation" button,
    /// the carried-item box, the inspection panel and the action menu (plus the end-of-level screens),
    /// each already wired to its event assets.
    /// </summary>
    public static class HudPrefabBuilder
    {
        public const string HudName = "GameHUD";
        private const string Panels = "Assets/Art/Sprites/UI/Panels/";
        private static readonly Color Paper = new Color(0.98f, 0.95f, 0.86f);
        private static readonly Color Ink = new Color(0.12f, 0.08f, 0.10f);
        private static readonly Color Gold = new Color(1f, 0.84f, 0.37f);

        public static void Build(bool overwrite)
        {
            if (!overwrite && PrefabParts.Exists(UiPrefabBuilder.Folder, HudName))
            {
                return;
            }

            var root = new GameObject(HudName, typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            AddPocketWatch(root.transform);
            AddEndPreparationButton(root.transform);
            AddCarriedItem(root.transform);
            AddInspection(root.transform);
            AddActionMenu(root.transform);
            EndOfLevelUiBuilder.AddTo(root.transform);
            PrefabParts.SaveAndDestroy(root, UiPrefabBuilder.Folder, overwrite);
        }

        private static void AddPocketWatch(Transform parent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiPrefabBuilder.Folder}/PocketWatch.prefab");
            if (prefab == null)
            {
                return;
            }
            var rect = (RectTransform)((GameObject)PrefabUtility.InstantiatePrefab(prefab, parent)).transform;
            rect.anchorMin = rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(-120f, -120f);
        }

        private static void AddEndPreparationButton(Transform parent)
        {
            RectTransform rect = UiFactory.Rect("EndPreparationButton", parent, Vector2.one, new Vector2(0.5f, 0.5f),
                new Vector2(-120f, -268f), new Vector2(80f, 80f));
            Button button = UiFactory.AddButton(rect, Load("SealButton_EndPreparation"), Load("SealButton_EndPreparation_Hover"),
                Load("SealButton_EndPreparation"), false);
            UnityEventTools.AddPersistentListener(button.onClick, DataAssetBuilder.LoadEvent<GameEvent>(EventNames.PreparationEnded).Raise);

            RectTransform label = UiFactory.Rect("Label", rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(260f, 30f));
            UiFactory.AddText(label, "End preparation", 22, Paper, TextAnchor.UpperCenter, true);
        }

        private static void AddCarriedItem(Transform parent)
        {
            RectTransform root = UiFactory.Stretch("CarriedItem", parent);
            RectTransform panel = UiFactory.Rect("Panel", root, Vector2.zero, Vector2.zero, new Vector2(30f, 30f), new Vector2(320f, 112f));
            UiFactory.AddImage(panel, Load("Panel_Dark"), true, true);
            UiFactory.AddText(UiFactory.Rect("Caption", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -10f), new Vector2(240f, 28f)),
                "Carrying", 20, Gold, TextAnchor.UpperLeft, true);
            Image icon = UiFactory.AddImage(UiFactory.Rect("Icon", panel, Vector2.zero, Vector2.zero, new Vector2(18f, 12f), new Vector2(64f, 64f)), null, false);
            Text itemName = UiFactory.AddText(UiFactory.Rect("Name", panel, Vector2.zero, Vector2.zero, new Vector2(96f, 12f), new Vector2(210f, 64f)),
                "", 24, Paper, TextAnchor.MiddleLeft);

            var view = root.gameObject.AddComponent<CarriedItemView>();
            PrefabParts.SetReference(view, "carriedChanged", DataAssetBuilder.LoadEvent<CarriableEvent>(EventNames.CarriedChanged));
            PrefabParts.SetReference(view, "panel", panel.gameObject);
            PrefabParts.SetReference(view, "icon", icon);
            PrefabParts.SetReference(view, "itemName", itemName);
        }

        private static void AddInspection(Transform parent)
        {
            RectTransform root = UiFactory.Stretch("Inspection", parent);
            RectTransform panel = UiFactory.Rect("Panel", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(980f, 200f));
            UiFactory.AddImage(panel, Load("Panel_Parchment"), true, true);
            Text title = UiFactory.AddText(UiFactory.Rect("Title", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -20f), new Vector2(760f, 40f)),
                "", 30, Ink, TextAnchor.UpperLeft, true);
            Text body = UiFactory.AddText(UiFactory.Rect("Body", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -64f), new Vector2(760f, 110f)),
                "", 24, Ink, TextAnchor.UpperLeft);
            Button close = MenuButton("CloseButton", panel, "OK", false);
            var closeRect = (RectTransform)close.transform;
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 0f);
            closeRect.pivot = new Vector2(1f, 0f);
            closeRect.anchoredPosition = new Vector2(-24f, 22f);
            closeRect.sizeDelta = new Vector2(140f, 48f);

            var view = root.gameObject.AddComponent<InspectionView>();
            PrefabParts.SetReference(view, "inspected", DataAssetBuilder.LoadEvent<InteractableEvent>(EventNames.InteractableInspected));
            PrefabParts.SetReference(view, "menuClosed", DataAssetBuilder.LoadEvent<GameEvent>(EventNames.MenuClosed));
            PrefabParts.SetReference(view, "panel", panel.gameObject);
            PrefabParts.SetReference(view, "title", title);
            PrefabParts.SetReference(view, "body", body);
            PrefabParts.SetReference(view, "closeButton", close);
        }

        private static void AddActionMenu(Transform parent)
        {
            RectTransform root = UiFactory.Stretch("ActionMenu", parent);
            RectTransform panel = UiFactory.Rect("Panel", root, Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(320f, 200f));
            UiFactory.AddImage(panel, Load("Panel_Dark"), true, true);
            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 12, 14);
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            RectTransform titleRect = UiFactory.Rect("Title", panel, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(290f, 34f));
            titleRect.gameObject.AddComponent<LayoutElement>().preferredHeight = 34f;
            Text title = UiFactory.AddText(titleRect, "", 24, Gold, TextAnchor.MiddleLeft, true);

            Button template = MenuButton("ActionButton", panel, "Action", true);
            var templateView = template.gameObject.AddComponent<ActionButtonView>();
            PrefabParts.SetReference(templateView, "button", template);
            PrefabParts.SetReference(templateView, "icon", template.transform.Find("Icon").GetComponent<Image>());
            PrefabParts.SetReference(templateView, "label", template.transform.Find("Label").GetComponent<Text>());

            Button cancel = MenuButton("CancelButton", panel, "Cancel", false);

            var view = root.gameObject.AddComponent<ActionMenuView>();
            PrefabParts.SetReference(view, "menuRequested", DataAssetBuilder.LoadEvent<ActionMenuEvent>(EventNames.ActionMenuRequested));
            PrefabParts.SetReference(view, "menuClosed", DataAssetBuilder.LoadEvent<GameEvent>(EventNames.MenuClosed));
            PrefabParts.SetReference(view, "actionChosen", DataAssetBuilder.LoadEvent<ActionEvent>(EventNames.ActionChosen));
            PrefabParts.SetReference(view, "panel", panel);
            PrefabParts.SetReference(view, "title", title);
            PrefabParts.SetReference(view, "buttonTemplate", templateView);
            PrefabParts.SetReference(view, "cancelButton", cancel);
        }

        /// <summary>A wooden button with an optional icon on the left and a label.</summary>
        private static Button MenuButton(string name, Transform parent, string text, bool withIcon)
        {
            RectTransform rect = UiFactory.Rect(name, parent, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(290f, 48f));
            Button button = UiFactory.AddButton(rect, Load("Button_Normal"), Load("Button_Hover"), Load("Button_Pressed"));
            rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 48f;
            if (withIcon)
            {
                UiFactory.AddImage(UiFactory.Rect("Icon", rect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(34f, 34f)), null, false);
            }
            UiFactory.AddText(UiFactory.Stretch("Label", rect, withIcon ? 54f : 14f, 10f), text, 22, Paper, TextAnchor.MiddleLeft);
            return button;
        }

        private static Sprite Load(string spriteName) => AssetDatabase.LoadAssetAtPath<Sprite>($"{Panels}{spriteName}.png");
    }
}
