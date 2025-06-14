using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace BlackCatLocalisation
{
    public static class TextManager
    {
        private const string PATH = "Assets/The Black Cat/Language Localiser/Resources/Language Localisation System";

        public static string CurrentLanguage { get; private set; }

        private static LocalisationSettings setting;
        private static Dictionary<string, string> allTexts = new Dictionary<string, string>();

        public static void SetLanguage(string languageId)
        {
            var txt = Resources.Load($"lang/{languageId}");

            if (txt != null)
            {
                allTexts.Clear();
                if (txt is TextAsset textAsset)
                {
                    allTexts = ParseJson(textAsset.text);
                }
                else if (txt is TextData textData)
                {
                    allTexts = ParseSO(textData);
                }
                else
                {
                    throw new InvalidCastException($"Unvalid localisation file type: {txt.GetType()}. Only support Json files or Scriptable Objects of TextData class.");
                }

                var tmps = UnityEngine.Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                var font = GetSettings().GetFont(languageId);

                foreach (var tmp in tmps)
                {
                    if (tmp != null && !tmp.TryGetComponent<DontChangeFont>(out _))
                    {
                        tmp.font = font;
                    }
                }

                CurrentLanguage = languageId;
            }
            else
            {
                throw new System.NullReferenceException($"File named \"{languageId}\" doesn't exist.");
            }
        }

        public static string GetText(string key)
        {
            return allTexts.TryGetValue(key, out var text) ? text : key;
        }

        public static LocalisationSettings GetSettings()
        {
            if (setting == null)
            {
                if (!Directory.Exists(PATH))
                {
                    Directory.CreateDirectory(PATH);
                }

                var newSetting = Resources.Load<LocalisationSettings>("Language Localisation System/Localisation Setting");
                if (newSetting == null)
                {
                    newSetting = ScriptableObject.CreateInstance<LocalisationSettings>();
                    AssetDatabase.CreateAsset(newSetting, PATH + "/Localisation Setting.asset");
                    AssetDatabase.SaveAssets();
                }
                setting = newSetting;
            }
            return setting;
        }

        private static Dictionary<string, string> ParseJson(string json)
        {
            if (json.StartsWith('{') && json.EndsWith('}'))
            {
                json = json.Substring(1, json.Length - 2);
                json = json.Trim();
            }
            else
            {
                throw new FormatException("Invalid JSON format: must start with '{' and end with '}'.");
            }

            Dictionary<string, string> texts = new Dictionary<string, string>();
            var validLines = Regex.Matches(json, "\"(.*?)\"\\s*:\\s*\"(.*?)\"");

            foreach (Match line in validLines)
            {
                if (line != null && line.Groups.Count == 3)
                {
                    var key = line.Groups[1].Value.Trim();
                    var value = line.Groups[2].Value.Trim();

                    if (!texts.ContainsKey(key))
                    {
                        texts.Add(key, value);
                    }
                    else
                    {
                        Debug.LogWarning($"Key {key} already exists and is paired with {texts[key]}.");
                    }
                }
            }

            return texts;
        }

        private static Dictionary<string, string> ParseSO(TextData data)
        {
            var entries = data.entries;
            Dictionary<string, string> texts = new Dictionary<string, string>();

            foreach (var entry in entries)
            {
                if (!texts.ContainsKey(entry.key))
                {
                    texts.Add(entry.key, entry.text);
                }
                else
                {
                    Debug.LogWarning($"Key {entry.key} already exists and is paired with {texts[entry.key]}.");
                }
            }

            return texts;
        }

        public static List<string> GetAllEntries()
        {
            return allTexts.Keys.ToList();
        }

        public static List<KeyValuePair<string, string>> GetAllTexts()
        {
            return allTexts.ToList<KeyValuePair<string, string>>();
        }

        public static List<string> GetMissingKeys(string targetLanguage, string compareLanguage)
        {
            var txt1 = Resources.Load($"lang/{targetLanguage}");
            var txt2 = Resources.Load($"lang/{compareLanguage}");

            var keys1 = (txt1 is TextAsset ? ParseJson((txt1 as TextAsset).text) : ParseSO((TextData)txt1)).Keys.ToArray();
            var keys2 = (txt2 is TextAsset ? ParseJson((txt2 as TextAsset).text) : ParseSO((TextData)txt2)).Keys.ToArray();

            List<string> missingKeys = new List<string>();
            Array.Sort(keys1);

            foreach (var key in keys2)
            {
                if (Array.BinarySearch(keys1, key) < 0)
                {
                    missingKeys.Add(key);
                }
            }

            return missingKeys;
        }
    }
}
