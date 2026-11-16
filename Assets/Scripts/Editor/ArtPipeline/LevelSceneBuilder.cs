using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using WrongSuspect.Characters;
using WrongSuspect.Detective;
using WrongSuspect.Dialogue;
using WrongSuspect.Events;
using WrongSuspect.Game;
using WrongSuspect.Interactables;
using WrongSuspect.Investigation;
using WrongSuspect.Navigation;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>
    /// Creates one playable scene per level (Assets/Scenes/Levels), furnished like the Level_Design.md maps:
    /// camera with click detection, EventSystem, HUD, the room (walls + NavGrid), props, evidence and the culprit,
    /// a GameController (timer, case file, Sherlock's investigation, restart) and Sherlock waiting at the entrance.
    /// The scenes are put first in the build's scene list so they can be restarted.
    /// </summary>
    public static class LevelSceneBuilder
    {
        public const string Folder = "Assets/Scenes/Levels";
        private const string Portraits = "Assets/Art/Sprites/Portraits/";
        private static readonly Color Background = new Color(0.07f, 0.06f, 0.09f);
        private const int IgnoreRaycastLayer = 2;

        public static void BuildAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Dictionary<string, string> prefabs = FindPrefabs();
            AssetFolders.Ensure(Folder);
            var scenePaths = new List<string>();
            foreach (LayoutEntry layout in ArtManifest.Data.layouts)
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                AddCamera(layout);
                AddGlobalLight2DIfNeeded();
                AddEventSystem();
                Spawn(prefabs, HudPrefabBuilder.HudName, scene);

                var root = new GameObject(layout.title);
                var placed = new Dictionary<string, GameObject>();
                foreach (LayoutItem item in layout.items)
                {
                    GameObject instance = Place(item, prefabs, root.transform, scene);
                    if (instance != null && !placed.ContainsKey(item.prefab))
                    {
                        placed[item.prefab] = instance;
                    }
                }

                CaseEntry caseEntry = ArtManifest.CaseFor(layout.level);
                GameObject sherlock = caseEntry != null ? AddSherlock(prefabs, caseEntry, root.transform, scene) : null;
                ConnectWalkersToRoom(root);
                AddGameController(layout, caseEntry, placed, sherlock);

                string path = $"{Folder}/{layout.scene}.unity";
                EditorSceneManager.SaveScene(scene, path);
                scenePaths.Add(path);
            }
            PutScenesFirstInBuild(scenePaths);
            Debug.Log($"[Wrong Suspect] Level scenes saved in {Folder} and added to the build's scene list.");
        }

        // ------------------------------------------------------------------ room contents
        private static Dictionary<string, string> FindPrefabs()
        {
            var result = new Dictionary<string, string>();
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                result[Path.GetFileNameWithoutExtension(path)] = path;
            }
            return result;
        }

        private static GameObject Spawn(Dictionary<string, string> prefabs, string prefabName, UnityEngine.SceneManagement.Scene scene)
        {
            GameObject prefab = prefabs.TryGetValue(prefabName, out string path) ? AssetDatabase.LoadAssetAtPath<GameObject>(path) : null;
            if (prefab == null)
            {
                Debug.LogWarning($"[Wrong Suspect] Prefab missing: {prefabName}. Run Build Art Assets first.");
                return null;
            }
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        }

        private static GameObject Place(LayoutItem item, Dictionary<string, string> prefabs, Transform parent, UnityEngine.SceneManagement.Scene scene)
        {
            GameObject instance = Spawn(prefabs, item.prefab, scene);
            if (instance == null)
            {
                return null;
            }
            instance.transform.SetParent(parent, false);
            instance.transform.position = new Vector3(item.x, item.y, 0f);

            if (item.open && instance.TryGetComponent(out OpenableAnimator openable))
            {
                SetBool(openable, "startOpen", true);
            }
            if (item.standOffset != null && item.standOffset.Length == 2 && instance.TryGetComponent(out Interactable interactable))
            {
                var so = new SerializedObject(interactable);
                so.FindProperty("standOffset").vector2Value = new Vector2(item.standOffset[0], item.standOffset[1]);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            if (item.facing != null && item.facing.Length == 2 && instance.TryGetComponent(out CharacterAnimator character))
            {
                SetFacing(character, new Vector2(item.facing[0], item.facing[1]));
            }
            return instance;
        }

        /// <summary>Sherlock waits, switched off, at the room's entrance until the investigation starts.</summary>
        private static GameObject AddSherlock(Dictionary<string, string> prefabs, CaseEntry caseEntry, Transform parent, UnityEngine.SceneManagement.Scene scene)
        {
            GameObject sherlock = Spawn(prefabs, "Sherlock", scene);
            if (sherlock == null)
            {
                return null;
            }
            sherlock.transform.SetParent(parent, false);
            sherlock.transform.position = new Vector3(caseEntry.entrance[0], caseEntry.entrance[1], 0f);
            SetFacing(sherlock.GetComponent<CharacterAnimator>(), Vector2.up);
            sherlock.SetActive(false);
            return sherlock;
        }

        /// <summary>Gives every PathMover (the culprit and Sherlock) the room's NavGrid.</summary>
        private static void ConnectWalkersToRoom(GameObject root)
        {
            var grid = root.GetComponentInChildren<NavGrid>(true);
            if (grid == null)
            {
                return;
            }
            foreach (PathMover mover in root.GetComponentsInChildren<PathMover>(true))
            {
                PrefabParts.SetReference(mover, "navGrid", grid);
            }
        }

        // ------------------------------------------------------------------ game controller
        private static void AddGameController(LayoutEntry layout, CaseEntry caseEntry, Dictionary<string, GameObject> placed, GameObject sherlock)
        {
            var controller = new GameObject("GameController");

            var timer = new SerializedObject(controller.AddComponent<PreparationTimer>());
            timer.FindProperty("totalSeconds").floatValue = layout.preparationSeconds;
            timer.FindProperty("playerWalkingChanged").objectReferenceValue = DataAssetBuilder.LoadEvent<BoolEvent>(EventNames.PlayerWalkingChanged);
            timer.FindProperty("timeSpent").objectReferenceValue = DataAssetBuilder.LoadEvent<FloatEvent>(EventNames.TimeSpent);
            timer.FindProperty("timerChanged").objectReferenceValue = DataAssetBuilder.LoadEvent<TimerEvent>(EventNames.PreparationTimerChanged);
            timer.FindProperty("preparationEnded").objectReferenceValue = DataAssetBuilder.LoadEvent<GameEvent>(EventNames.PreparationEnded);
            timer.ApplyModifiedPropertiesWithoutUndo();

            var flow = new SerializedObject(controller.AddComponent<LevelFlow>());
            flow.FindProperty("restartRequested").objectReferenceValue = DataAssetBuilder.LoadEvent<GameEvent>(EventNames.RestartRequested);
            flow.FindProperty("nextLevelRequested").objectReferenceValue = DataAssetBuilder.LoadEvent<GameEvent>(EventNames.NextLevelRequested);
            flow.FindProperty("levelCount").intValue = ArtManifest.Data.layouts.Length;
            flow.ApplyModifiedPropertiesWithoutUndo();

            if (caseEntry == null || sherlock == null)
            {
                return;
            }
            CaseFile caseFile = AddCaseFile(controller, caseEntry, placed);
            AddInvestigation(controller, caseFile, sherlock);
        }

        private static CaseFile AddCaseFile(GameObject controller, CaseEntry entry, Dictionary<string, GameObject> placed)
        {
            var caseFile = controller.AddComponent<CaseFile>();
            var so = new SerializedObject(caseFile);
            so.FindProperty("culprit").objectReferenceValue = FindCulprit();

            SerializedProperty suspects = so.FindProperty("suspects");
            suspects.arraySize = entry.suspects.Length;
            for (int i = 0; i < entry.suspects.Length; i++)
            {
                suspects.GetArrayElementAtIndex(i).objectReferenceValue = DataAssetBuilder.LoadSuspect(entry.suspects[i]);
            }

            SerializedProperty alibis = so.FindProperty("alibiOptions");
            alibis.arraySize = entry.alibiOptions.Length;
            for (int i = 0; i < entry.alibiOptions.Length; i++)
            {
                alibis.GetArrayElementAtIndex(i).stringValue = entry.alibiOptions[i];
            }

            SerializedProperty clues = so.FindProperty("clues");
            clues.arraySize = entry.clues.Length;
            for (int i = 0; i < entry.clues.Length; i++)
            {
                ClueEntry c = entry.clues[i];
                SerializedProperty rule = clues.GetArrayElementAtIndex(i);
                rule.FindPropertyRelative("line").stringValue = c.line;
                rule.FindPropertyRelative("condition").enumValueIndex = (int)Enum.Parse(typeof(ClueRule.Condition), c.condition);
                rule.FindPropertyRelative("item").objectReferenceValue = ComponentOn<Carriable>(placed, c.item);
                rule.FindPropertyRelative("place").objectReferenceValue = ComponentOn<Transform>(placed, c.place);
                rule.FindPropertyRelative("radius").floatValue = c.radius;
                rule.FindPropertyRelative("openable").objectReferenceValue = ComponentOn<OpenableAnimator>(placed, c.openable);
                rule.FindPropertyRelative("alibiIndex").intValue = c.alibiIndex;
                rule.FindPropertyRelative("suspect").objectReferenceValue = DataAssetBuilder.LoadSuspect(c.suspect);
                rule.FindPropertyRelative("weight").intValue = c.weight;
                rule.FindPropertyRelative("contradiction").boolValue = c.contradiction;
                rule.FindPropertyRelative("lookAt").objectReferenceValue = ComponentOn<Transform>(placed, c.lookAt);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return caseFile;
        }

        private static void AddInvestigation(GameObject controller, CaseFile caseFile, GameObject sherlock)
        {
            var investigation = controller.AddComponent<SherlockInvestigation>();
            var so = new SerializedObject(investigation);
            so.FindProperty("caseFile").objectReferenceValue = caseFile;
            so.FindProperty("sherlock").objectReferenceValue = sherlock;
            DoorPassage[] doorways = sherlock.transform.parent.GetComponentsInChildren<DoorPassage>(true);   // all doors in the level
            SerializedProperty doors = so.FindProperty("doors");
            doors.arraySize = doorways.Length;
            for (int i = 0; i < doorways.Length; i++)
            {
                doors.GetArrayElementAtIndex(i).objectReferenceValue = doorways[i].GetComponent<OpenableAnimator>();
            }
            so.FindProperty("sherlockMover").objectReferenceValue = sherlock.GetComponent<PathMover>();
            so.FindProperty("sherlockPoses").objectReferenceValue = sherlock.GetComponent<DetectiveAnimator>();
            so.FindProperty("sherlockBody").objectReferenceValue = sherlock.GetComponent<CharacterAnimator>();
            so.FindProperty("portrait").objectReferenceValue = Portrait("Portrait_Sherlock_Keen");
            so.FindProperty("thinkingPortrait").objectReferenceValue = Portrait("Portrait_Sherlock_Thinking");
            so.FindProperty("triumphantPortrait").objectReferenceValue = Portrait("Portrait_Sherlock_Triumphant");
            so.FindProperty("preparationEnded").objectReferenceValue = DataAssetBuilder.LoadEvent<GameEvent>(EventNames.PreparationEnded);
            so.FindProperty("alibiChosen").objectReferenceValue = DataAssetBuilder.LoadEvent<IntEvent>(EventNames.AlibiChosen);
            so.FindProperty("alibiRequested").objectReferenceValue = DataAssetBuilder.LoadEvent<AlibiRequestEvent>(EventNames.AlibiRequested);
            so.FindProperty("dialogue").objectReferenceValue = DataAssetBuilder.LoadEvent<DialogueEvent>(EventNames.DialogueLine);
            so.FindProperty("verdictReady").objectReferenceValue = DataAssetBuilder.LoadEvent<VerdictEvent>(EventNames.VerdictReady);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SuspectProfile FindCulprit()
        {
            foreach (SuspectEntry s in ArtManifest.Data.suspects)
            {
                if (s.culprit)
                {
                    return DataAssetBuilder.LoadSuspect(s.name);
                }
            }
            return null;
        }

        private static T ComponentOn<T>(Dictionary<string, GameObject> placed, string objectName) where T : Component
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return null;
            }
            if (!placed.TryGetValue(objectName, out GameObject go))
            {
                Debug.LogWarning($"[Wrong Suspect] Case rule refers to {objectName}, which is not in this level.");
                return null;
            }
            return go.GetComponent<T>();
        }

        private static Sprite Portrait(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{Portraits}{name}.png");

        private static void SetBool(UnityEngine.Object target, string field, bool value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFacing(CharacterAnimator character, Vector2 facing)
        {
            if (character == null)
            {
                return;
            }
            var so = new SerializedObject(character);
            so.FindProperty("startFacing").vector2Value = facing;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ------------------------------------------------------------------ scene basics
        private static void AddCamera(LayoutEntry layout)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = layout.height * 0.5f + 0.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
            go.transform.position = new Vector3(layout.width * 0.5f, layout.height * 0.5f, -10f);
            go.AddComponent<AudioListener>();
            // lets clicks and hover reach the objects and the floor; walls and furniture footprints are skipped
            go.AddComponent<Physics2DRaycaster>().eventMask = ~((1 << IgnoreRaycastLayer) | (1 << LayerSetup.EnsureObstaclesLayer()));
        }

        private static void AddEventSystem()
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        /// <summary>With the URP 2D Renderer, lit sprites are black without a light, so add a Global Light 2D when URP is in use.</summary>
        private static void AddGlobalLight2DIfNeeded()
        {
            Type lightType = Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
            if (GraphicsSettings.currentRenderPipeline == null || lightType == null)
            {
                return;
            }
            try
            {
                Component light = new GameObject("Global Light 2D").AddComponent(lightType);
                var property = lightType.GetProperty("lightType");
                if (property != null && property.CanWrite)
                {
                    property.SetValue(light, Enum.Parse(property.PropertyType, "Global"));
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Wrong Suspect] Add a Global Light 2D by hand if sprites look black ({e.Message}).");
            }
        }

        /// <summary>Level scenes go first, in order, so "Next level" after level 4 wraps around to level 1.</summary>
        private static void PutScenesFirstInBuild(List<string> levelScenes)
        {
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string path in levelScenes)
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }
            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                if (!levelScenes.Contains(existing.path))
                {
                    scenes.Add(existing);
                }
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
