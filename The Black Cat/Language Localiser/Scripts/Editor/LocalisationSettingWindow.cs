using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BlackCatLocalisation
{
    public class LocalisationSettingWindow : EditorWindow
    {
        private int fontPage = 0;
        private int entryPage = 0;
        private bool showEntries = false;
        private List<KeyValuePair<string, string>> entries;
        private string checkLanguage;
        private List<string> missingEntries;
        private LocalisationSettings util;
        private SerializedObject so;
        private SerializedProperty property;

        [MenuItem("Tools/Localisation Setting")]
        public static void ShowWindow()
        {
            GetWindow<LocalisationSettingWindow>("Localisation Setting");
        }

        private void OnEnable()
        {
            util = TextManager.GetSettings();
            so = new SerializedObject(util);
            property = so.FindProperty("language2font");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Fonts for Languages", EditorStyles.boldLabel);
            GUILayout.Label("", GUILayout.Width(20));

            fontPage = EditorGUILayout.IntField(fontPage + 1, GUILayout.Width(30));
            fontPage = Mathf.Clamp(fontPage - 1, 0, Mathf.CeilToInt((float)property.arraySize / 7) - 1);

            string pages = $" / {Mathf.CeilToInt((float)property.arraySize / 7)}";
            GUILayout.Label(pages, GUILayout.Width(30 + CalculateLabelWidth(pages + 5)));

            GUI.enabled = fontPage > 0;
            if (GUILayout.Button("¡ö", GUILayout.Width(40)))
            {
                fontPage--;
            }

            GUI.enabled = (fontPage + 1) * 7 < property.arraySize;
            if (GUILayout.Button("¡÷", GUILayout.Width(40)))
            {
                fontPage++;
            }
            GUI.enabled = true;

            if (GUILayout.Button("+", GUILayout.Width(150)))
            {
                util.language2font.Add(new LocalisationSettings.LanguageFontData());
                so.Update();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            Rect rect = new Rect(2, EditorGUIUtility.singleLineHeight * 2 - 4, EditorGUIUtility.currentViewWidth - 4, EditorGUIUtility.singleLineHeight - 2);
            EditorGUI.DrawRect(rect, new Color32(36, 36, 36, 255));
            GUILayout.Label("Language ID", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("Font", EditorStyles.boldLabel);

            EditorGUILayout.EndHorizontal();

            rect.y += rect.height;
            rect.height = EditorGUIUtility.singleLineHeight + 3;
            int min = 7 * fontPage;
            int max = min + 6;
            for (int i = min; i <= max; i++)
            {
                if (i >= property.arraySize) break;
                SerializedProperty element = property.GetArrayElementAtIndex(i);

                var prop_lang = element.FindPropertyRelative("languageId");
                var prop_font = element.FindPropertyRelative("font");

                EditorGUILayout.BeginHorizontal();

                EditorGUI.DrawRect(rect, new Color32(48, 48, 48, 255));
                rect.y += rect.height;

                EditorGUILayout.PropertyField(prop_lang, GUIContent.none, GUILayout.Width(150));
                EditorGUILayout.PropertyField(prop_font, GUIContent.none);

                if (GUILayout.Button("-", GUILayout.Width(40)))
                {
                    property.DeleteArrayElementAtIndex(i);
                    so.ApplyModifiedProperties();
                    i--;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(20);

            GUILayout.Label("Default Language", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            var prop_loadDefault = so.FindProperty("loadDefaultOnSceneLoad");
            float width = CalculateLabelWidth("Load Default Language On Scene Load");

            GUILayout.Label("Load Default Language On Scene Load", GUILayout.Width(width + 5));
            prop_loadDefault.boolValue = EditorGUILayout.Toggle(prop_loadDefault.boolValue);

            EditorGUILayout.EndHorizontal();

            if (prop_loadDefault.boolValue)
            {
                EditorGUILayout.PropertyField(so.FindProperty("defaultLanguageId"), new GUIContent("Default Language ID"));
            }

            EditorGUILayout.Space(20);

            GUILayout.Label("Current Language Entries", EditorStyles.boldLabel);

            GUI.enabled = Application.isPlaying;
            if (!GUI.enabled && showEntries)
            {
                showEntries = false;
            }

            if (GUILayout.Button(showEntries ? "Hide All Current Entries" : "Show All Current Entries", GUILayout.Width(150)))
            {
                showEntries = !showEntries;
                entries = TextManager.GetAllTexts();
            }

            if (showEntries)
            {
                entries = entries ?? TextManager.GetAllTexts();

                GUILayout.Label("Current Language ID: " + TextManager.CurrentLanguage);

                EditorGUILayout.BeginHorizontal();

                string count = (entries != null ? entries.Count.ToString() : "0") + (entries?.Count == 1 ? " Entry" : " Entries");
                GUILayout.Label(count, GUILayout.Width(CalculateLabelWidth(count + 5)));
                GUILayout.FlexibleSpace();

                entryPage = EditorGUILayout.IntField(entryPage + 1, GUILayout.Width(30));
                entryPage = Mathf.Clamp(entryPage - 1, 0, Mathf.CeilToInt((float)entries.Count / 8) - 1);

                pages = $" / {Mathf.CeilToInt((float)entries.Count / 8)}";
                GUILayout.Label(pages, GUILayout.Width(CalculateLabelWidth(pages + 5)));

                GUI.enabled = entryPage > 0;
                if (GUILayout.Button("¡ö", GUILayout.Width(40)))
                {
                    entryPage--;
                }

                GUI.enabled = (entryPage + 1) * 8 < entries.Count;
                if (GUILayout.Button("¡÷", GUILayout.Width(40)))
                {
                    entryPage++;
                }
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                rect = new Rect(2, 228 + Mathf.Min(property.arraySize, 7) * (EditorGUIUtility.singleLineHeight + 3), EditorGUIUtility.currentViewWidth - 4, EditorGUIUtility.singleLineHeight - 2);
                EditorGUI.DrawRect(rect, new Color32(36, 36, 36, 255));
                GUILayout.Label("Key", EditorStyles.boldLabel, GUILayout.Width(150));
                GUILayout.Label("Text", EditorStyles.boldLabel);

                EditorGUILayout.EndHorizontal();

                rect.y += rect.height;
                rect.height = EditorGUIUtility.singleLineHeight + 5;

                min = 8 * entryPage;
                max = min + 7;
                for (int i = min; i <= max; i++)
                {
                    if (i >= entries.Count) break;

                    GUILayout.BeginHorizontal();

                    EditorGUI.DrawRect(rect, new Color32(48, 48, 48, 255));
                    rect.y += rect.height;

                    GUILayout.TextField(entries[i].Key, GUILayout.Width(150), GUILayout.Height(EditorGUIUtility.singleLineHeight + 2));
                    GUILayout.TextField(entries[i].Value, GUILayout.Height(EditorGUIUtility.singleLineHeight + 2));

                    GUILayout.EndHorizontal();

                    GUILayout.Space(1);
                }
            }

            GUI.enabled = true;

            EditorGUILayout.Space(20);

            GUILayout.Label("Check Missing Keys", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Check Language ID", GUILayout.Width(150));
            checkLanguage = EditorGUILayout.TextField(checkLanguage, GUILayout.Width(150));

            if (GUILayout.Button("Check", GUILayout.Width(70)))
            {
                if (string.IsNullOrEmpty(checkLanguage))
                {
                    Debug.LogWarning("Please enter the language ID you want to check.");
                }
                else
                {
                    missingEntries = TextManager.GetMissingKeys(checkLanguage, util.defaultLanguageId);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (missingEntries != null && missingEntries.Count > 0)
            {
                for (int i = 0; i < missingEntries.Count; i++)
                {
                    GUILayout.Label(missingEntries[i], EditorStyles.wordWrappedLabel);
                }
            }
            else
            {
                GUILayout.Label("None");
            }

            so.ApplyModifiedProperties();
        }

        private float CalculateLabelWidth(string label)
        {
            return EditorStyles.label.CalcSize(new GUIContent(label)).x;
        }
    }
}