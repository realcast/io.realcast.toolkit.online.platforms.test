using System;
using UnityEditor;
using UnityEngine;

namespace CloudBuild
{
    public static class PreBuilder
    {
        public static void PrepareNightlyBuilds()
        {
            updateBundleVersion();

            // We change the scripting backend so that the build completes faster.
            // But this change shouldn't be versioned since our git-patch process run
            // when the build is complete will filter out any changes not related to 
            // android bundle version code or game version.
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
        }

        public static void PrepareBuild()
        {
            try
            {
#if UNITY_ANDROID
                setAndroidTextureCompressionFromEnvVars();
                EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Debugging;
#endif

#if USE_PLAYFAB
                setPlayFabInstanceFromEnvVars();
#endif

                prepareSelectedPlatform();
            }
            catch (Exception e)
            {
                Debug.LogError("PreBuilder: uncaught exception during PrepareBuild : " + e.Message);
                Debug.Log(e.StackTrace);
                EditorApplication.Exit(1);
            }
        }

        private static void updateBundleVersion()
        {
            ++PlayerSettings.Android.bundleVersionCode;

            string[] bundleVersionParts = PlayerSettings.bundleVersion.Split('.');
            if (bundleVersionParts.Length == 3)
                PlayerSettings.bundleVersion = bundleVersionParts[0] + '.' + bundleVersionParts[1] + '.' + PlayerSettings.Android.bundleVersionCode;
            else
                Debug.LogError($"PreBuilder: ill-formed bundleVersion {PlayerSettings.bundleVersion}, leaving as it is...");
        }

        private static void prepareSelectedPlatform()
        {
            var buildConfigs = AssetDatabaseUtils.FindAndLoadFirstAsset<BuildConfigPerPlatform>();
            var selectedConfig = buildConfigs.BuildConfigs.Find((config) => config.BuildConfig.Platform.IsBuildable());
            if (selectedConfig == null)
            {
                Debug.LogError("PreBuilder: couldn't find a buildable config for the current platform");
                EditorApplication.Exit(1);
            }

            Debug.Log("PrepareSelectedPlatform: selected " + selectedConfig.Name + " platform !");

            if (!selectedConfig.BuildConfig.Apply())
            {
                Debug.LogError("PreBuilder: error when applying config " + selectedConfig.Name);
                EditorApplication.Exit(1);
            }
        }

        private static void setAndroidTextureCompressionFromEnvVars()
        {
            string EnvironmentCompressionVariable = Environment.GetEnvironmentVariable("COMPRESSION_TEXTURE");
            var textureCompression = EnvironmentCompressionVariable switch
            {
                "dxt" => MobileTextureSubtarget.DXT,
                "pvrtc" => MobileTextureSubtarget.PVRTC,
                "etc" => MobileTextureSubtarget.ETC,
                "etc2" => MobileTextureSubtarget.ETC2,
                "astc" => MobileTextureSubtarget.ASTC,
                _ => MobileTextureSubtarget.Generic,
            };
            EditorUserBuildSettings.androidBuildSubtarget = textureCompression;
        }

        private static void setPlayFabInstanceFromEnvVars()
        {
            string playfabInstanceId = Environment.GetEnvironmentVariable("PLAYFAB_INSTANCE_ID");
            if (!string.IsNullOrEmpty(playfabInstanceId))
            {
                var playFabSharedSettings = AssetDatabaseUtils.FindAndLoadFirstAsset<PlayFabSharedSettings>();
                if (playFabSharedSettings)
                {
                    playFabSharedSettings.TitleId = playfabInstanceId;
                    playFabSharedSettings.ProductionEnvironmentUrl = $"https://{playfabInstanceId.ToLower()}.playfabapi.com";

                    EditorUtility.SetDirty(playFabSharedSettings);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}
