using System.IO;
using UnityEditor;

namespace WrongSuspect.ArtPipeline
{
    /// <summary>Creates project folders (and their parents) when they don't exist yet.</summary>
    public static class AssetFolders
    {
        public static void Ensure(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            Ensure(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        /// <summary>PNG files directly inside a folder, as project paths with forward slashes.</summary>
        public static string[] PngsIn(string folder, bool recursive)
        {
            if (!Directory.Exists(folder))
            {
                return new string[0];
            }
            string[] files = Directory.GetFiles(folder, "*.png", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Replace('\\', '/');
            }
            System.Array.Sort(files, System.StringComparer.Ordinal);
            return files;
        }
    }
}
