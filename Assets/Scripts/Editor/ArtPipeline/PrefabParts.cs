using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using WrongSuspect.Actions;
using WrongSuspect.Dialogue;
using WrongSuspect.Events;
using WrongSuspect.Interactables;
using WrongSuspect.Navigation;
using WrongSuspect.Player;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>Small building blocks that the prefab builders snap together.</summary>
    public static class PrefabParts
    {
        private const float FootprintWidth = 0.85f;         // share of the sprite width that blocks the floor
        private const string CulpritPortrait = "Assets/Art/Sprites/Portraits/Portrait_Player_Neutral.png";

        public static GameObject SpriteObject(string name, Sprite sprite, int sortingOrder)
        {
            var go = new GameObject(name);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;   // Y-sort by the feet / base, not the sprite centre
            return go;
        }

        public static void AddAnimator(GameObject go, string controllerName)
        {
            go.AddComponent<Animator>().runtimeAnimatorController = AnimatorControllerBuilder.Load(controllerName);
        }

        /// <summary>
        /// Everything the point-and-click culprit needs: a kinematic body (he walks along paths, so no collider is needed)
        /// and the mover / hands / interactor wired to their event assets.
        /// </summary>
        public static void AddPlayerGameplay(GameObject go)
        {
            go.tag = "Player";
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            SetReference(go.AddComponent<PathMover>(), "walkingChanged", DataAssetBuilder.LoadEvent<BoolEvent>(EventNames.PlayerWalkingChanged));
            SetReference(go.AddComponent<PlayerHands>(), "carriedChanged", DataAssetBuilder.LoadEvent<CarriableEvent>(EventNames.CarriedChanged));

            var interactor = go.AddComponent<PlayerInteractor>();
            SetReference(interactor, "interactableSelected", DataAssetBuilder.LoadEvent<InteractableEvent>(EventNames.InteractableSelected));
            SetReference(interactor, "floorClicked", DataAssetBuilder.LoadEvent<Vector2Event>(EventNames.FloorClicked));
            SetReference(interactor, "actionChosen", DataAssetBuilder.LoadEvent<ActionEvent>(EventNames.ActionChosen));
            SetReference(interactor, "preparationEnded", DataAssetBuilder.LoadEvent<GameEvent>(EventNames.PreparationEnded));
            SetReference(interactor, "menuRequested", DataAssetBuilder.LoadEvent<ActionMenuEvent>(EventNames.ActionMenuRequested));
            SetReference(interactor, "menuClosed", DataAssetBuilder.LoadEvent<GameEvent>(EventNames.MenuClosed));
            SetReference(interactor, "timeSpent", DataAssetBuilder.LoadEvent<FloatEvent>(EventNames.TimeSpent));
            SetReference(interactor, "dialogue", DataAssetBuilder.LoadEvent<DialogueEvent>(EventNames.DialogueLine));
            SetReference(interactor, "portrait", AssetDatabase.LoadAssetAtPath<Sprite>(CulpritPortrait));
        }

        /// <summary>A kinematic body and a PathMover, so a character (Sherlock) can walk along NavGrid paths.</summary>
        public static void AddWalker(GameObject go)
        {
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            go.AddComponent<PathMover>();
        }

        /// <summary>
        /// Click collider + outline + highlight + Interactable (+ Carriable / Container).
        /// PointerTarget's events are wired as persistent UnityEvent listeners (visible in the Inspector):
        /// hover → highlight Show/Hide, click → Interactable.Select.
        /// </summary>
        public static void AddInteraction(GameObject go, Sprite outlineSprite, InteractionEntry config)
        {
            var renderer = go.GetComponent<SpriteRenderer>();
            Bounds bounds = renderer.sprite.bounds;

            var click = go.AddComponent<BoxCollider2D>();
            click.isTrigger = true;
            click.size = bounds.size;
            click.offset = bounds.center;

            var outlineGo = new GameObject("Outline");
            outlineGo.transform.SetParent(go.transform, false);
            var outline = outlineGo.AddComponent<SpriteRenderer>();
            outline.sprite = outlineSprite;
            outline.sortingOrder = renderer.sortingOrder + 1;
            outline.spriteSortPoint = SpriteSortPoint.Pivot;
            outline.enabled = false;

            var highlight = go.AddComponent<InteractableHighlight>();
            SetReference(highlight, "outline", outline);
            SetReference(highlight, "style", DataAssetBuilder.LoadHighlightStyle());

            Interactable interactable = AddInteractable(go, config);
            if (config.carriable)
            {
                go.AddComponent<Carriable>();
            }
            if (config.container)
            {
                var so = new SerializedObject(go.AddComponent<Container>());
                so.FindProperty("startsLocked").boolValue = config.locked;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            var pointer = go.AddComponent<PointerTarget>();
            UnityEventTools.AddPersistentListener(pointer.Entered, highlight.Show);
            UnityEventTools.AddPersistentListener(pointer.Exited, highlight.Hide);
            UnityEventTools.AddPersistentListener(pointer.Clicked, interactable.Select);
        }

        private static Interactable AddInteractable(GameObject go, InteractionEntry config)
        {
            var interactable = go.AddComponent<Interactable>();
            var so = new SerializedObject(interactable);
            so.FindProperty("displayName").stringValue = config.displayName;
            so.FindProperty("description").stringValue = config.description;
            so.FindProperty("acceptsPlacedItems").boolValue = config.acceptsItems;
            so.FindProperty("standOffset").vector2Value = ToVector(config.standOffset, new Vector2(0f, -0.7f));
            so.FindProperty("dropOffset").vector2Value = ToVector(config.dropOffset, new Vector2(0.6f, 0f));
            so.FindProperty("selected").objectReferenceValue = DataAssetBuilder.LoadEvent<InteractableEvent>(EventNames.InteractableSelected);

            SerializedProperty actions = so.FindProperty("actions");
            string[] names = config.actions ?? new string[0];
            actions.arraySize = names.Length;
            for (int i = 0; i < names.Length; i++)
            {
                actions.GetArrayElementAtIndex(i).objectReferenceValue = DataAssetBuilder.LoadAction(names[i]);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return interactable;
        }

        /// <summary>A solid box over the bottom part of a piece of furniture, so paths go around it.</summary>
        public static void AddFootprint(GameObject go, float depth)
        {
            if (depth <= 0f)
            {
                return;
            }
            Bounds bounds = go.GetComponent<SpriteRenderer>().sprite.bounds;
            float height = bounds.size.y * depth;
            var box = ObstacleChild(go, "Footprint").AddComponent<BoxCollider2D>();
            box.size = new Vector2(bounds.size.x * FootprintWidth, height);
            box.offset = new Vector2(bounds.center.x, bounds.min.y + height * 0.5f);
        }

        /// <summary>
        /// Wall colliders matching the room art, the NavGrid that paths are searched on,
        /// and a floor click target so the culprit walks wherever the player clicks.
        /// </summary>
        public static void AddRoomNavigation(GameObject room, float[] wallRects)
        {
            Bounds roomBounds = room.GetComponent<SpriteRenderer>().sprite.bounds;
            var floor = room.AddComponent<BoxCollider2D>();
            floor.isTrigger = true;
            floor.size = roomBounds.size;
            floor.offset = roomBounds.center;
            SetReference(room.AddComponent<FloorClickTarget>(), "floorClicked", DataAssetBuilder.LoadEvent<Vector2Event>(EventNames.FloorClicked));

            GameObject walls = ObstacleChild(room, "Walls");
            for (int i = 0; i + 3 < wallRects.Length; i += 4)
            {
                var box = walls.AddComponent<BoxCollider2D>();
                box.size = new Vector2(wallRects[i + 2], wallRects[i + 3]);
                box.offset = new Vector2(wallRects[i] + wallRects[i + 2] * 0.5f, wallRects[i + 1] + wallRects[i + 3] * 0.5f);
            }

            var grid = room.AddComponent<NavGrid>();
            var so = new SerializedObject(grid);
            so.FindProperty("obstacleLayers").intValue = 1 << LayerSetup.EnsureObstaclesLayer();
            so.FindProperty("obstaclesChanged").objectReferenceValue = DataAssetBuilder.LoadEvent<GameEvent>(EventNames.ObstaclesChanged);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// A blocker filling the doorway above the door's base. The door's OpenableAnimator.OpenChanged is wired
        /// (in the Inspector) to DoorPassage.SetOpen, which switches the blocker and tells the NavGrid to rebuild.
        /// </summary>
        public static void AddDoorPassage(GameObject door, float[] size)
        {
            var box = ObstacleChild(door, "DoorwayBlocker").AddComponent<BoxCollider2D>();
            box.size = new Vector2(size[0], size[1]);
            box.offset = new Vector2(0f, size[1] * 0.5f);

            var passage = door.AddComponent<DoorPassage>();
            SetReference(passage, "blocker", box);
            SetReference(passage, "obstaclesChanged", DataAssetBuilder.LoadEvent<GameEvent>(EventNames.ObstaclesChanged));
            UnityEventTools.AddPersistentListener(door.GetComponent<OpenableAnimator>().OpenChanged, passage.SetOpen);
        }

        private static GameObject ObstacleChild(GameObject parent, string name)
        {
            var child = new GameObject(name) { layer = LayerSetup.EnsureObstaclesLayer() };
            child.transform.SetParent(parent.transform, false);
            return child;
        }

        public static Vector2 ToVector(float[] values, Vector2 fallback) =>
            values != null && values.Length == 2 ? new Vector2(values[0], values[1]) : fallback;

        public static void SetReference(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetFloat(Object target, string field, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Saves the temporary object as a prefab (unless one exists and overwrite is off) and destroys it.</summary>
        public static void SaveAndDestroy(GameObject go, string folder, bool overwrite)
        {
            try
            {
                string path = $"{folder}/{go.name}.prefab";
                if (overwrite || AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                {
                    AssetFolders.Ensure(folder);
                    PrefabUtility.SaveAsPrefabAsset(go, path);
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        public static bool Exists(string folder, string name) =>
            AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/{name}.prefab") != null;
    }
}
