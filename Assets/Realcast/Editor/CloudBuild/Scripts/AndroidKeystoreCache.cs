using CloudBuild.Versioning;
using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

public class AndroidKeystoreCache
{
    public string KeystorePath;
    public string KeystorePass;
    public string KeystoreAlias;
    public string KeystoreAliasPass;

    private string _platformName;

    public AndroidKeystoreCache(string platformName)
    {
        Load(platformName);
    }

    public void Load(string platformName)
    {
        _platformName = platformName;
        KeystorePath = PlayerPrefs.GetString(_platformName + "-path");
        KeystorePass = PlayerPrefs.GetString(_platformName + "-pass");
        KeystoreAlias = PlayerPrefs.GetString(_platformName + "-alias");
        KeystoreAliasPass = PlayerPrefs.GetString(_platformName + "-aliasPass");
    }

    public void Save()
    {
        PlayerPrefs.SetString(_platformName + "-path", KeystorePath);
        PlayerPrefs.SetString(_platformName + "-pass", KeystorePass);
        PlayerPrefs.SetString(_platformName + "-alias", KeystoreAlias);
        PlayerPrefs.SetString(_platformName + "-aliasPass", KeystoreAliasPass);
    }

    public static void ClearCache(string platformName)
    {
        PlayerPrefs.DeleteKey(platformName + "-path");
        PlayerPrefs.DeleteKey(platformName + "-pass");
        PlayerPrefs.DeleteKey(platformName + "-alias");
        PlayerPrefs.DeleteKey(platformName + "-aliasPass");
    }

    public void ApplyCache()
    {
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = KeystorePath;
        PlayerSettings.Android.keystorePass = KeystorePass;
        PlayerSettings.Android.keyaliasName = KeystoreAlias;
        PlayerSettings.Android.keyaliasPass = KeystoreAliasPass;
    }

    public bool CacheIsValid()
    {
        return KeystorePasswordIsValid() && KeystoreAliasPassIsValid();
    }

    public bool KeystorePasswordIsValid()
    {
        if (!string.IsNullOrEmpty(KeystorePath))
        {
            string arguments = "-list -keystore \"" + KeystorePath + "\" -storepass \"" + KeystorePass + "\"";
            if (RunKeyTool(arguments, out string output, out string errors) == 0 && !output.Contains("java.io.IOException"))
                return true;
        }

        return false;
    }

    public bool KeystoreAliasPassIsValid()
    {
        if (!string.IsNullOrEmpty(KeystorePath))
        {
            string arguments = "-keypasswd -keystore \"" + KeystorePath + "\" -storepass \"" + KeystorePass + "\" -alias " + KeystoreAlias + " -keypass \"" + KeystoreAliasPass + "\" -new \"" + KeystoreAliasPass + "\"";
            if (RunKeyTool(arguments, out string output, out string errors) == 0 &&
                (!output.Contains("java.security.UnrecoverableKeyException") || !output.Contains("java.lang.Exception")))
            {
                try
                {
                    Git.Run("checkout " + KeystorePath);
                }
                catch (GitException ex)
                {
                    Debug.LogError("Couldn't revert change on keystore " + KeystorePath + " : " + ex.Message);
                    return false;
                }

                return true;
            }
        }

        return false;
    }

    private int RunKeyTool(string arguments, out string output, out string errors)
    {
        using (var process = new System.Diagnostics.Process())
        {
            string workingDirectory = Path.Combine(AndroidExternalToolsSettings.jdkRootPath, "bin");
#if UNITY_EDITOR_WIN
            string executablePath = Path.Combine(workingDirectory, "keytool.exe");
#else
            string executablePath = Path.Combine(workingDirectory, "keytool");
#endif
            int code = process.Run(executablePath, arguments, workingDirectory, out output, out errors);

            return code;
        }
    }
}
