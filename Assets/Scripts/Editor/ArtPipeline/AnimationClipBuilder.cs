using System;
using UnityEditor;
using UnityEngine;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>
    /// Builds the .anim files: sprite-swap clips from the manifest's frame lists,
    /// and scale/fade tween clips for the UI. Existing clips are updated in place.
    /// </summary>
    public static class AnimationClipBuilder
    {
        public static void BuildAll()
        {
            foreach (ClipEntry entry in ArtManifest.Data.clips)
            {
                BuildSpriteClip(entry);
            }
            foreach (TweenEntry entry in ArtManifest.Data.tweens)
            {
                BuildTweenClip(entry);
            }
        }

        private static void BuildSpriteClip(ClipEntry entry)
        {
            Type target = entry.target == "Image" ? Type.GetType("UnityEngine.UI.Image, UnityEngine.UI") : typeof(SpriteRenderer);
            Sprite[] sprites = LoadSprites(entry);
            if (target == null || sprites == null)
            {
                return;
            }

            // one key per frame, plus a last key so the final frame is shown for a full frame too
            var keys = new ObjectReferenceKeyframe[sprites.Length + 1];
            for (int i = 0; i < sprites.Length; i++)
            {
                keys[i] = new ObjectReferenceKeyframe { time = i / entry.fps, value = sprites[i] };
            }
            keys[sprites.Length] = new ObjectReferenceKeyframe { time = sprites.Length / entry.fps, value = sprites[sprites.Length - 1] };

            AnimationClip clip = LoadOrCreate($"{entry.folder}/{entry.name}.anim", out bool isNew);
            clip.frameRate = entry.fps;
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve("", target, "m_Sprite"), keys);
            Save(clip, $"{entry.folder}/{entry.name}.anim", entry.loop, isNew);
        }

        private static void BuildTweenClip(TweenEntry entry)
        {
            string path = $"{ClipLibrary.TweenFolder}/{entry.name}.anim";
            AnimationClip clip = LoadOrCreate(path, out bool isNew);
            clip.frameRate = 60f;

            AnimationCurve scale = ToCurve(entry.scale);
            float length = scale.keys[scale.length - 1].time;
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.x"), scale);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.y"), scale);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.z"),
                AnimationCurve.Constant(0f, length, 1f));
            if (entry.alpha != null && entry.alpha.Length >= 2)
            {
                // fades need a CanvasGroup on the animated UI object
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(CanvasGroup), "m_Alpha"), ToCurve(entry.alpha));
            }
            Save(clip, path, entry.loop, isNew);
        }

        private static Sprite[] LoadSprites(ClipEntry entry)
        {
            var sprites = new Sprite[entry.frames.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(entry.frames[i]);
                if (sprites[i] == null)
                {
                    Debug.LogError($"[Wrong Suspect] {entry.name}: missing sprite {entry.frames[i]}");
                    return null;
                }
            }
            return sprites;
        }

        private static AnimationCurve ToCurve(float[] timeValuePairs)
        {
            var curve = new AnimationCurve();
            for (int i = 0; i + 1 < timeValuePairs.Length; i += 2)
            {
                curve.AddKey(timeValuePairs[i], timeValuePairs[i + 1]);
            }
            return curve;
        }

        private static AnimationClip LoadOrCreate(string path, out bool isNew)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            isNew = clip == null;
            return isNew ? new AnimationClip() : clip;
        }

        private static void Save(AnimationClip clip, string path, bool loop, bool isNew)
        {
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            if (isNew)
            {
                AssetFolders.Ensure(System.IO.Path.GetDirectoryName(path).Replace('\\', '/'));
                AssetDatabase.CreateAsset(clip, path);
            }
            else
            {
                EditorUtility.SetDirty(clip);
            }
        }
    }
}
