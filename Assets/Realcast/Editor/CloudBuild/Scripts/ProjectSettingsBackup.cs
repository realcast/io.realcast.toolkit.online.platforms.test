using UnityEditor;

public class ProjectSettingsBackup
{
    public string ScriptingDefineSymbols = null;
    public string KeystoreName = null;
    public string KeystorePass = null;
    public string KeyAliasName = null;
    public string keyAliasPass = null;
    public bool? UseCustomKeystore = null;

    public void Apply(BuildTargetGroup targetGroup)
    {
        if (ScriptingDefineSymbols != null)
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, ScriptingDefineSymbols);

        if (KeystoreName != null)
            PlayerSettings.Android.keystoreName = KeystoreName;

        if (KeystorePass != null)
            PlayerSettings.Android.keystorePass = KeystorePass;

        if (KeyAliasName != null)
            PlayerSettings.Android.keyaliasName = KeyAliasName;

        if (keyAliasPass != null)
            PlayerSettings.Android.keyaliasPass = keyAliasPass;

        if (UseCustomKeystore.HasValue)
            PlayerSettings.Android.useCustomKeystore = UseCustomKeystore.Value;
    }
}
