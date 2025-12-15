using System;
using UnityEditor;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace DrawModePlusMLS.Editor
{
    public static class ResourceFinder
    {
        public static string GetTexturePath() => GetPluginPath() + "\\Textures";

        public static string GetPluginPath()
        {
            string path = GetScriptPath();
            int index = path.IndexOf("Assets\\", StringComparison.Ordinal);
            if (index < 0)
            {
                Debug.LogWarning($"can not find \"Assets\" :{path}");
                return path; // 没找到 Assets，返回原始值（安全兜底）
            }


            path = path.Substring(index);

            const string rootFolder = "DrawModePlusMLS";
            index = path.IndexOf(rootFolder, StringComparison.Ordinal);
            if (index < 0)
                return path; // 找不到时返回原路径

            // 截到 "DrawModePlusMLS" 的末尾
            var result = path.Substring(0, index + rootFolder.Length);
            return result;
        }

        private static string GetScriptPath([CallerFilePath] string path = "") => path;

        public static Texture2D LoadTexture(string name)
        {
            string path = GetTexturePath() + "\\" + name;
            path = path.Replace("\\", "/");
            // Debug.Log(path);
            Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture2D == null)
            {
                Debug.LogWarning($"LoadTexture : texture2D is null, path: {path}");
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}