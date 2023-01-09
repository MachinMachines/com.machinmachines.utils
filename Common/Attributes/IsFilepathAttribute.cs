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

using UnityEngine;

namespace MachinMachines.Utils
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class IsFilepathAttribute : PropertyAttribute
    {
        public enum FileType
        {
            File,
            Directory,
            // This allows to bypass the "fully qualified check" of settings, please use responsibly!
            RootDirectory
        }

        public readonly bool IsReadOnly;
        public readonly string ExtensionMatch;
        public readonly FileType Filetype;

        public IsFilepathAttribute(bool isReadOnly, string extensionMatch = "", FileType filetype = FileType.File)
        {
            IsReadOnly = isReadOnly;
            ExtensionMatch = extensionMatch;
            Filetype = filetype;
        }
    }
}
