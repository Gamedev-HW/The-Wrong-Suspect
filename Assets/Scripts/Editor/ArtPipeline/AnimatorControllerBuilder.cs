using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using WrongSuspect.Characters;
using WrongSuspect.Interactables;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>
    /// Builds the Animator Controllers described in the manifest:
    /// characters (4-direction blend trees + actions), openable props (Closed/Opening/Closing),
    /// single looping/one-shot clips, and on/off toggles.
    /// </summary>
    public static class AnimatorControllerBuilder
    {
        public static void BuildAll(bool overwrite)
        {
            foreach (ControllerEntry entry in ArtManifest.Data.controllers)
            {
                string path = PathOf(entry);
                if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null)
                {
                    if (!overwrite)
                    {
                        continue;
                    }
                    AssetDatabase.DeleteAsset(path);
                }

                AssetFolders.Ensure(entry.folder);
                AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
                switch (entry.kind)
                {
                    case "character": BuildCharacter(controller, entry); break;
                    case "openable": BuildOpenable(controller, entry.clip); break;
                    case "toggle": BuildToggle(controller, entry.clip, entry.parameter); break;
                    default: BuildSingle(controller, entry.clip); break;
                }
                EditorUtility.SetDirty(controller);
            }
        }

        public static RuntimeAnimatorController Load(string controllerName)
        {
            foreach (ControllerEntry entry in ArtManifest.Data.controllers)
            {
                if (entry.name == controllerName)
                {
                    return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PathOf(entry));
                }
            }
            Debug.LogError($"[Wrong Suspect] No Animator Controller named {controllerName} in the manifest.");
            return null;
        }

        private static string PathOf(ControllerEntry entry) => $"{entry.folder}/{entry.name}.controller";

        // ------------------------------------------------------------------ characters
        private static void BuildCharacter(AnimatorController controller, ControllerEntry entry)
        {
            // Add the floats first: CreateBlendTreeInController picks the first float as its default parameter.
            foreach (string p in new[] { CharacterAnimator.MoveXParameter, CharacterAnimator.MoveYParameter,
                         CharacterAnimator.LastXParameter, CharacterAnimator.LastYParameter, CharacterAnimator.SpeedParameter })
            {
                controller.AddParameter(p, AnimatorControllerParameterType.Float);
            }

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState idle = Directional(controller, "Idle", $"{entry.clip}_Idle", CharacterAnimator.LastXParameter, CharacterAnimator.LastYParameter);
            AnimatorState walk = Directional(controller, "Walk", $"{entry.clip}_Walk", CharacterAnimator.MoveXParameter, CharacterAnimator.MoveYParameter);
            machine.defaultState = idle;
            Instant(idle, walk).AddCondition(AnimatorConditionMode.Greater, 0.01f, CharacterAnimator.SpeedParameter);
            Instant(walk, idle).AddCondition(AnimatorConditionMode.Less, 0.01f, CharacterAnimator.SpeedParameter);

            foreach (ActionEntry action in entry.actions)
            {
                AddAction(controller, machine, entry.clip, action, idle, walk);
            }
            Arrange(machine);
        }

        /// <summary>Parameter name = animation name. Bool actions loop until turned off; trigger actions play once.</summary>
        private static void AddAction(AnimatorController controller, AnimatorStateMachine machine, string prefix,
            ActionEntry action, AnimatorState idle, AnimatorState walk)
        {
            string clipPrefix = $"{prefix}_{action.animation}";
            AnimatorState state = ClipLibrary.Contains(clipPrefix + "_Up")
                ? Directional(controller, action.animation, clipPrefix, CharacterAnimator.LastXParameter, CharacterAnimator.LastYParameter)
                : Simple(machine, action.animation, clipPrefix + "_Down");

            if (action.type == "trigger")
            {
                controller.AddParameter(action.animation, AnimatorControllerParameterType.Trigger);
                AnimatorStateTransition fromAny = machine.AddAnyStateTransition(state);
                fromAny.canTransitionToSelf = false;
                fromAny.hasExitTime = false;
                fromAny.duration = 0f;
                fromAny.AddCondition(AnimatorConditionMode.If, 0f, action.animation);
                AfterClip(state, idle);
            }
            else
            {
                controller.AddParameter(action.animation, AnimatorControllerParameterType.Bool);
                Instant(idle, state).AddCondition(AnimatorConditionMode.If, 0f, action.animation);
                Instant(walk, state).AddCondition(AnimatorConditionMode.If, 0f, action.animation);
                Instant(state, idle).AddCondition(AnimatorConditionMode.IfNot, 0f, action.animation);
            }
        }

        // ------------------------------------------------------------------ props & UI
        private static void BuildOpenable(AnimatorController controller, string prefix)
        {
            controller.AddParameter(OpenableAnimator.OpenParameter, AnimatorControllerParameterType.Bool);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState closed = Simple(machine, OpenableAnimator.ClosedState, $"{prefix}_Closed");
            AnimatorState opening = Simple(machine, OpenableAnimator.OpeningState, $"{prefix}_Open");    // holds the open frame
            AnimatorState closing = Simple(machine, OpenableAnimator.ClosingState, $"{prefix}_Close");   // holds the closed frame
            machine.defaultState = closed;
            Instant(closed, opening).AddCondition(AnimatorConditionMode.If, 0f, OpenableAnimator.OpenParameter);
            Instant(closing, opening).AddCondition(AnimatorConditionMode.If, 0f, OpenableAnimator.OpenParameter);
            Instant(opening, closing).AddCondition(AnimatorConditionMode.IfNot, 0f, OpenableAnimator.OpenParameter);
            Arrange(machine);
        }

        private static void BuildSingle(AnimatorController controller, string clipName)
        {
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            machine.defaultState = Simple(machine, "Play", clipName);
        }

        private static void BuildToggle(AnimatorController controller, string clipName, string parameter)
        {
            controller.AddParameter(parameter, AnimatorControllerParameterType.Bool);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState off = machine.AddState("Off");
            AnimatorState on = Simple(machine, parameter, clipName);
            machine.defaultState = off;
            Instant(off, on).AddCondition(AnimatorConditionMode.If, 0f, parameter);
            Instant(on, off).AddCondition(AnimatorConditionMode.IfNot, 0f, parameter);
            Arrange(machine);
        }

        // ------------------------------------------------------------------ helpers
        private static AnimatorState Directional(AnimatorController controller, string stateName, string clipPrefix, string paramX, string paramY)
        {
            AnimatorState state = controller.CreateBlendTreeInController(stateName, out BlendTree tree, 0);
            tree.name = stateName;
            tree.blendType = BlendTreeType.SimpleDirectional2D;
            tree.blendParameter = paramX;
            tree.blendParameterY = paramY;
            tree.AddChild(ClipLibrary.Load(clipPrefix + "_Down"), new Vector2(0f, -1f));
            tree.AddChild(ClipLibrary.Load(clipPrefix + "_Up"), new Vector2(0f, 1f));
            tree.AddChild(ClipLibrary.Load(clipPrefix + "_Left"), new Vector2(-1f, 0f));
            tree.AddChild(ClipLibrary.Load(clipPrefix + "_Right"), new Vector2(1f, 0f));
            return state;
        }

        private static AnimatorState Simple(AnimatorStateMachine machine, string stateName, string clipName)
        {
            AnimatorState state = machine.AddState(stateName);
            state.motion = ClipLibrary.Load(clipName);
            return state;
        }

        private static AnimatorStateTransition Instant(AnimatorState from, AnimatorState to)
        {
            AnimatorStateTransition t = from.AddTransition(to);
            t.hasExitTime = false;
            t.duration = 0f;
            return t;
        }

        private static void AfterClip(AnimatorState from, AnimatorState to)
        {
            AnimatorStateTransition t = from.AddTransition(to);
            t.hasExitTime = true;
            t.exitTime = 1f;
            t.duration = 0f;
        }

        /// <summary>Lays the states out in a grid so the Animator window is readable.</summary>
        private static void Arrange(AnimatorStateMachine machine)
        {
            ChildAnimatorState[] states = machine.states;
            for (int i = 0; i < states.Length; i++)
            {
                states[i].position = new Vector3(300f + (i % 3) * 240f, (i / 3) * 110f, 0f);
            }
            machine.states = states;
            machine.entryPosition = new Vector3(60f, 0f, 0f);
            machine.anyStatePosition = new Vector3(60f, 120f, 0f);
        }
    }
}
