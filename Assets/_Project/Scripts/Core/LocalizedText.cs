using System;
using UnityEngine;

namespace OystermouthCastle.Core
{
    public enum AppLanguage
    {
        English,
        Welsh
    }

    [Serializable]
    public struct LocalizedText
    {
        [TextArea(1, 6)] public string english;
        [TextArea(1, 6)] public string welsh;

        public string Get(AppLanguage language)
        {
            if (language == AppLanguage.Welsh && !string.IsNullOrWhiteSpace(welsh))
            {
                return welsh;
            }

            return english ?? string.Empty;
        }
    }
}
