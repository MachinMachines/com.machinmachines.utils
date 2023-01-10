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

using UnityEditor.AssetImporters;

using UnityEngine;

namespace MachinMachines.Common.Repository
{
    /// <summary>
    /// Import repository asset and notify the global manager for this new object
    /// </summary>
    [ScriptedImporter(1, ".filesrepository")]
    public class FilesRepositoryImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            using (StreamReader stream = new StreamReader(ctx.assetPath))
            {
                // Serialisation from JSON
                FilesRepository data = FilesRepository.CreateInstance<FilesRepository>();
                try
                {
                    JsonUtility.FromJsonOverwrite(stream.ReadToEnd(), data);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.ToString());
                }
                if (data != null)
                {
                    ctx.AddObjectToAsset("data", data);
                    ctx.SetMainObject(data);
                    // Notify global manager
                    FilesRepositoryManager.Instance.AddRepository(data);
                }
            }
        }
    }
}
