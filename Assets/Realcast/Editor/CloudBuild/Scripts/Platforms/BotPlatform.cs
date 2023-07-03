using UnityEngine.XR.Management;
using UnityEngine;

[CreateAssetMenu(fileName = "BotPlatform", menuName = "Realcast/Build/Bot Platform")]
public class BotPlatform : PlatformDiscriminator
{
    public override string ConfigFileName => "bot";

    public override XRLoader GetXRLoader()
    {
        return null;
    }

    public override bool IsBuildable()
    {
#if UNITY_SERVER
        return true;
#else
        return false;
#endif
    }
}
