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

using UnityEditor;

using UnityEngine;

namespace MachinMachines.Utils
{
    /// <summary>
    /// Custom drawer for memory size fields, displaying with the required unit
    /// </summary>
    [CustomPropertyDrawer(typeof(IsMemorySizeUnitAttribute))]
    public class IsMemorySizeUnitPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginDisabledGroup(true);
            using (new GUILayout.HorizontalScope())
            {
                string valueDisplay = MemorySizeUnit.DisplayByteSize(property.longValue, 1);
                Rect labelRect = new Rect(position.x, position.y, position.width / 4, position.height);
                EditorGUI.LabelField(labelRect, label);
                Rect valueRect = new Rect(position.x + position.width / 4, position.y, position.width / 4, position.height);
                EditorGUI.LabelField(valueRect, valueDisplay);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUI.EndProperty();
        }
    }
}
