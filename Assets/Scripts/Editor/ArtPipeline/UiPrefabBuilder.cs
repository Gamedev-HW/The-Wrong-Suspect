using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WrongSuspect.Events;
using WrongSuspect.UI;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>
    /// Builds the UI prefabs: the pocket-watch timer (already listening to the PreparationTimerChanged event)
    /// and the three verdict stamps (they slam in whenever they are switched on).
    /// </summary>
    public static class UiPrefabBuilder
    {
        public const string Folder = "Assets/Prefabs/UI";
        private const float UiScale = 2f;                       // sprites shown at 2x on a 1080p reference canvas

        public static void BuildAll(bool overwrite)
        {
            if (overwrite || !PrefabParts.Exists(Folder, "PocketWatch"))
            {
                PrefabParts.SaveAndDestroy(BuildPocketWatch(), Folder, overwrite);
            }
            foreach (StampEntry stamp in ArtManifest.Data.stamps)
            {
                if (overwrite || !PrefabParts.Exists(Folder, stamp.name))
                {
                    PrefabParts.SaveAndDestroy(BuildStamp(stamp), Folder, overwrite);
                }
            }
        }

        private static GameObject BuildPocketWatch()
        {
            PocketWatchSprites sprites = ArtManifest.Data.pocketWatch;
            Sprite faceSprite = Load(sprites.face);
            GameObject root = UiObject("PocketWatch", null, faceSprite);

            Image glow = Picture("WarningGlow", root, Load(sprites.glow));
            glow.enabled = false;
            Image face = Picture("Face", root, faceSprite);
            Image minute = Picture("MinuteHand", root, Load(sprites.minuteHand));
            Image second = Picture("SecondHand", root, Load(sprites.secondHand));

            var pulse = root.AddComponent<Animator>();
            pulse.runtimeAnimatorController = AnimatorControllerBuilder.Load("TimerWarning");

            var view = root.AddComponent<PocketWatchView>();
            PrefabParts.SetReference(view, "timerChanged", DataAssetBuilder.LoadEvent<TimerEvent>(EventNames.PreparationTimerChanged));
            PrefabParts.SetReference(view, "minuteHand", minute.rectTransform);
            PrefabParts.SetReference(view, "secondHand", second.rectTransform);
            PrefabParts.SetReference(view, "face", face);
            PrefabParts.SetReference(view, "normalFace", faceSprite);
            PrefabParts.SetReference(view, "warningFace", Load(sprites.faceWarning));
            PrefabParts.SetReference(view, "warningGlow", glow);
            PrefabParts.SetReference(view, "pulseAnimator", pulse);
            return root;
        }

        private static GameObject BuildStamp(StampEntry entry)
        {
            Sprite sprite = Load(entry.sprite);
            GameObject root = UiObject(entry.name, null, sprite);
            var image = root.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            root.AddComponent<CanvasGroup>();                    // the slam animation fades it in
            root.AddComponent<Animator>().runtimeAnimatorController = AnimatorControllerBuilder.Load("StampSlam");
            return root;
        }

        private static GameObject UiObject(string name, GameObject parent, Sprite sizeFrom)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            if (parent != null)
            {
                rect.SetParent(parent.transform, false);
            }
            if (sizeFrom != null)
            {
                rect.sizeDelta = sizeFrom.rect.size * UiScale;
            }
            return go;
        }

        private static Image Picture(string name, GameObject parent, Sprite sprite)
        {
            GameObject go = UiObject(name, parent, sprite);
            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        private static Sprite Load(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
