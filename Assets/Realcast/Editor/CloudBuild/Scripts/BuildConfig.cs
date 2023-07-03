using Realcast.Toolkit.Core;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Management;
using static NetworkManagerConfig;

public enum SDCardWritePermission
{
    Internal,
    External
}

public enum VSync
{
    Off = 0,
    EveryVBlank = 1,
    EverySecondVBlank = 2
}

public enum NamedTarget
{
    Standalone,
    Server,
    Android,
    PS5
}

[Serializable]
public class BuildConfig
{
    public PlatformDiscriminator Platform;
    public AndroidSdkVersions MinSDKVersion;
    public string ProjectName;
    [Tooltip("Application identifier with the following format com.company.app")]
    public string ApplicationIdentifier;
    [Tooltip("Application's Identifier on a platform such as Oculus, Pico or iQiYu")]
    public string AppId;
    public MsaaQuality MSAAQuality;
    public VSync VSyncCount;
    public SDCardWritePermission AndroidWritePermission;
    public ColorSpace ColorSpace;
    public GraphicsDeviceType GraphicAPI;
    public bool Use32BitsDisplayBuffer;
    public float RenderScale;
    public bool RunInBackground;
    public TextAsset CustomAndroidManifest;
    public Texture2D Splashscreen;
    public BuildTarget BuildTarget;
    public bool StripEngineCode;
    public ManagedStrippingLevel StrippingLevel;
    public NamedTarget NamedTarget;
    public PlayerPrefabType PlayerPrefabType;
    public string[] AdditionnalScriptingDefineSymbols;
    public string[] AdditionalDependencies;

    private static NamedBuildTarget[] _namedBuildTargetMapping = new NamedBuildTarget[]
    {
        NamedBuildTarget.Standalone,
        NamedBuildTarget.Server,
        NamedBuildTarget.Android,
        NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.PS5)
    };

    public bool Apply()
    {
        Debug.Log($"[BUILD] Applying config {Platform.name}");
        bool appIdentifierOk = applyApplicationIdentifier(ApplicationIdentifier);
        bool appIdOk = Platform.ApplyAppId(AppId);
        bool msaaOk = applyMSAAQualityAndRenderScale(MSAAQuality, RenderScale);
        bool splashscreenOk = ApplySplashScreen(Splashscreen);
        bool appNameOk = applyAppName();
        //bool playerPrefabOk = applyPlayerPrefabType(PlayerPrefabType);

        PlayerSettings.Android.forceSDCardPermission = AndroidWritePermission != SDCardWritePermission.Internal;
        PlayerSettings.colorSpace = ColorSpace;
        PlayerSettings.use32BitDisplayBuffer = Use32BitsDisplayBuffer;
        QualitySettings.vSyncCount = (int)VSyncCount;
        applyXRLoader(Platform.GetXRLoader());
        applyMinSDkVersion(MinSDKVersion);
        applyGraphicsAPI(GraphicAPI);
        applyAndroidManifest(CustomAndroidManifest);
        applyEngineStripping(StripEngineCode, StrippingLevel);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        return splashscreenOk && appIdentifierOk && appIdOk && msaaOk/* && playerPrefabOk*/;
    }

    //private static bool applyPlayerPrefabType(PlayerPrefabType prefabType)
    //{
    //    var networkConfig = AssetDatabaseUtils.FindAndLoadFirstAsset<NetworkManagerConfig>();
    //    if (networkConfig)
    //    {
    //        networkConfig.PrefabType = prefabType;
    //        EditorUtility.SetDirty(networkConfig);
    //        return true;
    //    }
    //    else
    //    {
    //        Debug.LogError("Couldn't find NetworkManagerConfig and set PlayerPrefabType.Bot");
    //        return false;
    //    }
    //}

    private static void applyXRLoader(XRLoader loader)
    {
        if (loader)
        {
            var xrSettings = AssetDatabaseUtils.FindAndLoadFirstAsset<XRGeneralSettingsPerBuildTarget>();
            if (xrSettings)
            {
                var buildtargetGroup = BuildTargetGroupFromBuildTarget(EditorUserBuildSettings.activeBuildTarget);
                if (buildtargetGroup != BuildTargetGroup.Unknown)
                {
                    var xrManagerSettings = xrSettings.ManagerSettingsForBuildTarget(buildtargetGroup);
                    xrManagerSettings.TrySetLoaders(new List<XRLoader> { loader });
                    EditorUtility.SetDirty(xrSettings);
                }
                else
                    Debug.LogError("ApplyXRLoader: unknown BuildTargetGroup");
            }
            else
                Debug.LogError("ApplyXRLoader: couldn't find XRGeneralSettingsPerBuildTarget");
        }
        else
            Debug.LogWarning("ApplyXRLoader: no loader defined");
    }

    private static void applyMinSDkVersion(AndroidSdkVersions minSDKVersion)
    {
        PlayerSettings.Android.minSdkVersion = minSDKVersion;
    }

    private static bool applyApplicationIdentifier(string identifier)
    {
        if (!string.IsNullOrEmpty(identifier))
        {
            var buildTargetGroup = BuildTargetGroupFromBuildTarget(EditorUserBuildSettings.activeBuildTarget);
            Debug.Log($"[BUILD] Build target group : {buildTargetGroup}");
            if (buildTargetGroup != BuildTargetGroup.Unknown)
                PlayerSettings.SetApplicationIdentifier(buildTargetGroup, identifier);
            else
                Debug.LogError("ApplyApplicationIdentifier: unknown BuildTargetGroup");

            return buildTargetGroup != BuildTargetGroup.Unknown;
        }
        else
            Debug.LogWarning("[BUILD] Empty application identifier, skipping");

        return true;
    }

    private static bool applyMSAAQualityAndRenderScale(MsaaQuality msaaQuality, float renderScale)
    {
        int defaultQualityLevel = getDefaultQualityLevelForCurrentPlatform();
        var renderPipeline = QualitySettings.GetRenderPipelineAssetAt(defaultQualityLevel);
        if (renderPipeline is UniversalRenderPipelineAsset universalRenderPipeline)
        {
            QualitySettings.antiAliasing = (int)msaaQuality;
            universalRenderPipeline.msaaSampleCount = (int)msaaQuality;
            universalRenderPipeline.renderScale = renderScale;
            EditorUtility.SetDirty(renderPipeline);

            return true;
        }

        Debug.LogError("ApplyMSAAQualityAndRenderScale: couldn't find render pipeline asset");

        return false;
    }

    private static int getDefaultQualityLevelForCurrentPlatform()
    {
        SerializedObject qualitySettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset")[0]);
        if (qualitySettings != null)
        {
            SerializedProperty perPlatformDefaultQuality = qualitySettings.FindProperty("m_PerPlatformDefaultQuality");
            if (perPlatformDefaultQuality != null && perPlatformDefaultQuality.isArray)
            {
                for (int i = 0; i < perPlatformDefaultQuality.arraySize; ++i)
                {
                    SerializedProperty platformQuality = perPlatformDefaultQuality.GetArrayElementAtIndex(i);
                    if (platformQuality.hasChildren)
                    {
                        string platform = platformQuality.displayName;
                        platformQuality.Next(enterChildren: true);
                        platformQuality.Next(enterChildren: false);

                        int level = platformQuality.intValue;

                        if (EditorUserBuildSettings.activeBuildTarget == Enum.Parse<BuildTarget>(platform))
                            return level;
                    }
                }
            }

            rcDebug.LogError<BuildConfig>("Couldn't find default quality level for current platform...");
        }
        else
            rcDebug.LogError<BuildConfig>("Couldn't load QualitySettings");

        return 0;
    }

    private static void applyGraphicsAPI(GraphicsDeviceType graphicsAPI)
    {
        PlayerSettings.SetGraphicsAPIs(EditorUserBuildSettings.activeBuildTarget, new GraphicsDeviceType[] { graphicsAPI });
    }

    public static bool ApplySplashScreen(Texture2D splashScreen)
    {
#if RC_ANDROID_OVR
        OVRProjectConfig ovrProjectConfig = AssetDatabaseUtils.FindAndLoadFirstAsset<OVRProjectConfig>();
        if (ovrProjectConfig)
        {
            SerializedObject serializedOVRConfig = new SerializedObject(ovrProjectConfig);
            SerializedProperty systemSplashScreen = serializedOVRConfig.FindProperty(nameof(OVRProjectConfig.systemSplashScreen));
            systemSplashScreen.objectReferenceValue = splashScreen;

            serializedOVRConfig.ApplyModifiedProperties();

            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.virtualRealitySplashScreen = null;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            Debug.Log("Splashscreen successfully applied");
        }

        return ovrProjectConfig;
#else
        PlayerSettings.SplashScreen.show = true;
        PlayerSettings.SplashScreen.showUnityLogo = false;
        PlayerSettings.virtualRealitySplashScreen = splashScreen;
        return true;
#endif
    }

    private static void applyAndroidManifest(TextAsset customManifest)
    {
        if (customManifest)
        {
            string androidManifestPath = "Assets/Plugins/Android/AndroidManifest.xml";

            string path = AssetDatabase.GetAssetPath(customManifest);
            if (path != androidManifestPath)
            {
                var androidManifest = AssetDatabase.LoadAssetAtPath<TextAsset>(androidManifestPath);
                File.WriteAllText(androidManifestPath, customManifest.text);
                EditorUtility.SetDirty(androidManifest);
            }
        }
    }

    private static bool applyAppName()
    {
        var appNameLocalization = AssetDatabaseUtils.FindAndLoadFirstAsset<AppNameLocalization>();
        var stringTables = AssetDatabaseUtils.FindAndLoadAllAssets<StringTable>(new string[] { "Assets/game_basketabll_arcade/Settings/Locales" });

        if (appNameLocalization && stringTables != null && stringTables.Count > 0)
        {
            foreach (var stringTable in stringTables)
            {
                SerializedObject serializedObject = new SerializedObject(stringTable);
                SerializedProperty localizedProperty = serializedObject.FindProperty("m_TableData.Array.data[0].m_Localized");

                if (localizedProperty != null)
                {
                    localizedProperty.stringValue = appNameLocalization.GetAppName(stringTable.LocaleIdentifier);
                    serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    Debug.LogError("ApplyAppName: couldn't find m_TableData.Array.data[0].m_Localized on " + stringTable.name);
                    return false;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            return true;
        }

        Debug.Log("ApplyAppName: couldn't appNameLocalization or any StringTable");

        return false;
    }

    private void applyEngineStripping(bool strip, ManagedStrippingLevel level)
    {
        PlayerSettings.stripEngineCode = strip;
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroupFromBuildTarget(EditorUserBuildSettings.activeBuildTarget), level);
    }

    public static BuildTargetGroup BuildTargetGroupFromBuildTarget(BuildTarget buildTarget)
    {
        BuildTargetGroup buildTargetGroup = buildTarget switch
        {
            BuildTarget.Android => BuildTargetGroup.Android,
            BuildTarget.StandaloneWindows64 => BuildTargetGroup.Standalone,
            BuildTarget.StandaloneWindows => BuildTargetGroup.Standalone,
            BuildTarget.PS5 => BuildTargetGroup.PS5,
            BuildTarget.PS4 => BuildTargetGroup.PS4,
            _ => BuildTargetGroup.Unknown
        };

        if (buildTargetGroup == BuildTargetGroup.Unknown)
            Debug.LogError($"BuildTargetGroupFromBuildTarget : target {buildTarget} not handled");

        return buildTargetGroup;
    }

    public static NamedBuildTarget NamedTargetToNamedBuildTarget(NamedTarget namedTarget)
    {
        return _namedBuildTargetMapping[(int)namedTarget];
    }
}