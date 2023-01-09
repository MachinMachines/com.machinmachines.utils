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

using UnityEngine;

namespace MachinMachines.Utils
{
    /// <summary>
    /// Stolen from Unity Recorder, which itself mentions:
    /// Inspired from http://wiki.unity3d.com/index.php/OpenInFileBrowser
    /// </summary>
    public static class OpenInFileBrowser
    {
        static void OpenInOSX(string path, bool openInsideFolder)
        {
            var osxPath = path.Replace("\\", "/");

            if (!osxPath.StartsWith("\""))
            {
                osxPath = "\"" + osxPath;
            }

            if (!osxPath.EndsWith("\""))
            {
                osxPath = osxPath + "\"";
            }

            var arguments = (openInsideFolder ? "" : "-R ") + osxPath;

            try
            {
                System.Diagnostics.Process.Start("open", arguments);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                // tried to open mac finder in windows
                // just silently skip error
                // we currently have no platform define for the current OS we are in, so we resort to this
                e.HelpLink = ""; // do anything with this variable to silence warning about not using it
            }
        }

        static void OpenInWindows(string path, bool openInsideFolder)
        {
            var winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes

            try
            {
                System.Diagnostics.Process.Start("explorer.exe", (openInsideFolder ? "/root," : "/select,") + winPath);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                // tried to open win explorer in mac
                // just silently skip error
                // we currently have no platform define for the current OS we are in, so we resort to this
                e.HelpLink = ""; // do anything with this variable to silence warning about not using it
            }
        }

        public static void Open(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (!File.Exists(path))
                    path = Path.GetDirectoryName(path);

                var openInsideFolder = Directory.Exists(path);

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    OpenInWindows(path, openInsideFolder);
                }
                else if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    OpenInOSX(path, openInsideFolder);
                }
            }
        }
    }
}
