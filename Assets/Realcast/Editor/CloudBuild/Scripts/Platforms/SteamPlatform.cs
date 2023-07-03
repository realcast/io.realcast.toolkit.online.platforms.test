using UnityEngine;
using UnityEngine.XR.Management;

#if RC_WINDOWS_STEAM
using Unity.XR.OpenVR;
#endif

[CreateAssetMenu(fileName = "SteamPlatform", menuName = "Realcast/Build/Steam Platform")]
public class SteamPlatform : PlatformDiscriminator
{
    public override string ConfigFileName => "steam";

    public override bool IsBuildable()
    {
#if RC_WINDOWS_STEAM
            return true;
#else
        return false;
#endif
    }

    public override XRLoader GetXRLoader()
    {
#if RC_WINDOWS_STEAM
        return AssetDatabaseUtils.FindAndLoadFirstAsset<OpenVRLoader>();
#else
        return null;
#endif
    }
}
