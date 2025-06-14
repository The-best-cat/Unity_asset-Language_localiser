using System;
using UnityEngine;

namespace BlackCatLocalisation
{
    [CreateAssetMenu(fileName = "Text Localisation Data", menuName = "Language Localiser/Text Localisation Data", order = 1)]
    public class TextData : ScriptableObject
    {
        public TextEntry[] entries;

        [Serializable]
        public class TextEntry
        {
            public string key;
            public string text;
        }
    }
}