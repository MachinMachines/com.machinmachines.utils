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
using System.Linq;

using MachinMachines.Utils;

using UnityEngine;
using UnityEngine.Profiling;

namespace MachinMachines
{
    namespace Common
    {
        namespace Repository
        {
            public enum RepositoryUsage
            {
                Texture = 0,
                Material = 1
            }

            // Singleton holding a reference to all files repositories
            // There can be multiple ones per type (for instance multiple files for texture repositories)
            // but also multiple types (for instance texture and materials repositories)
            //
            // Notice that it behaves differently between editor/built game:
            // - in editor/PIE we look for all assets of the correct type
            // - in built games we expect FilesRepository to auto-register during their Awake() phase,
            // which means that they have to be referenced by some owning scene object
            public class FilesRepositoryManager
            {
                public const string FileExtension = "filesrepository";

                public static FilesRepositoryManager Instance { get { return _instance; } }
                // Mostly for debug
                public FilesRepository[] Repositories { get { return _repositories.ToArray(); } }

                private static readonly FilesRepositoryManager _instance = new FilesRepositoryManager();
                private HashSet<FilesRepository> _repositories = new HashSet<FilesRepository>();

                public IEnumerable<FilesRepository.Repository> GetRepositoriesForUsage(RepositoryUsage usage)
                {
                    // Return all repositories for onve given usage
                    return _repositories.Where(item => item.Usage == usage)
                                        // All flattened into one big list
                                        .SelectMany(item => item._repositories)
                                        // Ordered by priority
                                        .OrderBy(item => item.Priority);
                }

                // Should only be used for Repository imports
                public void AddRepository(FilesRepository item)
                {
                    _repositories.Add(item);
                }

                // List all possible paths
                public IEnumerable<string> GetAllPaths(RepositoryUsage usage)
                {
                    return GetRepositoriesForUsage(usage).Select(item => item.Path);
                }

                // Return true if the given path is contained within all repositories
                public bool ContainsPath(RepositoryUsage usage, string path)
                {
                    foreach (FilesRepository.Repository repo in GetRepositoriesForUsage(usage))
                    {
                        if (repo.Path.StartsWith(path, System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                // Main entry point: given a partial file path,
                // retrieve the first full path that matches an existing file
                //
                // External code using it (e.g. texture pool) tends to have a surrounding tringlerie,
                // so make sure you are not bypassing something by calling it directly!
                //
                // editorOnly: only looks in "editor" folders (basically, those starting with "Assets/" or "Packages/")
                public string GetFullPathFrom(RepositoryUsage usage, string relativePath, bool editorOnly = false)
                {
                    bool isDirectoryPath = string.IsNullOrEmpty(Path.GetExtension(relativePath));
                    foreach (FilesRepository.Repository repo in GetRepositoriesForUsage(usage))
                    {
                        if (editorOnly && !PathIsInEditor(repo.Path))
                        {
                            continue;
                        }
                        // Not really "absolute" as it can be relative to the project root
                        string absolutePath = Path.Combine(repo.Path, relativePath);
                        bool exist = false;
                        if (isDirectoryPath)
                        {
                            exist = Directory.Exists(absolutePath);
                        }
                        else
                        {
                            exist = File.Exists(absolutePath);
                        }
                        if (exist)
                        {
                            return absolutePath;
                        }
                    }

                    return null;
                }

                // Secondary entry point: given an absolute file path,
                // retrieve the path relative to the first entry found that matches
                //
                // If no repository entry can be found, it returns null
                public string GetRelativePathFrom(RepositoryUsage usage, string path)
                {
                    foreach (FilesRepository.Repository repo in GetRepositoriesForUsage(usage))
                    {
                        if (Paths.PathStartsWith(path, repo.Path))
                        {
                            return Paths.GetRelativePath(repo.Path + "/", path);
                        }
                        // Support for local files: also check absolute path for local files
                        if (Paths.PathStartsWith(repo.Path, "Assets"))
                        {
                            if (Paths.PathStartsWith(path, Application.dataPath))
                            {
                                string fullAssetsPath = Path.Combine(Application.dataPath, "..", repo.Path + "/");
                                return Paths.GetRelativePath(fullAssetsPath, path);
                            }
                        }
                    }
                    return null;
                }

                // True for paths in Assets/
                static public bool PathIsInEditor(string path)
                {
                    return Paths.PathStartsWith(path, "Assets/") || Paths.PathStartsWith(path, "Packages/");
                }

                // SLOW function: retrieve and cache all repository description assets
                public void FindAllRepositories()
                {
                    Profiler.BeginSample("FilesRepositoryManager - FindAllRepositories");
#if UNITY_EDITOR
                    foreach (string GUID in UnityEditor.AssetDatabase.FindAssets("t:StudioManette.Bob.Repository.FilesRepository"))
                    {
                        string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(GUID);
                        if (!string.IsNullOrEmpty(assetPath))
                        {
                            FilesRepository filesRepository = UnityEditor.AssetDatabase.LoadAssetAtPath<FilesRepository>(assetPath);
                            if (filesRepository != null)
                            {
                                _repositories.Add(filesRepository);
                            }
                        }
                    }
#else
                    // Nothing here, repositories have to be referenced by a scene object and will auto-register during their Awake()
#endif  // UNITY_EDITOR

                    Profiler.EndSample();
                }

                private FilesRepositoryManager()
                {
                    FindAllRepositories();
                }
            }

            // This helper class manages a list of "files repository",
            // ie locations on disk to look for when looking for partial file paths
            [System.Serializable]
            public class FilesRepository : ScriptableObject
            {
                public RepositoryUsage Usage;

                // A file repository is a simple absolute path and a priority
                [System.Serializable]
                public struct Repository
                {
                    [IsFilepath(false, "")]
                    public string Path;
                    public int Priority;
                }

                // All repositories are stored here and always maintained sorted by priority order
                public Repository[] _repositories;

                private void Awake()
                {
                    // Register itself into the main manager
                    FilesRepositoryManager.Instance.AddRepository(this);
                }

                //#if UNITY_EDITOR
                //                [UnityEditor.MenuItem("Assets/Create/Studio Manette/FileRepositoryTest")]
                //                static void CreateTestFile()
                //                {
                //                    FilesRepository data = CreateInstance<FilesRepository>();
                //                    data.Usage = RepositoryUsage.Material;
                //                    data._repositories = new Repository[] { new Repository { Path = "TOTO", Priority = 1 } };
                //                    using (System.IO.StreamWriter stream = new StreamWriter("Assets/toto.filesrepository"))
                //                    {
                //                        stream.Write(JsonUtility.ToJson(data, true));
                //                    }
                //                }
                //#endif
            }
        }
    }
}
