using UnityEngine;
using UnityEngine.Rendering;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>Top-down sorting: whatever is lower on screen is drawn in front.</summary>
    public static class SortingSettings
    {
        // With the URP 2D Renderer, set the same thing on the Renderer 2D Data asset instead
        // (Transparency Sort Mode = Custom Axis, Axis = 0, 1, 0).
        public static void EnableTopDownSorting()
        {
            GraphicsSettings.transparencySortMode = TransparencySortMode.CustomAxis;
            GraphicsSettings.transparencySortAxis = new Vector3(0f, 1f, 0f);
        }
    }
}
