using System.IO;
using UnityEditor;
using UnityEngine;

public class AndroidKeystoreDialog : EditorWindow
{
    AndroidKeystoreCache _keystore;

    private bool _keystorePassValid;
    private bool _aliasPassValid;

    public bool KeystoreOk => _keystorePassValid && _aliasPassValid;

    public static void ShowWindow(AndroidKeystoreCache keystoreCache)
    {
        var window = (AndroidKeystoreDialog)GetWindow(typeof(AndroidKeystoreDialog), utility: true, "Android Keystore Settings", focus: true);
        window.Initialize(keystoreCache);
        window.ShowModalUtility();
    }

    private void Initialize(AndroidKeystoreCache keystoreCache)
    {
        _keystore = keystoreCache;
    }

    private void Save()
    {
        _keystore.Save();
    }

    public void OnGUI()
    {
        EditorGUILayout.LabelField("Android Keystore Manager");

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Set Keystore path "))
                _keystore.KeystorePath = EditorUtility.OpenFilePanel("Open Keystore", Path.GetDirectoryName(Application.dataPath), "");

            EditorGUILayout.LabelField(_keystore.KeystorePath);
        }
        _keystore.KeystorePass = EditorGUILayout.PasswordField(new GUIContent("Keystore password " + (_keystorePassValid ? "\u2713" : "\u2573")), _keystore.KeystorePass);

        _keystore.KeystoreAlias = EditorGUILayout.TextField(new GUIContent("Keystore alias"), _keystore.KeystoreAlias);
        _keystore.KeystoreAliasPass = EditorGUILayout.PasswordField(new GUIContent("Keystore alias password " + (_aliasPassValid ? "\u2713" : "\u2573")), _keystore.KeystoreAliasPass);

        if (GUILayout.Button(new GUIContent("Check passwords")))
        {
            if (!string.IsNullOrEmpty(_keystore.KeystorePath))
                _keystorePassValid = _keystore.KeystorePasswordIsValid();

            if (!string.IsNullOrEmpty(_keystore.KeystoreAliasPass))
                _aliasPassValid = _keystore.KeystoreAliasPassIsValid();

            if (KeystoreOk)
                Save();
        }
    }
}
