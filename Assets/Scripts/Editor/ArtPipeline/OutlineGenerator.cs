using System.IO;
using UnityEditor;
using UnityEngine;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>
    /// Creates the white 1-pixel outline sprite (Name_Highlight.png) used to highlight an object.
    /// Drop a new PNG into an interactive folder and its outline is made for you.
    /// </summary>
    public static class OutlineGenerator
    {
        public const string Suffix = "_Highlight";

        public static bool IsOutline(string spritePath) => Path.GetFileNameWithoutExtension(spritePath).EndsWith(Suffix);

        public static string OutlinePathFor(string spritePath) =>
            spritePath.Substring(0, spritePath.Length - ".png".Length) + Suffix + ".png";

        /// <summary>Returns the outline's path, generating the file first if it doesn't exist.</summary>
        public static string Ensure(string spritePath)
        {
            string outlinePath = OutlinePathFor(spritePath);
            if (File.Exists(outlinePath))
            {
                return outlinePath;
            }

            var source = new Texture2D(2, 2);
            Texture2D outline = null;
            try
            {
                if (!source.LoadImage(File.ReadAllBytes(spritePath)))
                {
                    return null;
                }
                outline = DrawOutline(source);
                File.WriteAllBytes(outlinePath, outline.EncodeToPNG());
            }
            finally
            {
                Object.DestroyImmediate(source);
                if (outline != null)
                {
                    Object.DestroyImmediate(outline);
                }
            }

            AssetDatabase.ImportAsset(outlinePath);
            return outlinePath;
        }

        private static Texture2D DrawOutline(Texture2D source)
        {
            int w = source.width, h = source.height;
            Color32[] src = source.GetPixels32();
            var dst = new Color32[src.Length];
            var white = new Color32(255, 255, 255, 255);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (src[y * w + x].a == 0 &&
                        (IsOpaque(src, w, h, x - 1, y) || IsOpaque(src, w, h, x + 1, y) ||
                         IsOpaque(src, w, h, x, y - 1) || IsOpaque(src, w, h, x, y + 1)))
                    {
                        dst[y * w + x] = white;
                    }
                }
            }

            var outline = new Texture2D(w, h, TextureFormat.RGBA32, false);
            outline.SetPixels32(dst);
            outline.Apply();
            return outline;
        }

        private static bool IsOpaque(Color32[] pixels, int w, int h, int x, int y) =>
            x >= 0 && y >= 0 && x < w && y < h && pixels[y * w + x].a > 0;
    }
}
