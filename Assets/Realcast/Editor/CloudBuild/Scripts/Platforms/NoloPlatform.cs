using UnityEngine;
using UnityEngine.XR.Management;
#if RC_ANDROID_NOLO
using Unity.XR.GSXR;
#endif

[CreateAssetMenu(fileName = "NoloPlatform", menuName = "Realcast/Build/Nolo Platform")]
public class NoloPlatform : PlatformDiscriminator
{
    public override string ConfigFileName => "nolo";

    public override bool IsBuildable()
    {
#if RC_ANDROID_NOLO
            return true;
#else
        return false;
#endif
    }

    public override XRLoader GetXRLoader()
    {
#if RC_ANDROID_NOLO
        return AssetDatabaseUtils.FindAndLoadFirstAsset<GSXR_Loader>();
#else
        return null;
#endif
    }

    public override bool ApplyAppId(string appId)
    {
        Debug.LogWarning("NoloPlatform: temporary implementation, no AppId applied");
        return true;
    }
}
