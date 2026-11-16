using System;
using UnityEditor;
using UnityEngine;
using WrongSuspect.Actions;
using WrongSuspect.Dialogue;
using WrongSuspect.Events;
using WrongSuspect.Interactables;
using WrongSuspect.Investigation;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>
    /// Creates the ScriptableObject assets under Assets/Data: the highlight style, the event assets, the action assets and the suspects.
    /// Existing assets are never overwritten, so values you tune in the Inspector stay.
    /// </summary>
    public static class DataAssetBuilder
    {
        public const string SettingsFolder = "Assets/Data/Settings";
        public const string EventsFolder = "Assets/Data/Events";
        public const string ActionsFolder = "Assets/Data/Actions";
        public const string SuspectsFolder = "Assets/Data/Suspects";
        public const string HighlightStylePath = SettingsFolder + "/HighlightStyle.asset";

        public static void BuildAll()
        {
            AssetFolders.Ensure(SettingsFolder);
            AssetFolders.Ensure(EventsFolder);
            AssetFolders.Ensure(ActionsFolder);
            AssetFolders.Ensure(SuspectsFolder);

            CreateIfMissing(typeof(HighlightStyle), HighlightStylePath);
            foreach (EventAssetEntry entry in ArtManifest.Data.eventAssets)
            {
                Type type = EventType(entry.type);
                if (type != null)
                {
                    CreateIfMissing(type, $"{EventsFolder}/{entry.name}.asset");
                }
            }
            foreach (ActionAssetEntry entry in ArtManifest.Data.actions)
            {
                BuildAction(entry);
            }
            foreach (SuspectEntry entry in ArtManifest.Data.suspects)
            {
                BuildSuspect(entry);
            }
        }

        public static SuspectProfile LoadSuspect(string name) =>
            string.IsNullOrEmpty(name) ? null : AssetDatabase.LoadAssetAtPath<SuspectProfile>($"{SuspectsFolder}/{name}.asset");

        private static void BuildSuspect(SuspectEntry entry)
        {
            string path = $"{SuspectsFolder}/{entry.name}.asset";
            if (AssetDatabase.LoadAssetAtPath<SuspectProfile>(path) != null)
            {
                return;
            }
            var suspect = ScriptableObject.CreateInstance<SuspectProfile>();
            AssetDatabase.CreateAsset(suspect, path);
            var so = new SerializedObject(suspect);
            so.FindProperty("displayName").stringValue = entry.displayName;
            so.FindProperty("portrait").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(entry.portrait);
            so.FindProperty("isCulprit").boolValue = entry.culprit;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static HighlightStyle LoadHighlightStyle() => AssetDatabase.LoadAssetAtPath<HighlightStyle>(HighlightStylePath);

        public static T LoadEvent<T>(string name) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>($"{EventsFolder}/{name}.asset");
            if (asset == null)
            {
                Debug.LogError($"[Wrong Suspect] Event asset missing: {EventsFolder}/{name}.asset");
            }
            return asset;
        }

        public static GameAction LoadAction(string name) => AssetDatabase.LoadAssetAtPath<GameAction>($"{ActionsFolder}/{name}.asset");

        private static void BuildAction(ActionAssetEntry entry)
        {
            string path = $"{ActionsFolder}/{entry.name}.asset";
            Type type = ActionType(entry.type);
            if (type == null || AssetDatabase.LoadAssetAtPath<GameAction>(path) != null)
            {
                return;
            }

            ScriptableObject action = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(action, path);
            var so = new SerializedObject(action);
            so.FindProperty("label").stringValue = entry.label;
            so.FindProperty("icon").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(entry.icon);
            so.FindProperty("timeCost").floatValue = entry.timeCost;
            so.FindProperty("animationSeconds").floatValue = entry.animationSeconds;
            so.FindProperty("busyAnimation").boolValue = entry.busy;
            if (action is InspectAction)
            {
                so.FindProperty("inspected").objectReferenceValue = LoadEvent<InteractableEvent>(EventNames.InteractableInspected);
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Type EventType(string typeName)
        {
            switch (typeName)
            {
                case "GameEvent": return typeof(GameEvent);
                case "TimerEvent": return typeof(TimerEvent);
                case "BoolEvent": return typeof(BoolEvent);
                case "FloatEvent": return typeof(FloatEvent);
                case "InteractableEvent": return typeof(InteractableEvent);
                case "CarriableEvent": return typeof(CarriableEvent);
                case "ActionEvent": return typeof(ActionEvent);
                case "ActionMenuEvent": return typeof(ActionMenuEvent);
                case "IntEvent": return typeof(IntEvent);
                case "Vector2Event": return typeof(Vector2Event);
                case "AlibiRequestEvent": return typeof(AlibiRequestEvent);
                case "DialogueEvent": return typeof(DialogueEvent);
                case "VerdictEvent": return typeof(VerdictEvent);
                default:
                    Debug.LogError($"[Wrong Suspect] Unknown event type in manifest: {typeName}");
                    return null;
            }
        }

        private static Type ActionType(string typeName)
        {
            switch (typeName)
            {
                case "Inspect": return typeof(InspectAction);
                case "PickUp": return typeof(PickUpAction);
                case "Place": return typeof(PlaceAction);
                case "Hide": return typeof(HideAction);
                case "OpenClose": return typeof(OpenCloseAction);
                case "Unlock": return typeof(UnlockAction);
                default:
                    Debug.LogError($"[Wrong Suspect] Unknown action type in manifest: {typeName}");
                    return null;
            }
        }

        private static void CreateIfMissing(Type type, string path)
        {
            if (AssetDatabase.LoadAssetAtPath(path, type) == null)
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance(type), path);
            }
        }
    }
}
