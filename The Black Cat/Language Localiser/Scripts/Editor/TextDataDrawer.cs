using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BlackCatLocalisation
{
    [CustomEditor(typeof(TextData)), CanEditMultipleObjects]
    public class TextDataDrawer : Editor
    {
        private int page = 0;       
        private List<int> pageElementIndex = new List<int>();
        private Dictionary<string, int> keyOccurrence = new Dictionary<string, int>();
        private Dictionary<string, int> occurredIndex = new Dictionary<string, int>();
        private ReorderableList reorderableList;
        private SerializedObject so;
        private SerializedProperty list;

        public override void OnInspectorGUI()
        {
            if (reorderableList == null)
            {
                Initialise();                
            }

            EditorGUI.BeginChangeCheck();

            reorderableList.DoLayoutList();
            
            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedProperties();
                UpdatePage();
            }
        }

        private void Initialise()
        {
            so = new SerializedObject(target);
            list = so.FindProperty("entries");
            reorderableList = new ReorderableList(pageElementIndex, typeof(int), true, true, false, false);

            UpdatePage();

            reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                Rect r_header = new Rect(rect.x, rect.y, CalculateLabelWidth("Entries") + 5, EditorGUIUtility.singleLineHeight);
                Rect r_entries = new Rect(r_header.x + r_header.width + 5, rect.y, rect.width - r_header.width, EditorGUIUtility.singleLineHeight);

                EditorGUI.LabelField(rect, "Entries", EditorStyles.boldLabel);
                EditorGUI.LabelField(r_entries, list.arraySize + (list.arraySize == 1 ? " entry" : " entries"), EditorStyles.miniLabel);

                Rect addButton = new Rect(rect.x + rect.width - 150, rect.y, 150, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(addButton, "+"))
                {
                    list.InsertArrayElementAtIndex(list.arraySize);
                    UpdatePage();
                }

                Rect nextPage = new Rect(addButton.x - 43, addButton.y, 40, EditorGUIUtility.singleLineHeight);
                Rect prevPage = new Rect(nextPage.x - 43, addButton.y, 40, EditorGUIUtility.singleLineHeight);

                GUI.enabled = page > 0;
                if (GUI.Button(prevPage, "¡ö"))
                {
                    page--;
                    UpdatePage();
                }                

                GUI.enabled = (page + 1) * 10 < list.arraySize;
                if (GUI.Button(nextPage, "¡÷"))
                {
                    page++;
                    UpdatePage();
                }
                GUI.enabled = true;

                string pages = $" / {Mathf.CeilToInt((float)list.arraySize / 10)}";                
                Rect label_page = new Rect(prevPage.x - CalculateLabelWidth(pages + 5), addButton.y, CalculateLabelWidth(pages + 5), EditorGUIUtility.singleLineHeight);
                Rect currentPage = new Rect(label_page.x - label_page.width - 5, addButton.y, 30, EditorGUIUtility.singleLineHeight);

                page = EditorGUI.IntField(currentPage, page + 1);
                page = Mathf.Clamp(page - 1, 0, Mathf.CeilToInt((float)list.arraySize / 10) - 1);
                EditorGUI.LabelField(label_page, pages);
            };        

            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index >= pageElementIndex.Count)
                {
                    return;
                }

                if (index == 0)
                {
                    keyOccurrence.Clear();
                    occurredIndex.Clear();
                    for (int i = 0; i < list.arraySize; i++)
                    {
                        string key = list.GetArrayElementAtIndex(i).FindPropertyRelative("key").stringValue;
                        if (!keyOccurrence.ContainsKey(key) && !string.IsNullOrEmpty(key))
                        {
                            keyOccurrence.Add(key, 1);
                            occurredIndex.Add(key, i);
                        }
                        else
                        {
                            keyOccurrence[key]++;
                        }
                    }
                }

                SerializedProperty element = list.GetArrayElementAtIndex(pageElementIndex[index]);

                var prop_key = element.FindPropertyRelative("key");
                var prop_text = element.FindPropertyRelative("text");

                float width = CalculateLabelWidth("Text ") + 8;
                Rect label_key = new Rect(rect.x, rect.y + 3, width, EditorGUIUtility.singleLineHeight);
                Rect label_text = new Rect(rect.x, label_key.y + 2 + label_key.height, width, EditorGUIUtility.singleLineHeight);

                EditorGUI.LabelField(label_key, "Key ", EditorStyles.boldLabel);
                EditorGUI.LabelField(label_text, "Text ", EditorStyles.boldLabel);

                Rect r_key = new Rect(rect.x + width, label_key.y, 250, EditorGUIUtility.singleLineHeight);
                Rect r_text = new Rect(rect.x + width, label_text.y, rect.width - width, EditorGUIUtility.singleLineHeight * 4 - 8);

                Color originalColor = GUI.color;
                if (keyOccurrence[prop_key.stringValue] > 1 && !string.IsNullOrEmpty(prop_key.stringValue))
                {
                    GUI.color = occurredIndex[prop_key.stringValue] == pageElementIndex[index] ? Color.yellow : Color.red;
                }

                prop_key.stringValue = EditorGUI.TextField(r_key, prop_key.stringValue);
                prop_text.stringValue = EditorGUI.TextArea(r_text, prop_text.stringValue, EditorStyles.textArea);

                GUI.color = originalColor;

                Rect delButton = new Rect(rect.x + rect.width - 80, label_key.y, 80, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(delButton, "-"))
                {                    
                    list.DeleteArrayElementAtIndex(pageElementIndex[index]);                    
                    pageElementIndex.RemoveAt(index);                    
                    UpdatePage();
                }              
                
                if (index >= pageElementIndex.Count - 1)
                {
                    keyOccurrence.Clear();
                }
            };

            reorderableList.elementHeightCallback = (int index) =>
            {
                return EditorGUIUtility.singleLineHeight * 5 + 2;
            };            

            reorderableList.onChangedCallback = (ReorderableList list) =>
            {
                UpdatePage();
            };
        }

        private void UpdatePage()
        {
            int maxPage = Mathf.CeilToInt((float)list.arraySize / 10) - 1;
            page = Mathf.Clamp(page, 0, Mathf.Max(maxPage, 0));

            pageElementIndex.Clear();
            int min = page * 10;
            int max = min + 9;
            for (int i = min; i <= max; i++)
            {
                if (i >= list.arraySize)
                {
                    break;
                }
                pageElementIndex.Add(i);
            }
        }

        private float CalculateLabelWidth(string label)
        {
            return EditorStyles.label.CalcSize(new GUIContent(label)).x;
        }
    }
}