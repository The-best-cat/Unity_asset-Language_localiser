using UnityEngine;

namespace BlackCatLocalisation
{
    public static class Text
    {
        public static string Translatable(string key, params object[] args)
        {       
            string text = TextManager.GetText(key);
            if (text != null)
            {
                if (args != null && args.Length > 0)
                {
                    text = Substitute(text, args);
                }
                return text;
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Key \"{key}\" doesn't exist.");
#endif
                return key;
            }
        }

        private static string Substitute(string text, params object[] args)
        {            
            return string.Format(text, args);
        }
    }
}