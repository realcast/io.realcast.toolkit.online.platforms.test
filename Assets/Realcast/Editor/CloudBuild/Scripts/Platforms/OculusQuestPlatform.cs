using UnityEngine;
using UnityEngine.XR.Management;
#if RC_ANDROID_OVR || RC_XPF_OVR
using Unity.XR.Oculus;
using Oculus.Platform;
#endif

[CreateAssetMenu(fileName = "OculusQuestPlatform", menuName = "Realcast/Build/Oculus Quest Platform")]
public class OculusQuestPlatform : PlatformDiscriminator
{
    public override string ConfigFileName => "oculus-quest";

    public override bool IsBuildable()
    {
#if RC_ANDROID_OVR
        return true;
#else
        return false;
#endif
    }

    public override XRLoader GetXRLoader()
    {
#if RC_ANDROID_OVR || RC_XPF_OVR
        return AssetDatabaseUtils.FindAndLoadFirstAsset<OculusLoader>();
#else
        return null;
#endif
    }

    public override bool ApplyAppId(string appId)
    {
#if RC_ANDROID_OVR
        PlatformSettings.AppID = appId;
        PlatformSettings.MobileAppID = appId;
        return true;
#else
        return false;
#endif
    }
}
