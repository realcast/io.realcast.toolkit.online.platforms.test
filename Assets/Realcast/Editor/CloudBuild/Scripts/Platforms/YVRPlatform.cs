using UnityEngine;
using UnityEngine.XR.Management;
#if RC_ANDROID_YVR
using YVR.Core.XR;
#endif

[CreateAssetMenu(fileName = "YVRPlatform", menuName = "Realcast/Build/YVR Platform")]
public class YVRPlatform : PlatformDiscriminator
{
    public override string ConfigFileName => "yvr";

    public override bool IsBuildable()
    {
#if RC_ANDROID_YVR
        return true;
#else
        return false;
#endif
    }

    public override XRLoader GetXRLoader()
    {
#if RC_ANDROID_YVR
        return AssetDatabaseUtils.FindAndLoadFirstAsset<YVRXRLoader>();
#else
        return null;
#endif
    }

    public override bool ApplyAppId(string appId)
    {
        Debug.LogWarning("YVRPlatform: temporary implementation, no AppId applied");
        return true;
    }
}
