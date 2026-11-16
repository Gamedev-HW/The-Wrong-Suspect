using System;
using System.IO;
using UnityEngine;

namespace WrongSuspect.ArtPipeline
{
    // Data model of Assets/Art/ArtManifest.json (written by the art generator).
    // It only holds what can't be read from file names: frame rates, animator setups, 9-slice borders,
    // which actions each object offers, the event assets, walls and furniture footprints, Sherlock's case rules, and the level layouts.

    [Serializable] public class SliceBorder { public string path; public float[] border; }              // left, bottom, right, top
    [Serializable] public class SortingOverride { public string name; public int order; }
    [Serializable] public class TweenEntry { public string name; public bool loop; public float[] scale; public float[] alpha; }   // [time, value, ...]
    [Serializable] public class ClipEntry { public string name; public string folder; public float fps; public bool loop; public string target; public string[] frames; }
    [Serializable] public class ActionEntry { public string animation; public string type; }            // type: bool | trigger
    [Serializable] public class ControllerEntry { public string name; public string folder; public string kind; public string clip; public string parameter; public ActionEntry[] actions; }
    [Serializable] public class AnimatedPrefabEntry { public string name; public string folder; public string kind; public string sprite; public string highlight; public string controller; public int sortingOrder; public float lifetime; }
    [Serializable] public class FolderPrefabRule { public string spriteFolder; public string prefabFolder; public string kind; public int sortingOrder; }
    [Serializable] public class PocketWatchSprites { public string face; public string faceWarning; public string minuteHand; public string secondHand; public string glow; }
    [Serializable] public class StampEntry { public string name; public string sprite; }
    [Serializable] public class LayoutItem { public string prefab; public float x; public float y; public bool open; public float[] facing; public float[] standOffset; }
    [Serializable] public class LayoutEntry { public int level; public string scene; public string title; public float width; public float height; public float preparationSeconds; public LayoutItem[] items; }
    [Serializable] public class RoomWallEntry { public string room; public float[] rects; }       // x, y, width, height per wall, from the room's bottom-left
    [Serializable] public class ObstacleEntry { public string name; public float depth; }         // share of the sprite height that blocks the floor
    [Serializable] public class DoorPassageEntry { public string name; public float[] size; }               // doorway blocked while the door is closed
    [Serializable] public class SuspectEntry { public string name; public string displayName; public string portrait; public bool culprit; }
    [Serializable] public class ClueEntry { public string line; public string condition; public string suspect; public int weight; public string item; public string place; public float radius; public string openable; public int alibiIndex; public bool contradiction; public string lookAt; }
    [Serializable] public class CaseEntry { public int level; public string[] suspects; public float[] entrance; public string[] alibiOptions; public ClueEntry[] clues; }
    [Serializable] public class EventAssetEntry { public string name; public string type; }
    [Serializable] public class ActionAssetEntry { public string name; public string type; public string label; public string icon; public float timeCost; public float animationSeconds; public bool busy; }
    [Serializable] public class InteractionEntry { public string name; public string displayName; public string description; public string[] actions; public bool carriable; public bool container; public bool locked; public bool acceptsItems; public float[] standOffset; public float[] dropOffset; }

    [Serializable]
    public class ArtManifestData
    {
        public float pixelsPerUnit = 32f;
        public SliceBorder[] sliceBorders = new SliceBorder[0];
        public SortingOverride[] sortingOverrides = new SortingOverride[0];
        public TweenEntry[] tweens = new TweenEntry[0];
        public ClipEntry[] clips = new ClipEntry[0];
        public ControllerEntry[] controllers = new ControllerEntry[0];
        public AnimatedPrefabEntry[] animatedPrefabs = new AnimatedPrefabEntry[0];
        public FolderPrefabRule[] folderPrefabs = new FolderPrefabRule[0];
        public EventAssetEntry[] eventAssets = new EventAssetEntry[0];
        public ActionAssetEntry[] actions = new ActionAssetEntry[0];
        public InteractionEntry[] interactions = new InteractionEntry[0];
        public RoomWallEntry[] roomWalls = new RoomWallEntry[0];
        public ObstacleEntry[] obstacles = new ObstacleEntry[0];
        public DoorPassageEntry[] doorPassages = new DoorPassageEntry[0];
        public SuspectEntry[] suspects = new SuspectEntry[0];
        public CaseEntry[] cases = new CaseEntry[0];
        public PocketWatchSprites pocketWatch = new PocketWatchSprites();
        public StampEntry[] stamps = new StampEntry[0];
        public LayoutEntry[] layouts = new LayoutEntry[0];
    }

    /// <summary>Loads the art manifest once and answers lookups on it.</summary>
    public static class ArtManifest
    {
        public const string ManifestPath = "Assets/Art/ArtManifest.json";
        public const string SpriteRoot = "Assets/Art/Sprites/";

        private static ArtManifestData data;

        public static ArtManifestData Data
        {
            get
            {
                if (data == null)
                {
                    data = Load();
                }
                return data;
            }
        }

        public static void Reload() => data = null;

        public static float[] FindBorder(string spritePath)
        {
            foreach (SliceBorder b in Data.sliceBorders)
            {
                if (b.path == spritePath)
                {
                    return b.border;
                }
            }
            return null;
        }

        public static int SortingOrderFor(string objectName, int fallback)
        {
            foreach (SortingOverride o in Data.sortingOverrides)
            {
                if (o.name == objectName)
                {
                    return o.order;
                }
            }
            return fallback;
        }

        /// <summary>How an interactive object behaves; objects not listed only offer "Inspect".</summary>
        public static InteractionEntry InteractionFor(string objectName)
        {
            foreach (InteractionEntry i in Data.interactions)
            {
                if (i.name == objectName)
                {
                    return i;
                }
            }
            return new InteractionEntry
            {
                name = objectName, displayName = objectName, description = "", actions = new[] { "Inspect" },
                standOffset = new[] { 0f, -0.7f }, dropOffset = new[] { 0.6f, 0f },
            };
        }

        /// <summary>Wall rectangles of a room (x, y, width, height in world units from its bottom-left corner).</summary>
        public static float[] WallsFor(string roomName)
        {
            foreach (RoomWallEntry w in Data.roomWalls)
            {
                if (w.room == roomName)
                {
                    return w.rects;
                }
            }
            return new float[0];
        }

        /// <summary>How much of an object's height blocks the floor (0 = you can walk over it).</summary>
        public static float ObstacleDepthFor(string objectName)
        {
            foreach (ObstacleEntry o in Data.obstacles)
            {
                if (o.name == objectName)
                {
                    return o.depth;
                }
            }
            return 0f;
        }

        /// <summary>Size of the doorway a closed door blocks, or null if the object is not a door.</summary>
        public static float[] DoorPassageFor(string objectName)
        {
            foreach (DoorPassageEntry d in Data.doorPassages)
            {
                if (d.name == objectName)
                {
                    return d.size;
                }
            }
            return null;
        }

        public static CaseEntry CaseFor(int level)
        {
            foreach (CaseEntry c in Data.cases)
            {
                if (c.level == level)
                {
                    return c;
                }
            }
            return null;
        }

        private static ArtManifestData Load()
        {
            string fullPath = Path.GetFullPath(ManifestPath);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"[Wrong Suspect] Art manifest not found at {ManifestPath}.");
                return new ArtManifestData();
            }
            return JsonUtility.FromJson<ArtManifestData>(File.ReadAllText(fullPath)) ?? new ArtManifestData();
        }
    }
}
