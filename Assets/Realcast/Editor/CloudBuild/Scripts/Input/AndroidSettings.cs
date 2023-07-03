using System.Collections.Generic;
using UnityEditor;

namespace CloudBuild.Input
{
    public class AndroidSettings
    {
        public static void Apply(Dictionary<string, string> options, ProjectSettingsBackup backup)
        {
            EditorUserBuildSettings.buildAppBundle = options["customBuildPath"].EndsWith(".aab");
            if (options.TryGetValue("androidKeystoreName", out string keystoreName) && !string.IsNullOrEmpty(keystoreName))
            {
                backup.KeystoreName = PlayerSettings.Android.keystoreName;
                PlayerSettings.Android.keystoreName = keystoreName;
            }

            if (options.TryGetValue("androidKeystorePass", out string keystorePass) && !string.IsNullOrEmpty(keystorePass))
            {
                backup.KeystorePass = PlayerSettings.Android.keystorePass;
                PlayerSettings.Android.keystorePass = keystorePass;
            }

            if (options.TryGetValue("androidKeyaliasName", out string keyaliasName) && !string.IsNullOrEmpty(keyaliasName))
            {
                backup.KeyAliasName = PlayerSettings.Android.keyaliasName;
                PlayerSettings.Android.keyaliasName = keyaliasName;
            }
                
            if (options.TryGetValue("androidKeyaliasPass", out string keyaliasPass) && !string.IsNullOrEmpty(keyaliasPass))
            {
                backup.keyAliasPass = PlayerSettings.Android.keyaliasPass;
                PlayerSettings.Android.keyaliasPass = keyaliasPass;
            }

            backup.UseCustomKeystore = PlayerSettings.Android.useCustomKeystore;
            PlayerSettings.Android.useCustomKeystore = (!string.IsNullOrEmpty(keystoreName) ||
                                                        !string.IsNullOrEmpty(keystorePass) ||
                                                        !string.IsNullOrEmpty(keyaliasPass) ||
                                                        !string.IsNullOrEmpty(keyaliasName));
        }
    }
}
