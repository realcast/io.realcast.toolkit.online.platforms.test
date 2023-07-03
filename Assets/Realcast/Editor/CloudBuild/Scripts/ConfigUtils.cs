using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using static BuildConfigPerPlatform;

public class ConfigUtils
{
    [Serializable]
    private struct ConfigInfos
    {
        public string BuildConfigName;
    }

    public static readonly string LastAppliedConfigKey = "LastAppliedConfig";
    private static string ConfigInfosPath => Path.Combine(Path.GetDirectoryName(Application.dataPath), "config-infos.tmp");

    public static void ApplyConfig<T>() where T : PlatformDiscriminator
    {
        var buildConfigPerPlatform = AssetDatabaseUtils.FindAndLoadFirstAsset<BuildConfigPerPlatform>();
        var platformBuildConfig = GetBuildConfig<T>(buildConfigPerPlatform);
        var buildConfig = platformBuildConfig.BuildConfig;
        if (buildConfig == null)
        {
            Debug.LogError("[CONFIG] No config found for platform " + nameof(T));
            return;
        }

        ApplyConfig(buildConfigPerPlatform, platformBuildConfig);
    }

    public static void ApplyConfig(PlatformDiscriminator platformDiscriminator)
    {
        var buildConfigPerPlatform = AssetDatabaseUtils.FindAndLoadFirstAsset<BuildConfigPerPlatform>();
        var platformBuildConfig = GetBuildConfig(buildConfigPerPlatform, platformDiscriminator);
        var buildConfig = platformBuildConfig.BuildConfig;
        if (buildConfig == null)
        {
            Debug.LogError("[CONFIG] No config found for platform " + platformDiscriminator.name);
            return;
        }

        ApplyConfig(buildConfigPerPlatform, platformBuildConfig);
    }

    private static void ApplyConfig(BuildConfigPerPlatform buildConfigPerPlatform, PlatformBuildConfig platformBuildConfig)
    {
        var buildConfig = platformBuildConfig.BuildConfig;

        if (buildConfig.BuildTarget == BuildTarget.Android)
        {
            AndroidKeystoreCache keystore = new AndroidKeystoreCache(platformBuildConfig.Name);
            if (!keystore.CacheIsValid())
            {
                AndroidKeystoreDialog.ShowWindow(keystore);

                if (keystore.CacheIsValid())
                    keystore.ApplyCache();
            }
        }

        List<string> scriptingDefineSymbols = new List<string>();
        scriptingDefineSymbols.AddRange(buildConfigPerPlatform.CommonScriptingDefineSymbols);
        scriptingDefineSymbols.AddRange(buildConfig.AdditionnalScriptingDefineSymbols);

        bool needScriptRebuild = false;
        try
        {
            EditorApplication.LockReloadAssemblies();

            var buildTargetGroup = BuildConfig.BuildTargetGroupFromBuildTarget(buildConfig.BuildTarget);
            if (ScriptingDefineSymbolsNeedUpdate(buildConfig.NamedTarget, scriptingDefineSymbols.ToArray()))
            {
                PlayerSettings.SetScriptingDefineSymbols(BuildConfig.NamedTargetToNamedBuildTarget(buildConfig.NamedTarget), scriptingDefineSymbols.ToArray());
                needScriptRebuild = true;
            }

            if (!SwitchBuildTarget(BuildConfig.NamedTargetToNamedBuildTarget(buildConfig.NamedTarget), buildConfig.BuildTarget))
                needScriptRebuild = true;

            if (OutdatedDependencies(buildConfigPerPlatform.CommonDependencies, buildConfig.AdditionalDependencies))
            {
                DependencyManager.UpdateDependencies(buildConfigPerPlatform.CommonDependencies, buildConfig.AdditionalDependencies);
                needScriptRebuild = true;
            }

            if (needScriptRebuild)
                WriteConfigInfosToDisk(platformBuildConfig.Name);
        }
        catch (Exception ex)
        {
            Debug.LogError("[CONFIG] An exception occured while trying to change scripting define symbols and packages list : " + ex.Message);
        }
        finally
        {
            EditorApplication.UnlockReloadAssemblies();
        }

        if (!needScriptRebuild)
        {
            if (buildConfig.Apply())
            {
                PlayerPrefs.SetString(LastAppliedConfigKey, platformBuildConfig.Name);
                Debug.Log("[CONFIG] Successfully applied config " + platformBuildConfig.Name);
            }
            else
                Debug.LogError("[CONFIG] Failed to apply config " + platformBuildConfig.Name);
        }
    }

    private static bool OutdatedDependencies(string[] commonDependencies, string[] additionalDependencies)
    {
        List<DependencyManager.Dependency> installedDependencies = new List<DependencyManager.Dependency>();
        var iterator = DependencyManager.GetInstalledDependencies(installedDependencies);

        while (iterator.MoveNext())
            Thread.Sleep(100);

        if (installedDependencies.Count != (commonDependencies.Length + additionalDependencies.Length))
            return true;

        Dictionary<string, string> candidateDependencies = new Dictionary<string, string>();
        foreach (var dependencyString in commonDependencies)
        {
            var dependency = DependencyManager.ParseDependency(dependencyString);
            candidateDependencies.Add(dependency.Identifier, dependency.Source);
        }

        foreach (var dependencyString in additionalDependencies)
        {
            var dependency = DependencyManager.ParseDependency(dependencyString);
            candidateDependencies.Add(dependency.Identifier, dependency.Source);
        }

        if (installedDependencies.Count != candidateDependencies.Count)
            return true;

        foreach (var dependency in installedDependencies)
        {
            if (!candidateDependencies.ContainsKey(dependency.Identifier) || candidateDependencies[dependency.Identifier] != dependency.Source)
                return true;
        }

        return false;
    }

    private static bool ScriptingDefineSymbolsNeedUpdate(NamedTarget namedTarget, string[] desiredSymbols)
    {
        PlayerSettings.GetScriptingDefineSymbols(BuildConfig.NamedTargetToNamedBuildTarget(namedTarget), out string[] currentSymbols);

        if (currentSymbols.Length != desiredSymbols.Length)
            return true;

        for (int i = 0; i < currentSymbols.Length; ++i)
        {
            if (currentSymbols[i] != desiredSymbols[i])
                return true;
        }

        return false;
    }

    private static void WriteConfigInfosToDisk(string buildConfigName)
    {
        var jsonConfig = JsonUtility.ToJson(new ConfigInfos()
        {
            BuildConfigName = buildConfigName
        },
        prettyPrint: true);

        File.WriteAllText(ConfigInfosPath, jsonConfig);
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void ReadAndApplyConfigInfos()
    {
        string jsonConfig = null;

        if (File.Exists(ConfigInfosPath))
            jsonConfig = File.ReadAllText(ConfigInfosPath);

        if (!string.IsNullOrEmpty(jsonConfig))
        {
            var configInfos = JsonUtility.FromJson<ConfigInfos>(jsonConfig);

            try
            {
                if (File.Exists(ConfigInfosPath))
                    File.Delete(ConfigInfosPath);
            }
            catch (IOException ex)
            {
                Debug.LogError("[CONFIG] ReadAndApplyConfigInfos: couldn't delete config-infos, this might re-apply the changes induced by " + configInfos.BuildConfigName + ".\n" + ex.Message);
            }

            var buildConfigPerPlatform = AssetDatabaseUtils.FindAndLoadFirstAsset<BuildConfigPerPlatform>();
            if (buildConfigPerPlatform)
            {
                var config = buildConfigPerPlatform.BuildConfigs.Find((config) => config.Name == configInfos.BuildConfigName);
                if (config != null)
                {
                    Debug.Log($"[CONFIG] Add apply config {config.Name} to editor delay call");
                    EditorApplication.delayCall += () =>
                    {
                        if (config.BuildConfig.Apply())
                        {
                            PlayerPrefs.SetString(LastAppliedConfigKey, config.Name);
                            Debug.Log("[CONFIG] Successfully applied config " + config.Name);
                        }
                        else
                            Debug.LogError("[CONFIG] Failed to apply config " + config.Name);
                    };
                }
                else
                    Debug.LogError("[CONFIG] ReadAndApplyConfigInfos: failed to find build config " + configInfos.BuildConfigName);
            }
            else
                Debug.LogError("[CONFIG] ReadAndApplyConfigInfos: failed to find BuildConfigPerPlatform asset");
        }
    }

    private static bool SwitchBuildTarget(NamedBuildTarget namedBuildTarget, BuildTarget target)
    {
        var currentNamedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target));
        if (EditorUserBuildSettings.activeBuildTarget != target || currentNamedBuildTarget != namedBuildTarget)
        {
            string warningMsg;
            if (currentNamedBuildTarget != namedBuildTarget)
                warningMsg = "Unity needs to switch build target to " + namedBuildTarget.TargetName + ". Are you sure ?";
            else
                warningMsg = "Unity needs to switch build target to " + target + ". Are you sure ?";

            if (EditorUtility.DisplayDialog("Switching build target", warningMsg, "Yes", "Cancel"))
                return EditorUserBuildSettings.SwitchActiveBuildTarget(namedBuildTarget, target);

            return false;
        }

        return true;
    }

    private static PlatformBuildConfig GetBuildConfig<T>(BuildConfigPerPlatform buildConfigs)
    {
        if (buildConfigs)
        {
            var buildConfig = buildConfigs.BuildConfigs.Find((config) => config.BuildConfig.Platform is T);
            if (buildConfig != null)
                return buildConfig;
            else
                Debug.LogError("[CONFIG] Couldn't find build config " + nameof(T));
        }
        else
            Debug.LogError("[CONFIG] Couldn't find build config list");

        return null;
    }

    private static PlatformBuildConfig GetBuildConfig(BuildConfigPerPlatform buildConfigs, PlatformDiscriminator platform)
    {
        if (platform)
        {
            var buildConfig = buildConfigs.BuildConfigs.Find((config) => config.BuildConfig.Platform == platform);
            if (buildConfig != null)
                return buildConfig;
            else
                Debug.LogError("[CONFIG] Couldn't find build config " + platform.name);
        }
        else
            Debug.LogError("[CONFIG] Couldn't find build config list");

        return null;
    }
}
