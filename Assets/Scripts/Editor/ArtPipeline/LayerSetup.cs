using UnityEditor;
using UnityEngine;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>Makes sure the "Obstacles" layer (walls and furniture footprints) exists in the project.</summary>
    public static class LayerSetup
    {
        public const string ObstaclesLayerName = "Obstacles";
        private const int FirstUserLayer = 8;

        /// <summary>Index of the Obstacles layer, created in the first free user layer if needed.</summary>
        public static int EnsureObstaclesLayer()
        {
            int existing = LayerMask.NameToLayer(ObstaclesLayerName);
            if (existing >= 0)
            {
                return existing;
            }

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            for (int i = FirstUserLayer; i < layers.arraySize; i++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layer.stringValue))
                {
                    layer.stringValue = ObstaclesLayerName;
                    tagManager.ApplyModifiedPropertiesWithoutUndo();
                    return i;
                }
            }

            Debug.LogError("[Wrong Suspect] No free layer for \"Obstacles\". Free one in Project Settings > Tags and Layers.");
            return 0;
        }
    }
}
