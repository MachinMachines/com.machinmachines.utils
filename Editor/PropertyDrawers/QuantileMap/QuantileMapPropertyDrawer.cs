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

namespace MachinMachines
{
    namespace Quantile
    {
        public abstract class QuantileMapPropertyDrawer : PropertyDrawer
        {
            protected static GUIStyle BucketHeaderStyle
            {
                get
                {
                    if (s_BucketHeaderStyle == null)
                    {
                        s_BucketHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
                        {
                            clipping = TextClipping.Overflow
                        };
                    }
                    return s_BucketHeaderStyle;
                }
            }

            private static GUIStyle s_BucketHeaderStyle;
            private bool _foldout = false;
            private Vector2 _scrollPosition = new Vector2();

            public override sealed void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                _foldout = EditorGUI.Foldout(position, _foldout, property.displayName);
                if (_foldout)
                {
                    EditorGUI.indentLevel += 1;
                    OnDrawGeneralMapGUI(property);
                    SerializedProperty sizeMapProps = property.FindPropertyRelative("Buckets");
                    int bucketsCount = sizeMapProps.arraySize;
                    using (EditorGUILayout.ScrollViewScope scope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
                    {
                        for (int bucketIdx = 0; bucketIdx < bucketsCount; ++bucketIdx)
                        {
                            SerializedProperty bucketItemProp = sizeMapProps.GetArrayElementAtIndex(bucketIdx);
                            OnDrawBucketGUI(bucketItemProp);
                        }
                        _scrollPosition = scope.scrollPosition;
                    }
                    EditorGUI.indentLevel -= 1;
                }

                EditorGUI.EndProperty();
            }

            protected abstract void OnDrawGeneralMapGUI(SerializedProperty mapProperty);
            protected abstract void OnDrawBucketGUI(SerializedProperty bucketProperty);
        }
    }
}
