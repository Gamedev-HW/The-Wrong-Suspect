using UnityEditor;
using UnityEngine;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>
    /// Menu: Tools > The Wrong Suspect. Runs the art pipeline steps in order.
    /// (Editor tools are static because Unity's MenuItem requires it; they keep no game state.)
    /// </summary>
    public static class ArtPipelineMenu
    {
        private const string Menu = "Tools/The Wrong Suspect/";

        [MenuItem(Menu + "Build Art Assets", priority = 0)]
        public static void BuildArtAssets() => Run(false);

        [MenuItem(Menu + "Create Level Scenes", priority = 1)]
        public static void CreateLevelScenes()
        {
            ArtManifest.Reload();
            LevelSceneBuilder.BuildAll();
        }

        [MenuItem(Menu + "Rebuild Art Assets (overwrite prefabs and controllers)", priority = 20)]
        public static void RebuildArtAssets()
        {
            if (EditorUtility.DisplayDialog("Rebuild art assets?",
                    "Generated prefabs and Animator Controllers will be replaced. Changes you made to them will be lost.",
                    "Rebuild", "Cancel"))
            {
                Run(true);
            }
        }

        private static void Run(bool overwrite)
        {
            ArtManifest.Reload();
            LayerSetup.EnsureObstaclesLayer();
            SpriteImportSettings.ApplyToAll();
            AnimationClipBuilder.BuildAll();
            AnimatorControllerBuilder.BuildAll(overwrite);
            DataAssetBuilder.BuildAll();
            PrefabBuilder.BuildAll(overwrite);
            UiPrefabBuilder.BuildAll(overwrite);
            HudPrefabBuilder.Build(overwrite);
            SortingSettings.EnableTopDownSorting();
            AssetDatabase.SaveAssets();
            Debug.Log("[Wrong Suspect] Assets ready: clips, Animator Controllers, data assets, prefabs and the HUD. " +
                      "Next: Tools > The Wrong Suspect > Create Level Scenes.");
        }
    }
}
