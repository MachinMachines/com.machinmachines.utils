// Copyright 2022 MachinMachines
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.IO;

using UnityEditor;

using UnityEngine;

namespace MachinMachines.Utils
{
    /// <summary>
    /// Various helpers we added to AssetDatabase
    /// Not implemented as extension methods as there will only be static methods anyway
    /// </summary>
    public static class AssetDatabaseExtensions
    {
        /// <summary>
        /// Given any path (directory or not), retrieve all assets paths below that one
        /// </summary>
        public static string[] GetAllAssetsPathsFromRoot(string root, string extension = "")
        {
            List<string> result = new List<string>();
            if (File.GetAttributes(root).HasFlag(FileAttributes.Directory))
            {
                SearchOption options = SearchOption.AllDirectories;
                string searchPattern = string.IsNullOrEmpty(extension) ? "*.*" : $"*{extension}";
                result.AddRange(Directory.GetFiles(root, searchPattern, options));
            }
            else
            {
                result.Add(root);
            }
            return result.ToArray();
        }

        public static void CreateOrReplaceAsset<T>(Object asset, string path) where T : Object
        {
            T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existingAsset != null)
            {
                EditorUtility.CopySerialized(asset, existingAsset);
                EditorUtility.SetDirty(existingAsset);
            }
            else
            {
                string folderPathStr = "";
                string[] folderPath = path.Split(Path.DirectorySeparatorChar);
                bool copyItems = false;
                for (int i = 0; i < folderPath.Length - 1; ++i)
                {
                    if (copyItems)
                    {
                        folderPathStr += folderPath[i];
                        folderPathStr += Path.DirectorySeparatorChar;
                    }
                    if (folderPath[i] == "Assets")
                    {
                        copyItems = true;
                    }
                }
                Directory.CreateDirectory(Path.Combine(Application.dataPath, folderPathStr));
                AssetDatabase.CreateAsset(asset, path);
            }
        }

        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            return assets;
        }
    }
}
