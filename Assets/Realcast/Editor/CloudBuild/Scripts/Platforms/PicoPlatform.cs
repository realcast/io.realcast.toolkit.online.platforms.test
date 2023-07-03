using UnityEngine;
using UnityEngine.XR.Management;
#if RC_ANDROID_PICO
using Unity.XR.PXR;
#endif

[CreateAssetMenu(fileName = "PicoPlatform", menuName = "Realcast/Build/Pico Platform")]
public class PicoPlatform : PlatformDiscriminator
{
    public override string ConfigFileName => "pico";

    public override bool IsBuildable()
    {
#if RC_ANDROID_PICO
            return true;
#else
        return false;
#endif
    }

    public override XRLoader GetXRLoader()
    {
#if RC_ANDROID_PICO
        return AssetDatabaseUtils.FindAndLoadFirstAsset<PXR_Loader>();
#else
        return null;
#endif
    }

    public override bool ApplyAppId(string appId)
    {
        Debug.LogError("PicoPlatform: temporary implementation, no AppId applied");
        return true;
    }
}