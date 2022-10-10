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
using System.Collections.Generic;
using System.IO;

namespace MachinMachines
{
    namespace Utils
    {
        // Named "Paths" so there is no collision with System.IO.Path
        public static class Paths
        {
            public class PathComparer : IEqualityComparer<string>
            {
                public bool Equals(string lhs, string rhs)
                {
                    // Check whether the objects are the same object.
                    if (lhs.Equals(rhs))
                    {
                        return true;
                    }
                    return NormalisePath(lhs).Equals(NormalisePath(rhs), StringComparison.OrdinalIgnoreCase);
                }

                public int GetHashCode(string obj)
                {
                    return NormalisePath(obj).GetHashCode();
                }
            }

            public static string NormalisePath(string value)
            {
                return value.Replace('\\', '/');
            }

            // Equivalent of string.pathStartsWith suitable for paths
            public static bool PathStartsWith(this string lhs, string rhs)
            {
                return NormalisePath(lhs).StartsWith(NormalisePath(rhs), StringComparison.OrdinalIgnoreCase);
            }

            // Equivalent of string.Equals() suitable for paths
            public static bool PathsAreEquivalent(string lhs, string rhs)
            {
                PathComparer cmp = new PathComparer();
                return cmp.Equals(lhs, rhs);
            }

            // From the given string input, retrieve a string suitable for a file name
            // For instance this string: Toto"a>b<c
            // will yield: Toto_a_b_c
            public static string SanitiseFilename(string input)
            {
                return String.Join("_", input.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
            }

            // Attemps to retrieve the path "toPath" relative from "fromPath"
            //
            // Typical example
            // with:
            // fromPath = Application.dataPath (= "D:/MachinMachines/Assets")
            // toPath = "D:/MachinMachines/Assets/Renderer/Textures/HDRI/HDRI_01.psd"
            // it yields:
            // "Assets\Renderer\Textures\HDRI\HDRI_01.psd"
            //
            // Beware, it can get tricky!
            //
            // GOTCHA: beware of trailing slashes!
            // With:
            // fromPath = "D:/MachinMachines/Assets/" (exactly the same as the above example + a trailing "/")
            // toPath = "D:/MachinMachines/Assets/Renderer/Textures/HDRI/HDRI_01.psd"
            // it yields:
            // "Renderer\Textures\HDRI\HDRI_01.psd"
            //
            // GOTCHA: matching seemingly unrelated paths
            // With:
            // fromPath = Application.dataPath (= "D:/MachinMachines/Assets")
            // toPath = "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipelineResources/Texture/BlueNoise16/L/LDR_LLL1_25.png"
            // it yields:
            // "Library\PackageCache\com.unity.render-pipelines.high-definition@12.1.6\Runtime\RenderPipelineResources\Texture\BlueNoise16\L\LDR_LLL1_25.png"
            public static string GetRelativePath(string fromPath, string toPath)
            {
                // Yay, yet another shoutout to https://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
                if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
                if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

                Uri fromUri = new Uri(Path.GetFullPath(fromPath));
                Uri toUri = new Uri(Path.GetFullPath(toPath));

                if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

                Uri relativeUri = fromUri.MakeRelativeUri(toUri);
                string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

                if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
                {
                    relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                }

                return relativePath;
            }

#if UNITY_EDITOR
            // Inspired from TextMesh Pro similar function
            // No runtime version!
            public static string GetPackageRelativePath(string packageFullName)
            {
                // Check for potential UPM package
                string packagePath = Path.GetFullPath($"Packages/{packageFullName}");
                if (Directory.Exists(packagePath))
                {
                    return packagePath;
                }

                packagePath = Path.GetFullPath("Assets/..");
                if (Directory.Exists(packagePath))
                {
                    // Search default location for development package
                    if (Directory.Exists($"{packagePath}/Assets/Packages/{packageFullName}"))
                    {
                        return $"Assets/Packages/{packageFullName}";
                    }
                }

                return null;
            }
#endif  // UNITY_EDITOR

            // Given two sets of files, "reconcile" them similarly to P4 reconcile:
            // - identify files present in both sets ("similar")
            // - identify files present in the set to be reconciled with only ("added")
            // - identify files present in the existing set only ("removed")
            public static void ReconcileFiles(IEnumerable<string> existingSet,
                                              IEnumerable<string> toReconcileWithSet,
                                              out HashSet<string> similarSet,
                                              out HashSet<string> addedSet,
                                              out HashSet<string> removedSet)
            {
                similarSet = new HashSet<string>(toReconcileWithSet);
                addedSet = new HashSet<string>(toReconcileWithSet);
                removedSet = new HashSet<string>(existingSet);
                similarSet.IntersectWith(existingSet);
                addedSet.ExceptWith(similarSet);
                removedSet.ExceptWith(similarSet);
            }

            // For a given path,
            // get all parent directories (from innermost to outermost) until the given root is met
            public static IEnumerable<string> GetParentDirectories(string path, string rootPath)
            {
                List<string> result = new List<string>();
                string current = Path.GetDirectoryName(path);
                while (!string.IsNullOrEmpty(current) && !Paths.PathsAreEquivalent(current, rootPath))
                {
                    result.Add(current);
                    current = Path.GetDirectoryName(current);
                }
                return result;
            }

            // For multiple input paths,
            // get all parent directories (from innermost to outermost) until the given root is met
            public static IEnumerable<string> GetParentDirectories(IEnumerable<string> paths, string pathRoot)
            {
                HashSet<string> parentDirectories = new HashSet<string>();
                foreach (string path in paths)
                {
                    parentDirectories.UnionWith(Paths.GetParentDirectories(path, pathRoot));
                }
                return parentDirectories;
            }
        }
    }
}
