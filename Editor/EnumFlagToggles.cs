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

using System.Linq;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace MachinMachines
{
    namespace EditorUI
    {
        // A pure UI helper to draw checkboxes for enums, either:
        // - the enum is a flag and multiple boxes can be selected at once
        // - it is a regular enum and selecting one will unselect any other
        public static class EnumFlagToggles<T> where T : System.Enum
        {
            static readonly int[] kValues = (int[])System.Enum.GetValues(typeof(T));
            static readonly string[] kNames = System.Enum.GetNames(typeof(T));

            public static void Draw(ref T _enum)
            {
                // Determine if this is a "flag enum" with multiple simultaneous values allowed
                bool isFlagEnum = System.Attribute.IsDefined(_enum.GetType().GetTypeInfo(), typeof(System.FlagsAttribute));
                if(!isFlagEnum)
                {
                    Debug.LogError($"Enum type {_enum.GetType().Name} not supported as it is not a flag.");
                    return;
                }

                using (new EditorGUI.IndentLevelScope())
                {
                    GUILayout.BeginVertical("", GUIStyle.none);
                    EditorGUILayout.LabelField(_enum.GetType().Name, EditorStyles.miniBoldLabel);

                    EditorGUI.BeginChangeCheck();
                    // Get the aggregated values
                    int enumIntValue = System.Convert.ToInt32(_enum);
                    for(int i = 0; i < kValues.Length; ++i)
                    {
                        T enumValue = (T)System.Enum.ToObject(typeof(T), kValues[i]);
                        bool gotClicked = EditorGUILayout.ToggleLeft(kNames[i], _enum.HasFlag(enumValue));

                        if (gotClicked)
                        {
                            enumIntValue |= kValues[i];
                        }
                        else
                        {
                            enumIntValue &= ~kValues[i];
                        }
                    }
                    // All/None buttons
                    if (isFlagEnum)
                    {
                        GUILayout.BeginHorizontal("", GUIStyle.none);
                        if (GUILayout.Button("None", GUILayout.MaxWidth(64)))
                        {
                            enumIntValue = 0;
                        }
                        if (GUILayout.Button("All", GUILayout.MaxWidth(64)))
                        {
                            enumIntValue = -1;
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();

                    if (EditorGUI.EndChangeCheck())
                    {
                        _enum = (T)System.Enum.ToObject(typeof(T), enumIntValue);
                    }
                }
            }
        }
    }
}
