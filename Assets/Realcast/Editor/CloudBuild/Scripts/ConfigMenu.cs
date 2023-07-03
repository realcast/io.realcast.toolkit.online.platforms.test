using UnityEditor;

public class ConfigMenu
{
    [MenuItem("Realcast/Build/Apply Config/Regular/Quest")]
    private static void QuestConfig()
    {
        ConfigUtils.ApplyConfig<OculusQuestPlatform>();
    }

    [MenuItem("Realcast/Build/Apply Config/Regular/Playstation VR2")]
    private static void PSVR2Config()
    {
        ConfigUtils.ApplyConfig<PSVR2Platform>();
    }

    [MenuItem("Realcast/Build/Apply Config/Regular/Nolo")]
    private static void NoloConfig()
    {
        ConfigUtils.ApplyConfig<NoloPlatform>();
    }

    [MenuItem("Realcast/Build/Apply Config/Regular/Pico")]
    private static void PicoConfig()
    {
        ConfigUtils.ApplyConfig<PicoPlatform>();
    }

    [MenuItem("Realcast/Build/Apply Config/Regular/Qiyu")]
    private static void QiyuConfig()
    {
        ConfigUtils.ApplyConfig<QiyuPlatform>();
    }

    [MenuItem("Realcast/Build/Apply Config/Regular/Steam")]
    private static void SteamConfig()
    {
        ConfigUtils.ApplyConfig<SteamPlatform>();
    }

    [MenuItem("Realcast/Build/Apply Config/Regular/YVR")]
    private static void YVRConfig()
    {
        ConfigUtils.ApplyConfig<YVRPlatform>();
    }

    [MenuItem("Realcast/Build/Apply Config/Regular/Bot")]
    private static void BotConfig()
    {
        ConfigUtils.ApplyConfig<BotPlatform>();
    }
}
