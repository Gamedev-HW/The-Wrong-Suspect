using UnityEditor;
using UnityEngine;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>
    /// The first time a PNG under Assets/Art/Sprites is imported, set it up as crisp pixel art.
    /// Later changes you make in the Inspector are kept.
    /// </summary>
    public class SpriteImportPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetPath.StartsWith(ArtManifest.SpriteRoot) && assetImporter.importSettingsMissing)
            {
                SpriteImportSettings.Apply((TextureImporter)assetImporter, assetPath);
            }
        }
    }

    /// <summary>
    /// Pixel-art import settings. The pivot comes from the folder:
    /// Characters and Props stand on their feet, Rooms start at the bottom-left, everything else is centred.
    /// </summary>
    public static class SpriteImportSettings
    {
        public static void ApplyToAll()
        {
            string[] pngs = AssetFolders.PngsIn(ArtManifest.SpriteRoot.TrimEnd('/'), true);
            try
            {
                AssetDatabase.StartAssetEditing();
                for (int i = 0; i < pngs.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("The Wrong Suspect", "Sprite import settings", (float)i / pngs.Length);
                    if (AssetImporter.GetAtPath(pngs[i]) is TextureImporter importer)
                    {
                        Apply(importer, pngs[i]);
                        importer.SaveAndReimport();
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }
        }

        public static void Apply(TextureImporter importer, string assetPath)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ArtManifest.Data.pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)PivotFor(assetPath);
            settings.spriteBorder = BorderFor(assetPath);
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
        }

        private static SpriteAlignment PivotFor(string assetPath)
        {
            if (assetPath.Contains("/Characters/") || assetPath.Contains("/Props/"))
            {
                return SpriteAlignment.BottomCenter;
            }
            if (assetPath.Contains("/Rooms/"))
            {
                return SpriteAlignment.BottomLeft;
            }
            return SpriteAlignment.Center;
        }

        private static Vector4 BorderFor(string assetPath)
        {
            float[] b = ArtManifest.FindBorder(assetPath);
            return b != null && b.Length == 4 ? new Vector4(b[0], b[1], b[2], b[3]) : Vector4.zero;
        }
    }
}
