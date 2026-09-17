using UnityEditor;
using UnityEngine;

namespace DrawModePlusMLS.Editor
{
    public static class ResourceFinder
    {
        public static string GetTexturePath() => GetPluginPath() + "/Arts/Textures";

        public static string GetPluginPath()
        {
            string[] guids = AssetDatabase.FindAssets("DrawModePlus t:AssemblyDefinitionAsset");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]).Replace('\\', '/');
                if (assetPath.EndsWith("/DrawModePlus.asmdef", System.StringComparison.Ordinal))
                    return assetPath.Substring(0, assetPath.LastIndexOf('/'));
            }

            Debug.LogWarning("DrawModePlusMLS: cannot locate DrawModePlus.asmdef");
            return "Assets/Plugins/DrawModePlusMLS";
        }

        public static Texture2D LoadTexture(string name)
        {
            string path = (GetTexturePath() + "/" + name).Replace('\\', '/');
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
                Debug.LogWarning($"DrawModePlusMLS: texture not found at {path}");

            return texture;
        }
    }
}
