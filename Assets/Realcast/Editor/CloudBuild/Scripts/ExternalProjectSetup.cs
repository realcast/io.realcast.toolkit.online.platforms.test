#if !UNITY_CLOUD_BUILD
using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class ExternalProjectSetup
{
    private static string ProjectExternalSetupPath => Path.Combine(Path.GetDirectoryName(Application.dataPath), "external-project-setup.tmp");

    static ExternalProjectSetup()
    {
        EditorApplication.update += RunOnce;
    }

    private static void RunOnce()
    {
        EditorApplication.update -= RunOnce;
        ApplyExternalProjectSetup();
    }

    [MenuItem("Realcast/Build/Debug/External Project Setup")]
    private static void ApplyExternalProjectSetup()
    {
        if (File.Exists(ProjectExternalSetupPath))
        {
            var buildConfigPerPlatform = AssetDatabaseUtils.FindAndLoadFirstAsset<BuildConfigPerPlatform>();
            if (buildConfigPerPlatform)
            {
                string configContent = File.ReadAllText(ProjectExternalSetupPath);

                for (int i = 0; i < buildConfigPerPlatform.BuildConfigs.Count; ++i)
                {
                    if (buildConfigPerPlatform.BuildConfigs[i] != null &&
                        buildConfigPerPlatform.BuildConfigs[i].BuildConfig != null &&
                        buildConfigPerPlatform.BuildConfigs[i].BuildConfig.Platform != null &&
                        configContent.StartsWith(buildConfigPerPlatform.BuildConfigs[i].BuildConfig.Platform.ConfigFileName))
                    {
                        File.Delete(ProjectExternalSetupPath);

                        var platform = buildConfigPerPlatform.BuildConfigs[i].BuildConfig.Platform;
                        bool isDemo = configContent.EndsWith("demo");

                        Debug.Log($"Applying config for {platform.name}");
                        
                        ConfigUtils.ApplyConfig(platform);
                        break;
                    }
                }
            }
        }
    }
}
#endif