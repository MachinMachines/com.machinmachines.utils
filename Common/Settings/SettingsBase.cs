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

using System.IO;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace MachinMachines
{
    namespace Utils
    {
        namespace Settings
        {
            // Abstract base class for a class of JSON settings that can be used both in-editor or ingame
            // There are two behaviour: in-editor, or in-built game ("runtime")
            // - in editor, if a matching file is found in the project StreamingAssets/ folder, it will be used.
            // If not found a default settings file is located within the host package (so it holds default values)
            // - at runtime (packaged/built game) the settings file is located in StreamingAssets/ so the JSON can be edited manually
            //
            // During the build process there is a hook to copy from either editor location (project specific StreamingAssets/ or in-package)
            // to the eventual runtime location (StreamingAssets/)
            //
            // Child classes only need to hold the actual settings data and nothing else


            abstract public class SettingsWithPath : ScriptableObject
            {
#if UNITY_EDITOR
                // Override this property in the child class if it is defined in another package
                // This is public only so it can be used below so do NOT use it!
                public virtual string PackagePath { get { return Paths.GetPackageRelativePath("com.machinmachines.utils"); } }
                public string EditorSettingsPath { get { return Path.Combine(PackagePath, _editorSettingsPath); } }
                public string OverwriteSettingsPath { get { return _editorSettingsPath; } }
#endif  // UNITY_EDITOR

                public string SettingsPath
                {
                    get
                    {
#if UNITY_EDITOR
                        if (File.Exists(OverwriteSettingsPath))
                        {
                            return OverwriteSettingsPath;
                        }
                        else
                        {
                            return EditorSettingsPath;
                        }
#else
                        return RuntimeSettingsPath;
#endif  // UNITY_EDITOR
                    }
                }

                public string RuntimeSettingsPath { get { return _runtimeSettingsPath; } }

                internal string _editorSettingsPath = "";
                internal string _runtimeSettingsPath = "";
            }

            // IMPORTANT: paths fields in the children settings classes have to be well-formed if they are being used "as is"
            // For instance this would work on Linux, not on Windows:
            // MbxpFolder = "X:/03_ASSETS/07_TEXTURES/MBXP";
            // But this would work anywhere as it is using platform-specific API to fix the path:
            // Path.GetFullPath(MbxpFolder)
            // So beware! There is a tringlerie de l'enfer(tm) to make sure that no full absolute paths are stored in settings

            abstract public class SettingsBase<T> : SettingsWithPath where T : SettingsWithPath
            {
                protected const string kSubFolderName = "Settings";
                // All settings are stored in the same folder, with the file named after their class
                // In editor we read the local folder, at runtime it's in StreamingAssets
                protected static readonly string kRuntimeSettingsPath = Path.Combine(Application.streamingAssetsPath,
                                                                                     kSubFolderName,
                                                                                     $"{typeof(T).Name}.json");
                protected static readonly string kEditorSettingsPath = Path.Combine("Assets",
                                                                                    kSubFolderName,
                                                                                     $"{typeof(T).Name}.json");

                internal static T _instance = null;

                // Static getter for retrieving the instance
                // If no instance is loaded already it does so by reading the JSON file
                // If no file can be found it writes it
                public static T GetOrCreateSettings()
                {
                    if (_instance == null)
                    {
                        _instance = ScriptableObject.CreateInstance<T>();
                        _instance._editorSettingsPath = kEditorSettingsPath;
                        _instance._runtimeSettingsPath = kRuntimeSettingsPath;

                        string directory = Path.GetDirectoryName(_instance.SettingsPath);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }
                        if (File.Exists(_instance.SettingsPath))
                        {
                            JsonUtility.FromJsonOverwrite(File.ReadAllText(_instance.SettingsPath), _instance);
                        }
                        // We always write the file in case the base ScriptableObject gets updated from the code
                        // TODO @gama: decide whether the file or the code is right once and for all!
                        SerialiseToFile();
#if UNITY_EDITOR
                        // This is only useful so the file appears immediately in the projet browser
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
#endif  // UNITY_EDITOR
                        CreateFolders(_instance);
                    }
                    return _instance;
                }

#if UNITY_EDITOR
                // Simple helper to be called from children classes to easily create a local version of the settings file
                static protected void CreateOverrideFile()
                {
                    string overwritePathDirectory = Path.GetDirectoryName(_instance.OverwriteSettingsPath);
                    if (!Directory.Exists(overwritePathDirectory))
                    {
                        Directory.CreateDirectory(overwritePathDirectory);
                    }
                    File.Copy(_instance.EditorSettingsPath, _instance.OverwriteSettingsPath, true);
                    // Make sure to reset the instance
                    _instance = null;
                }
#endif  // UNITY_EDITOR

                public static void SerialiseToFile()
                {
                    File.WriteAllText(_instance.SettingsPath, JsonUtility.ToJson(_instance, true));
                }

                // Tiny helper for settings marked with the "IsFilepath" attribute:
                // it automagically creates all referenced folders
                private static void CreateFolders(T settings)
                {
                    foreach (FieldInfo info in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        IsFilepathAttribute filePathAttribute = System.Attribute.GetCustomAttribute(info, typeof(IsFilepathAttribute)) as IsFilepathAttribute;
                        if (filePathAttribute != null)
                        {
                            string propertyValue = info.GetValue(settings) as string;
                            if (!string.IsNullOrEmpty(propertyValue))
                            {
                                // Make sure the path is relative to prevent the issue talked about in the class header comment
                                if (filePathAttribute.Filetype != IsFilepathAttribute.FileType.RootDirectory
                                    && Path.IsPathFullyQualified(propertyValue))
                                {
                                    Debug.LogError($"Path {propertyValue} for field {info.Name} in settings file {Path.GetFileName(_instance.SettingsPath)} is absolute, this is forbidden to prevent cross-platform issues!");
                                }
                                // Make sure reference folders exist
                                if (!Directory.Exists(propertyValue))
                                {
                                    // Make sure the drive exists before trying to create it
                                    string pathRoot = Path.GetPathRoot(propertyValue);
                                    if (string.IsNullOrEmpty(pathRoot))
                                    {
                                        if (propertyValue.StartsWith("Assets", System.StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            // If we are trying to create some "assets/***" subfolder within a runtime build, just stop
                                            if (!Application.isEditor)
                                            {
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            // We are trying to create some unrooted folder, just stop
                                            continue;
                                        }
                                    }
                                    if (string.IsNullOrEmpty(pathRoot) || Directory.Exists(pathRoot))
                                    {
                                        // This might fail for various reasons (typically access rights),
                                        // so better make sure we catch the potential exception
                                        try
                                        {
                                            Directory.CreateDirectory(propertyValue);
                                        }
                                        catch
                                        {
                                            Debug.LogWarning($"No access rights to create folder {propertyValue}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
