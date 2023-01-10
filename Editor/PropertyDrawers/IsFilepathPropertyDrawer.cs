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

using UnityEditor;

using UnityEngine;

namespace MachinMachines.Utils
{
    [CustomPropertyDrawer(typeof(IsFilepathAttribute))]
    public class IsFilepathPropertyDrawer : PropertyDrawer
    {
        static readonly GUIStyle s_OpenPathButtonStyle = EditorStyles.miniButtonRight;
        static readonly GUIStyle s_PathFieldStyle = EditorStyles.miniTextField;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            IsFilepathAttribute filepathAttribute = attribute as IsFilepathAttribute;

            label = EditorGUI.BeginProperty(position, label, property);

            //GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(filepathAttribute.IsReadOnly);

            Rect labelRect = new Rect(position.x, position.y, position.width * 3.0f / 12.0f, position.height);
            EditorGUI.LabelField(labelRect, label.text);

            EditorGUI.BeginChangeCheck();
            Rect textRect = new Rect(position.x + position.width * 3.0f / 12.0f, position.y, position.width * 6.0f / 12.0f - 5.0f, position.height);
            string newPathValue = EditorGUI.TextField(textRect, property.stringValue, s_PathFieldStyle);
            if (textRect.Contains(Event.current.mousePosition))
            {
                switch (Event.current.type)
                {
                    case EventType.DragUpdated:
                        DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                        Event.current.Use();
                        break;
                    case EventType.DragPerform:
                        DragAndDrop.AcceptDrag();
                        if (string.IsNullOrEmpty(filepathAttribute.ExtensionMatch) || DragAndDrop.paths[0].EndsWith(filepathAttribute.ExtensionMatch))
                        {
                            newPathValue = DragAndDrop.paths[0];
                            GUI.changed = true;
                            Event.current.Use();
                        }
                        break;
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = newPathValue;
                property.serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.EndDisabledGroup();

            //GUILayout.FlexibleSpace();

            bool locationExists = filepathAttribute.Filetype == IsFilepathAttribute.FileType.File ? File.Exists(property.stringValue) : Directory.Exists(property.stringValue);
            using (new EditorGUI.DisabledGroupScope(!locationExists))
            {

                Rect button0Rect = new Rect(position.x + position.width * 9.0f / 12.0f, position.y, position.width * 1.5f / 12.0f - 5.0f, position.height);
                if (EditorGUI.DropdownButton(button0Rect, new GUIContent("Browser"), FocusType.Passive, s_OpenPathButtonStyle))
                {
                    OpenInFileBrowser.Open(property.stringValue);
                }
                Rect button1Rect = new Rect(position.x + position.width * 10.5f / 12.0f, position.y, position.width * 1.5f / 12.0f, position.height);
                if (EditorGUI.DropdownButton(button1Rect, new GUIContent("Project"), FocusType.Passive, s_OpenPathButtonStyle))
                {
                    // Only look for locations within the projet
                    if (property.stringValue.StartsWith("Assets"))
                    {
                        UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(property.stringValue);
                        if (obj != null)
                        {
                            EditorGUIUtility.PingObject(obj.GetInstanceID());
                        }
                    }
                }
            }

            //GUILayout.EndHorizontal();

            EditorGUI.EndProperty();
        }
    }
}
