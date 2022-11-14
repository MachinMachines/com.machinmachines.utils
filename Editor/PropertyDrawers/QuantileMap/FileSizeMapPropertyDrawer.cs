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

using MachinMachines.Utils;

using UnityEditor;

namespace MachinMachines
{
    namespace Quantile
    {
        public class FileSizeMapPropertyDrawer : QuantileMapPropertyDrawer
        {
            private const int kMaxItemsDisplayCount = 16;

            protected override void OnDrawGeneralMapGUI(SerializedProperty mapProperty)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.PropertyField(mapProperty.FindPropertyRelative("TotalSizeBytes"));
                    EditorGUILayout.PropertyField(mapProperty.FindPropertyRelative("TotalItemsCount"));
                }
            }

            protected override void OnDrawBucketGUI(SerializedProperty bucketProperty)
            {
                SerializedProperty serializedPropertyName = bucketProperty.FindPropertyRelative("Name");
                SerializedProperty serializedPropertySize = bucketProperty.FindPropertyRelative("Size");
                SerializedProperty serializedPropertyFiles = bucketProperty.FindPropertyRelative("Items");
                string sizeDisplay = MemorySizeUnit.DisplayByteSize(serializedPropertySize.longValue, 1);
                string header = $"{serializedPropertyName.stringValue}px - {serializedPropertyFiles.arraySize} item(s) - {sizeDisplay}";
                if (EditorGUILayout.Foldout(serializedPropertyFiles.arraySize > 0, header, BucketHeaderStyle))
                {
                    //EditorGUILayout.BeginVertical();
                    //EditorGUI.indentLevel += 1;
                    for (int fileIdx = 0; fileIdx < Math.Min(serializedPropertyFiles.arraySize, kMaxItemsDisplayCount); ++fileIdx)
                    {
                        EditorGUILayout.LabelField(serializedPropertyFiles.GetArrayElementAtIndex(fileIdx).stringValue);
                    }
                    //EditorGUI.indentLevel -= 1;
                    //EditorGUILayout.EndVertical();
                    if (serializedPropertyFiles.arraySize > kMaxItemsDisplayCount)
                    {
                        EditorGUILayout.LabelField($"Items count >{kMaxItemsDisplayCount}, skipped some", EditorStyles.boldLabel);
                    }
                }
            }
        }
    }
}
