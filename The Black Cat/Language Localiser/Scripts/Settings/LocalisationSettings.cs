using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BlackCatLocalisation
{
    public class LocalisationSettings : ScriptableObject
    {
        public List<LanguageFontData> language2font = new List<LanguageFontData>();
        public bool loadDefaultOnSceneLoad = true;
        public string defaultLanguageId = "";

        [Serializable]
        public class LanguageFontData
        {
            public string languageId;
            public TMP_FontAsset font;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public TMP_FontAsset GetFont(string languageId)
        {
            foreach (var data in language2font)
            {
                if (data.languageId.Equals(languageId, StringComparison.OrdinalIgnoreCase))
                {
                    return data.font;
                }
            }

            throw new Exception($"Font for language '{languageId}' is either not linked or doesn't exist.");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (loadDefaultOnSceneLoad && string.IsNullOrEmpty(TextManager.CurrentLanguage))
            {
                TextManager.SetLanguage(defaultLanguageId);
            }

            var tmps = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var font = GetFont(TextManager.CurrentLanguage);

            foreach (var tmp in tmps)
            {
                if (tmp != null && !tmp.TryGetComponent<DontChangeFont>(out _))
                {
                    tmp.font = font;
                }
            }
        }
    }
}