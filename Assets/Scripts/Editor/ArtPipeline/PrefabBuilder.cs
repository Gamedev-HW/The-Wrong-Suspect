using System.IO;
using UnityEditor;
using UnityEngine;
using WrongSuspect.Characters;
using WrongSuspect.Detective;
using WrongSuspect.Effects;
using WrongSuspect.Interactables;
using WrongSuspect.Player;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>
    /// Builds the world prefabs.
    /// Animated ones (characters, openable props, ambient props, effects) come from the manifest;
    /// static ones are made from every PNG found in the folders listed in the manifest
    /// (so a new evidence sprite becomes a prefab without touching any code or data).
    /// Furniture gets a footprint collider and rooms get wall colliders, so paths go around them.
    /// </summary>
    public static class PrefabBuilder
    {
        public static void BuildAll(bool overwrite)
        {
            foreach (AnimatedPrefabEntry entry in ArtManifest.Data.animatedPrefabs)
            {
                BuildAnimated(entry, overwrite);
            }
            foreach (FolderPrefabRule rule in ArtManifest.Data.folderPrefabs)
            {
                BuildFolder(rule, overwrite);
            }
        }

        private static void BuildAnimated(AnimatedPrefabEntry entry, bool overwrite)
        {
            if (!overwrite && PrefabParts.Exists(entry.folder, entry.name))
            {
                return;
            }
            Sprite sprite = LoadSprite(entry.sprite);
            if (sprite == null)
            {
                return;
            }

            GameObject go = PrefabParts.SpriteObject(entry.name, sprite, entry.sortingOrder);
            PrefabParts.AddAnimator(go, entry.controller);
            switch (entry.kind)
            {
                case "player":
                    go.AddComponent<CharacterAnimator>();
                    go.AddComponent<PlayerActionAnimator>();
                    PrefabParts.AddPlayerGameplay(go);
                    break;
                case "detective":
                    go.AddComponent<CharacterAnimator>();
                    go.AddComponent<DetectiveAnimator>();
                    PrefabParts.AddWalker(go);
                    break;
                case "openable":
                    go.AddComponent<OpenableAnimator>();
                    PrefabParts.AddInteraction(go, LoadSprite(entry.highlight), ArtManifest.InteractionFor(entry.name));
                    float[] doorway = ArtManifest.DoorPassageFor(entry.name);
                    if (doorway != null)
                    {
                        PrefabParts.AddDoorPassage(go, doorway);
                    }
                    break;
                case "effect_oneshot":
                    PrefabParts.SetFloat(go.AddComponent<OneShotEffect>(), "lifetime", entry.lifetime);
                    break;
            }
            PrefabParts.AddFootprint(go, ArtManifest.ObstacleDepthFor(entry.name));
            PrefabParts.SaveAndDestroy(go, entry.folder, overwrite);
        }

        private static void BuildFolder(FolderPrefabRule rule, bool overwrite)
        {
            foreach (string spritePath in AssetFolders.PngsIn(rule.spriteFolder, false))
            {
                string name = Path.GetFileNameWithoutExtension(spritePath);
                if (OutlineGenerator.IsOutline(spritePath) || (!overwrite && PrefabParts.Exists(rule.prefabFolder, name)))
                {
                    continue;
                }
                Sprite sprite = LoadSprite(spritePath);
                if (sprite == null)
                {
                    continue;
                }

                GameObject go = PrefabParts.SpriteObject(name, sprite, ArtManifest.SortingOrderFor(name, rule.sortingOrder));
                if (rule.kind == "interactable")
                {
                    PrefabParts.AddInteraction(go, LoadSprite(OutlineGenerator.Ensure(spritePath)), ArtManifest.InteractionFor(name));
                }
                if (rule.kind == "room")
                {
                    PrefabParts.AddRoomNavigation(go, ArtManifest.WallsFor(name));
                }
                PrefabParts.AddFootprint(go, ArtManifest.ObstacleDepthFor(name));
                PrefabParts.SaveAndDestroy(go, rule.prefabFolder, overwrite);
            }
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite sprite = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogError($"[Wrong Suspect] Sprite missing or not imported as a Sprite: {path}");
            }
            return sprite;
        }
    }
}
