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

using System;
using System.IO;

using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using UnityEngine;

namespace MachinMachines.Utils.Settings
{
    /// <summary>
    /// Just a plain and stupid way to store file paths for settings we are interested to copy over
    /// Notice that editor settings are not in there
    /// Also, if ever a new settings class is being created, you will need to add it here manually
    /// </summary>
    public static class SettingsPaths
    {
        internal static readonly (string, string)[] s_Paths = new (string, string)[] {
                };
    }

    /// <summary>
    /// Prebuild step to make sure default assets are copied to StreamingAssets/
    /// </summary>
    public class SettingsPreBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            try
            {
                if (!Directory.Exists(Application.streamingAssetsPath))
                {
                    Directory.CreateDirectory(Application.streamingAssetsPath);
                }
                foreach ((string editorPath, string runtimePath) in SettingsPaths.s_Paths)
                {
                    string destFolder = Path.GetDirectoryName(runtimePath);
                    if (!Directory.Exists(destFolder))
                    {
                        Directory.CreateDirectory(destFolder);
                    }
                    File.Copy(editorPath, runtimePath, true);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Something went wrong when copying settings paths: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Postbuild step to make sure StreamingAssets is being emptied
    /// </summary>
    public class SettingsPostBuild : IPostprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        void IPostprocessBuildWithReport.OnPostprocessBuild(BuildReport report)
        {
            try
            {
                foreach ((string _, string runtimePath) in SettingsPaths.s_Paths)
                {
                    if (File.Exists(runtimePath))
                    {
                        File.Delete(runtimePath);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Something went wrong when deleting StreamingAssets directory after the build: {e.Message}");
            }
        }
    }
}
