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
using System.Linq;

using UnityEngine;

namespace MachinMachines.Algorithms
{
    /// <summary>
    /// Helper to compare two links between two hierarchical items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class EqualityComparerHierarchicalItemLink<T> : IEqualityComparer<(HierarchicalTreeItem<T>, HierarchicalTreeItem<T>)> where T : class
    {
        public bool Equals((HierarchicalTreeItem<T>, HierarchicalTreeItem<T>) lhs, (HierarchicalTreeItem<T>, HierarchicalTreeItem<T>) rhs)
        {
            // Check whether the objects are the same object.
            if (lhs.Equals(rhs))
            {
                return true;
            }
            return (lhs.Item1.Name == rhs.Item1.Name
                && lhs.Item2.Name == rhs.Item2.Name)
                || (lhs.Item1.Name == rhs.Item2.Name
                && lhs.Item2.Name == rhs.Item1.Name);
        }

        public int GetHashCode((HierarchicalTreeItem<T>, HierarchicalTreeItem<T>) obj)
        {
            // Cannot use HashCode.Combine() as we need a commutative operation
            return obj.Item1.Name.GetHashCode() ^ obj.Item2.Name.GetHashCode();
        }
    }

    // Super basic class for organising data as a hierarchical tree
    // Only useful as inheriting classes can then be automatically dumped as DGML
    public abstract class HierarchicalTreeItem<T> where T : class
    {
        // To be overridden by inheriting classes
        public abstract string Name { get; }
        // (optional) To be overridden by inheriting classes if need be
        public virtual string Category { get { return ""; } }
        // (optional) To be overridden by inheriting classes if need be
        public virtual Dictionary<string, Color> CategoryToColorMapping { get { return null; } }

        public HierarchicalTreeItem<T>[] children = new HierarchicalTreeItem<T>[0];
        // Same as the above but with the correct derived type
        public IEnumerable<T> DirectChildren
        {
            get
            {
                return children.Select(item => item as T);
            }
        }

        // Generator retrieving all children items, including the root one, as a flattened list
        public IEnumerable<HierarchicalTreeItem<T>> GetAllItems_r()
        {
            yield return this;
            foreach (HierarchicalTreeItem<T> child in children)
            {
                foreach (HierarchicalTreeItem<T> item in child.GetAllItems_r())
                {
                    yield return item;
                }
            }
        }

        // Build a list of all "links" (dependency relationship) between all resources recursively
        public static IEnumerable<(HierarchicalTreeItem<T>, HierarchicalTreeItem<T>)> ConstructLinks_r(HierarchicalTreeItem<T> root)
        {
            HashSet<(HierarchicalTreeItem<T>, HierarchicalTreeItem<T>)> result = new(
                root.children.Length,
                new EqualityComparerHierarchicalItemLink<T>()
            );
            foreach (HierarchicalTreeItem<T> child in root.children)
            {
                result.Add((child, root));
                result.UnionWith(ConstructLinks_r(child));
            }
            return result;
        }
    }
}
