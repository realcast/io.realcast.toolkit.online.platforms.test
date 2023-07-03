using System;
using UnityEditor;

namespace CloudBuild.Versioning
{
    public static class VersionApplicator
    {
        public static void SetAppVersionFromEnvVars()
        {
         	PlayerSettings.bundleVersion = Environment.GetEnvironmentVariable("VERSION");
        }

        public static void SetAndroidVersionCode(string androidVersionCode)
        {
            PlayerSettings.Android.bundleVersionCode = Int32.Parse(androidVersionCode);
        }

        public static void IncrementAndroidVersionCode()
        {
            ++PlayerSettings.Android.bundleVersionCode;
        }
    }
}
