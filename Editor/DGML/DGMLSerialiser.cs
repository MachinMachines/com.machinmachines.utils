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
using System.Xml.Serialization;

namespace MachinMachines
{
    namespace DGML
    {
        public class DGMLNode
        {
            [XmlAttribute]
            public string Id;
            [XmlAttribute]
        }

        public class DGMLLink
        {
            [XmlAttribute]
            public string Source;
            [XmlAttribute]
            public string Target;
        }

        [XmlRoot(Namespace = @"http://schemas.microsoft.com/vs/2009/dgml")]
        public class DirectedGraph
        {
            [XmlAttribute]
            public string Title;

            [XmlArray("Nodes")]
            [XmlArrayItem("Node")]
            public DGMLNode[] Nodes;

            [XmlArray("Links")]
            [XmlArrayItem("Link")]
            public DGMLLink[] Links;

            public int Categories { get; set; }

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
