using System;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Config/Localization/App Name Localization")]
public class AppNameLocalization : ScriptableObject
{
    [Serializable]
    public class Localization
    {
        public Locale Locale;
        public string AppName;
    }

    public Localization[] Localizations;

    public string GetAppName(LocaleIdentifier localeIdentifier)
    {
        for (int i = 0; i < Localizations.Length; ++i)
        {
            if (Localizations[i].Locale.Identifier == localeIdentifier)
                return Localizations[i].AppName;
        }

        return null;
    }
}
