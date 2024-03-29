// Copyright 2023 MachinMachines
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

        /// <summary>
        /// As its name suggests, nothing fancy here
        /// </summary>
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
                string assetParentDirectory = Path.GetDirectoryName(path);
                if (!Directory.Exists(assetParentDirectory))
                {
                    string assetDirParentDirectory = Path.GetDirectoryName(assetParentDirectory);
                    AssetDatabase.CreateFolder(assetDirParentDirectory,
                                               assetParentDirectory.Substring(assetDirParentDirectory.Length + 1));
                }
                AssetDatabase.CreateAsset(asset, path);
            }
        }

        /// <summary>
        /// Same as AssetDatabase.GUIDFromAssetPath but able to extract it from assets outside the project
        /// To do so it parses the associated metafile to the given path (if found)
        /// </summary>
        public static GUID GUIDFromAssetPath(string assetPath)
        {
            if (File.Exists(assetPath) || Directory.Exists(assetPath))
            {
                string metaFilePath = AssetDatabase.GetTextMetaFilePathFromAssetPath(assetPath);
                if (!string.IsNullOrEmpty(metaFilePath))
                {
                    using (StreamReader stream = new(metaFilePath))
                    {
                        // Skip 1 line
                        stream.ReadLine();
                        string guidHash = stream.ReadLine();
                        // Extract the guid hex manually
                        string[] tokens = guidHash.Split(' ');
                        return new GUID(tokens[1]);
                    }
                }
            }
            return new GUID();
        }

        /// <summary>
        /// Modify the given asset (including folders) GUID by touching its metadata file
        /// </summary>
        public static bool ChangeGUID(string assetPath, GUID newGUID)
        {
            if (File.Exists(assetPath) || Directory.Exists(assetPath))
            {
                string metaFilePath = AssetDatabase.GetTextMetaFilePathFromAssetPath(assetPath);
                if (!string.IsNullOrEmpty(metaFilePath))
                {
                    string[] metaContent;
                    using (StreamReader stream = new(metaFilePath))
                    {
                        metaContent = stream.ReadToEnd().Split('\n',
                                                               System.StringSplitOptions.RemoveEmptyEntries);
                    }
                    string[] hashTokens = metaContent[1].Split(' ');
                    hashTokens[1] = newGUID.ToString();
                    metaContent[1] = string.Join(' ', hashTokens);
                    using (StreamWriter stream = new(metaFilePath))
                    {
                        // Unity uses this style
                        stream.NewLine = "\n";
                        foreach (string line in metaContent)
                        {
                            stream.WriteLine(line);
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
