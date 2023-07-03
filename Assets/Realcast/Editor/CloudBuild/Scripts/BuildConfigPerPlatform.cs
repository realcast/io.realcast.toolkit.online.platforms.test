using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildConfigPerPlatform", menuName = "Realcast/Build/Build Config")]
public class BuildConfigPerPlatform : ScriptableObject
{
    [Serializable]
    public class PlatformBuildConfig
    {
        public string Name;
        public BuildConfig BuildConfig;
    }

    public List<PlatformBuildConfig> BuildConfigs;
    public string[] CommonScriptingDefineSymbols;
    public string[] CommonDependencies;

    [ContextMenu("Clear Keystore Cache")]
    private void ClearKeystoreCache()
    {
        foreach (var config in BuildConfigs)
            AndroidKeystoreCache.ClearCache(config.Name);
    }

    [ContextMenu("Lock Assembly Reload")]
    private void LockAssemblyReload()
    {
        EditorApplication.LockReloadAssemblies();
    }

    [ContextMenu("Unlock Assembly Reload")]
    private void UnlockAssemblyReload()
    {
        EditorApplication.UnlockReloadAssemblies();
    }

    private void GenerateCloudBuildDependenciesFiles()
    {
        foreach (var platform in BuildConfigs)
        {
            List<string> dependenciesLines = new List<string>();
            dependenciesLines.AddRange(CommonDependencies);
            dependenciesLines.AddRange(platform.BuildConfig.AdditionalDependencies);

            StringBuilder dependenciesContent = new StringBuilder();
            dependenciesContent.Append("{\n  \"dependencies\": {\n");

            for (int i = 0; i < dependenciesLines.Count; ++i)
            {
                dependenciesContent.Append("    " + dependenciesLines[i]);
                if (i != dependenciesLines.Count - 1)
                    dependenciesContent.AppendLine(",");
                else
                    dependenciesContent.AppendLine();
            }

            dependenciesContent.Append("  }\n}");

            string filePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Unity-Cloud-Build/platform-dependencies/" + platform.BuildConfig.Platform.ConfigFileName).Replace(" ", "-").ToLower();
            File.WriteAllText(filePath, dependenciesContent.ToString());
        }
    }

    private void GenerateCloudBuildScriptingDefineFiles()
    {
        foreach (var platform in BuildConfigs)
        {
            List<string> scriptingSymbols = new List<string>();
            scriptingSymbols.AddRange(CommonScriptingDefineSymbols);
            scriptingSymbols.AddRange(platform.BuildConfig.AdditionnalScriptingDefineSymbols);

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < scriptingSymbols.Count; ++i)
            {
                stringBuilder.Append(scriptingSymbols[i]);
                if (i != scriptingSymbols.Count - 1)
                    stringBuilder.Append(';');
            }

            string filePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Unity-Cloud-Build/platform-scripting-define-symbols/" + platform.BuildConfig.Platform.ConfigFileName).Replace(" ", "-").ToLower();
            File.WriteAllText(filePath, stringBuilder.ToString());
        }
    }

    private void OnValidate() 
    {
        GenerateCloudBuildDependenciesFiles();
        GenerateCloudBuildScriptingDefineFiles();
    }
}
