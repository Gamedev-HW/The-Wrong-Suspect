using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>Knows where every generated animation clip lives and loads it by name.</summary>
    public static class ClipLibrary
    {
        public const string TweenFolder = "Assets/Art/Animations/UI";

        private static ArtManifestData indexedData;
        private static Dictionary<string, string> paths;

        public static string PathOf(string clipName)
        {
            Index();
            return paths.TryGetValue(clipName, out string path) ? path : null;
        }

        public static bool Contains(string clipName) => PathOf(clipName) != null;

        public static AnimationClip Load(string clipName)
        {
            string path = PathOf(clipName);
            AnimationClip clip = path != null ? AssetDatabase.LoadAssetAtPath<AnimationClip>(path) : null;
            if (clip == null)
            {
                Debug.LogError($"[Wrong Suspect] Animation clip missing: {clipName}");
            }
            return clip;
        }

        private static void Index()
        {
            if (indexedData == ArtManifest.Data && paths != null)
            {
                return;
            }
            indexedData = ArtManifest.Data;
            paths = new Dictionary<string, string>();
            foreach (ClipEntry c in indexedData.clips)
            {
                paths[c.name] = $"{c.folder}/{c.name}.anim";
            }
            foreach (TweenEntry t in indexedData.tweens)
            {
                paths[t.name] = $"{TweenFolder}/{t.name}.anim";
            }
        }
    }
}
