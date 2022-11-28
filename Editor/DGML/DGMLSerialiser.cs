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
using System.Xml.Serialization;

using UnityEngine;

namespace MachinMachines
{
    namespace DGML
    {
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

            public DirectedGraph GenerateDirectedGraph()
            {
                DirectedGraph result = new DirectedGraph { Title = $"{Name}_graph" };
                // Using only links (from child to parent)
                IEnumerable<(HierarchicalTreeItem<T>, HierarchicalTreeItem<T>)> links = ConstructLinks_r(this);
                // This way we can easily get all items (all distinct children and parents)
                HashSet<HierarchicalTreeItem<T>> allItems = links.Select(item => item.Item1)
                                                                 .Union(links.Select(item => item.Item2))
                                                                 .ToHashSet();
                // Then hand it over to DGML containers
                result.Nodes = allItems
                    .Select(item => new DGMLNode
                    {
                        Id = item.Name,
                        Category = !string.IsNullOrEmpty(item.Category) ? item.Category : null
                    })
                    .ToHashSet(new DGMLNodeComparer());
                result.Links = links
                    .Select(item => new DGMLLink
                    {
                        Source = item.Item1.Name,
                        Target = item.Item2.Name
                    })
                    .ToHashSet();
                // If there is a lookup table setup, map every category to a color
                if (CategoryToColorMapping != null)
                {
                    result.Categories = result.Nodes.GroupBy(item => item.Category)
                                                  .Select(item => new DGMLCategory
                                                  {
                                                      Id = item.Key,
                                                      Background = $"#ff{ColorUtility.ToHtmlStringRGB(CategoryToColorMapping[item.Key])}"
                                                  })
                                                  .ToHashSet();
                }
                return result;
            }

            public void DumpAsDGMLToPath(string graphPath)
            {
                DirectedGraph graph = GenerateDirectedGraph();
                graphPath = Path.ChangeExtension(graphPath, DGMLSerialiser.Extension);
                DGMLSerialiser.SerialiseToPath(graph, graphPath);
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
            private IEnumerable<(HierarchicalTreeItem<T>, HierarchicalTreeItem<T>)> ConstructLinks_r(HierarchicalTreeItem<T> root)
            {
                List<(HierarchicalTreeItem<T>, HierarchicalTreeItem<T>)> result = new(root.children.Length);
                foreach (HierarchicalTreeItem<T> child in root.children)
                {
                    result.Add((child, root));
                    result.AddRange(ConstructLinks_r(child));
                }
                return result;
            }
        }

        // A simple helper so we can consider two DGML nodes are equal
        // as long as they have "equivalent" names (equal whatever the case)
        public class DGMLNodeComparer : IEqualityComparer<DGMLNode>
        {
            public bool Equals(DGMLNode lhs, DGMLNode rhs)
            {
                // Check whether the objects are the same object.
                if (lhs.Equals(rhs))
                {
                    return true;
                }
                return lhs.Id.ToLower().Equals(rhs.Id.ToLower());
            }

            public int GetHashCode(DGMLNode obj)
            {
                return obj.Id.ToLower().GetHashCode();
            }
        }

        public class DGMLNode
        {
            [XmlAttribute]
            public string Id;

            [XmlAttribute]
            public string Category;

            [XmlAttribute]
            public string Group = null;
            [XmlIgnore]
            public bool GroupSpecified { get { return this.Group != null; } }
        }

        public class DGMLLink
        {
            [XmlAttribute]
            public string Source;
            [XmlAttribute]
            public string Target;

            [XmlAttribute]
            public string Category = null;
            [XmlIgnore]
            public bool CategorySpecified { get { return this.Category != null; } }
        }

        public class DGMLCategory
        {
            [XmlAttribute]
            public string Id;
            [XmlAttribute]
            public string Background;
        }

        [XmlRoot(Namespace = @"http://schemas.microsoft.com/vs/2009/dgml")]
        public class DirectedGraph
        {
            [XmlAttribute]
            public string Title;

            [XmlArray("Nodes")]
            [XmlArrayItem("Node")]
            public HashSet<DGMLNode> Nodes;

            [XmlArray("Links")]
            [XmlArrayItem("Link")]
            public HashSet<DGMLLink> Links;

            [XmlArray("Categories")]
            [XmlArrayItem("Category")]
            public HashSet<DGMLCategory> Categories;

            public int Properties { get; set; }
        }

        public class DGMLSerialiser
        {
            public const string Extension = "dgml";

            public static void SerialiseToPath(DirectedGraph graph, string filepath)
            {
                using(StreamWriter stream = new StreamWriter(filepath))
                {
                    XmlSerializer xmls = new XmlSerializer(typeof(DirectedGraph));
                    xmls.Serialize(stream, graph);
                }
            }
        }
    }
}
