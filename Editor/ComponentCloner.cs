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

using UnityEditor;

using UnityEngine;
using UnityEngine.Profiling;

namespace MachinMachines
{
    namespace Utils
    {
        public class HierarchicalTreeItem
        {
            public string DisplayName
            {
                get
                {
                    return GameObject != null ? GameObject.name : Data;
                }
            }
            public GameObject GameObject;
            public GameObject Parent;
            public string Data;
            public bool Flag;
            public int Depth;
            public HashSet<HierarchicalTreeItem> Children;
        }

        public class HierarchicalTreeItemComparer : IEqualityComparer<HierarchicalTreeItem>
        {
            public bool Equals(HierarchicalTreeItem x, HierarchicalTreeItem y)
            {
                return x.DisplayName.Equals(y.DisplayName);
            }

            public int GetHashCode(HierarchicalTreeItem obj)
            {
                return obj.DisplayName.GetHashCode();
            }
        }

        public class HierarchicalTree
        {
            public HierarchicalTreeItem Root;

            public IEnumerable<HierarchicalTreeItem> Content
            {
                get
                {
                    return Browse_r(Root);
                }
            }

            public void Clear(GameObject root)
            {
                Root = new HierarchicalTreeItem { GameObject = root, Parent = null, Depth = 0, Children = new HashSet<HierarchicalTreeItem>(new HierarchicalTreeItemComparer()) };
            }

            public void Add(IEnumerable<GameObject> parentHierarchy)
            {
                GameObject current = parentHierarchy.FirstOrDefault();
                foreach (GameObject child in parentHierarchy.Skip(1).SkipLast(1))
                {
                    Add(child, current, false);
                    current = child;
                }
                Add(parentHierarchy.Last(), current, true);
            }

            public void Add(string data, GameObject parent)
            {
                HierarchicalTreeItem found = Root;
                foreach (HierarchicalTreeItem item in Content)
                {
                    if (item.GameObject == parent)
                    {
                        found = item;
                        break;
                    }
                }
                found.Children.Add(new HierarchicalTreeItem { Data = data, Parent = parent, Flag = true, Depth = found.Depth + 1, Children = new HashSet<HierarchicalTreeItem>(new HierarchicalTreeItemComparer()) });
            }

            private void Add(GameObject go, GameObject parent, bool flag)
            {
                HierarchicalTreeItem found = Root;
                foreach (HierarchicalTreeItem item in Content)
                {
                    if (item.GameObject == parent)
                    {
                        found = item;
                        break;
                    }
                }
                found.Children.Add(new HierarchicalTreeItem { GameObject = go, Parent = parent, Flag = flag, Depth = found.Depth + 1, Children = new HashSet<HierarchicalTreeItem>(new HierarchicalTreeItemComparer()) });
            }

            private IEnumerable<HierarchicalTreeItem> Browse_r(HierarchicalTreeItem start)
            {
                if (start == null)
                {
                    yield break;
                }
                yield return start;
                foreach (HierarchicalTreeItem child in BrowseChildren_r(start))
                {
                    yield return child;
                }
            }

            private IEnumerable<HierarchicalTreeItem> BrowseChildren_r(HierarchicalTreeItem start)
            {
                foreach (HierarchicalTreeItem child in start.Children)
                {
                    yield return child;
                    foreach (HierarchicalTreeItem grandChild in BrowseChildren_r(child))
                    {
                        yield return grandChild;
                    }
                }
            }
        }

        // Simple utility to copy components or from one game object or prefab to another
        public class ComponentClonerWindow : EditorWindow
        {
            private const string k_CloneButton = "Clone";

            private static readonly IEnumerable<Type> s_typeCache = TypeCache.GetTypesDerivedFrom<Component>().OrderBy(item => item.Name);

            private GameObject _lhsGO;
            private GameObject _rhsGO;
            private Type _cpntType;
            private string _cpntTypeStr = "";
            private Vector2 _typeScrollPos;
            private HashSet<Type> _checkedCpntTypes = new HashSet<Type>();
            private IEnumerable<Type> _filteredTypes = new Type[0];
            private HierarchicalTree _lhsHierarchy = new HierarchicalTree();
            private HierarchicalTree _rhsHierarchy = new HierarchicalTree();

            [MenuItem("MachinMachines/Utils/ComponentCloner")]
            static void Init()
            {
                GetWindow(typeof(ComponentClonerWindow));
            }

            public void OnGUI()
            {
                Profiler.BeginSample("ComponentClonerWindow - OnGUI");

                EditorGUI.BeginChangeCheck();

                using (new GUILayout.HorizontalScope())
                {
                    _lhsGO = EditorGUILayout.ObjectField(_lhsGO, typeof(UnityEngine.GameObject), false) as GameObject;
                    _rhsGO = EditorGUILayout.ObjectField(_rhsGO, typeof(UnityEngine.GameObject), false) as GameObject;
                }
                _cpntTypeStr = EditorGUILayout.TextField(_cpntTypeStr);

                if (EditorGUI.EndChangeCheck())
                {
                    _filteredTypes = s_typeCache;
                    // First filter out types not present on the LHS object
                    if (_lhsGO != null)
                    {
                        HashSet<string> lhsCpntTypes = _lhsGO.GetComponentsInChildren<Component>().Select(item => item.GetType().Name).ToHashSet();
                        _filteredTypes = s_typeCache.Where(item => lhsCpntTypes.Contains(item.Name));
                    }
                }

                EditorGUI.BeginChangeCheck();

                foreach (Type filteredType in _filteredTypes)
                {
                    bool typeIsSelected = _checkedCpntTypes.Contains(filteredType);
                    if (EditorGUILayout.ToggleLeft(filteredType.Name, typeIsSelected, GUILayout.MaxHeight(16.0f)))
                    {
                        _checkedCpntTypes.Add(filteredType);
                    }
                    else
                    {
                        _checkedCpntTypes.Remove(filteredType);
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    _lhsHierarchy.Clear(_lhsGO);
                    // List all children GO holding the selected types, in the hierarchical order
                    foreach (Type cpntType in _checkedCpntTypes)
                    {
                        foreach (GameObject item in GameObjectHierarchy.BrowseChildHierarchy(_lhsGO))
                        {
                            Component cpnt = item.GetComponent(cpntType);
                            if (cpnt != null)
                            {
                                IEnumerable<GameObject> parentHierarchy = GameObjectHierarchy.BrowseParentHierarchy(item, null).Reverse();
                                _lhsHierarchy.Add(parentHierarchy);
                            }
                        }
                    }
                    if (_rhsGO != null)
                    {
                        _rhsHierarchy.Clear(_rhsGO);
                        foreach (HierarchicalTreeItem lhsItem in _lhsHierarchy.Content)
                        {
                            if (lhsItem.Flag)
                            {
                                foreach (GameObject rhsItem in GameObjectHierarchy.BrowseChildHierarchy(_rhsGO))
                                {
                                    if (lhsItem.Parent.name == rhsItem.name)
                                    {
                                        IEnumerable<GameObject> parentHierarchy = GameObjectHierarchy.BrowseParentHierarchy(rhsItem, null).Reverse();
                                        _rhsHierarchy.Add(parentHierarchy);
                                        // Add missing element!
                                        _rhsHierarchy.Add(lhsItem.DisplayName, parentHierarchy.Last());
                                    }
                                }
                            }
                        }
                    }
                }
                using (new GUILayout.HorizontalScope())
                {
                    using (GUILayout.ScrollViewScope scrollScope = new GUILayout.ScrollViewScope(_typeScrollPos))
                    {
                        EditorGUI.indentLevel += 1;
                        using (new GUILayout.VerticalScope())
                        {
                            using (new EditorGUI.DisabledScope(true))
                            {
                                foreach (HierarchicalTreeItem item in _lhsHierarchy.Content)
                                {
                                    EditorGUI.indentLevel += item.Depth;
                                    GUIStyle labelStyle = EditorStyles.label;
                                    if (item.Children.Count == 0)
                                    {
                                        labelStyle = EditorStyles.boldLabel;
                                    }
                                    EditorGUILayout.LabelField(item.GameObject.name, labelStyle);
                                    EditorGUI.indentLevel -= item.Depth;
                                }
                            }
                        }
                        EditorGUI.indentLevel -= 1;
                        _typeScrollPos = scrollScope.scrollPosition;
                    }

                    bool canBeCloned = _checkedCpntTypes.Count > 0 && _rhsGO != null;
                    using (new EditorGUI.DisabledScope(!canBeCloned))
                    {
                        using (new GUILayout.VerticalScope())
                        {
                            using (new EditorGUI.DisabledScope(true))
                            {
                                foreach (HierarchicalTreeItem item in _rhsHierarchy.Content)
                                {
                                    EditorGUI.indentLevel += item.Depth;
                                    GUIStyle labelStyle = EditorStyles.label;
                                    if (item.Flag)
                                    {
                                        labelStyle = EditorStyles.boldLabel;
                                    }
                                    EditorGUILayout.LabelField(item.DisplayName, labelStyle);
                                    EditorGUI.indentLevel -= item.Depth;
                                }
                            }
                        }
                    }
                }
                Profiler.EndSample();
            }
        }
    }
}
