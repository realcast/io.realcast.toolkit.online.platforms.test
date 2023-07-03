using UnityEngine;
using UnityEngine.XR.Management;

#if RC_PS5_PSVR2

#endif

[CreateAssetMenu(fileName = "PSVR2Platform", menuName = "Realcast/Build/Playstation VR2 Platform")]
public class PSVR2Platform : PlatformDiscriminator
{
    public override string ConfigFileName => "playstation-vr2";

    public override bool IsBuildable()
    {
#if RC_PS5_PSVR2
            return true;
#else
        return false;
#endif
    }

    public override XRLoader GetXRLoader()
    {
#if RC_PS5_PSVR2
        return AssetDatabaseUtils.FindAndLoadFirstAsset<UnityEngine.XR.PSVR2.PSVR2Loader>();
#else
        return null;
#endif
    }
}
