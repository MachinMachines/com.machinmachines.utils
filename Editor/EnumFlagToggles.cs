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

using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace MachinMachines.EditorUI
{
    /// <summary>
    /// A pure UI helper to draw checkboxes for enums, either:
    /// - the enum is a flag and multiple boxes can be selected at once
    /// - it is a regular enum and selecting one will unselect any other
    /// </summary>
    public static class EnumFlagToggles<T> where T : System.Enum
    {
        static readonly int[] kValues = (int[])System.Enum.GetValues(typeof(T));
        static readonly string[] kNames = System.Enum.GetNames(typeof(T));
        // Determine if this is a "flag enum" with multiple simultaneous values allowed
        static readonly bool kIsFlagEnum = System.Attribute.IsDefined(typeof(T).GetTypeInfo(), typeof(System.FlagsAttribute));

        public static void Draw(ref T _enum)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                GUILayout.BeginVertical("", GUIStyle.none);
                EditorGUILayout.LabelField(_enum.GetType().Name, EditorStyles.miniBoldLabel);

                EditorGUI.BeginChangeCheck();
                // TODO @gama use an actual bitfield
                bool[] setBits = new bool[kValues.Length];
                for (int i = 0; i < kValues.Length; ++i)
                {
                    T enumValue = (T)System.Enum.ToObject(typeof(T), kValues[i]);
                    if (kIsFlagEnum)
                    {
                        setBits[i] = EditorGUILayout.ToggleLeft(kNames[i], _enum.HasFlag(enumValue));
                    }
                    else
                    {
                        setBits[i] = EditorGUILayout.ToggleLeft(kNames[i], _enum.Equals(enumValue));
                    }
                }
                int enumIntValue = System.Convert.ToInt32(_enum);
                // All/None buttons
                if (kIsFlagEnum)
                {
                    GUILayout.BeginHorizontal("", GUIStyle.none);
                    if (GUILayout.Button("None", GUILayout.MaxWidth(64)))
                    {
                        System.Array.Fill(setBits, false);
                    }
                    if (GUILayout.Button("All", GUILayout.MaxWidth(64)))
                    {
                        System.Array.Fill(setBits, true);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    // Here we convert the bitfield into actual values,
                    // As "regular" enums have values potentially not squashable
                    if (kIsFlagEnum)
                    {
                        for (int i = 0; i < setBits.Length; ++i)
                        {
                            if (setBits[i])
                            {
                                enumIntValue |= kValues[i];
                            }
                            else
                            {
                                enumIntValue &= ~kValues[i];
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < setBits.Length; ++i)
                        {
                            if (setBits[i])
                            {
                                if (kValues[i] != enumIntValue)
                                {
                                    enumIntValue = kValues[i];
                                    break;
                                }
                            }
                        }
                    }
                    _enum = (T)System.Enum.ToObject(typeof(T), enumIntValue);
                }
            }
        }
    }
}
