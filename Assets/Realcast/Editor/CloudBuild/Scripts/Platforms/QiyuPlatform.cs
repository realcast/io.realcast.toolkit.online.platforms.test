using UnityEngine;
using UnityEngine.XR.Management;
#if RC_ANDROID_QIYU
using Unity.XR.Qiyu;
#endif


[CreateAssetMenu(fileName = "QiyuPlatform", menuName = "Realcast/Build/Qiyu Platform")]
public class QiyuPlatform : PlatformDiscriminator
{
    public override string ConfigFileName => "qiyu";

    public override bool IsBuildable()
    {
#if RC_ANDROID_QIYU
            return true;
#else
        return false;
#endif
    }

    public override XRLoader GetXRLoader()
    {
#if RC_ANDROID_QIYU
        return AssetDatabaseUtils.FindAndLoadFirstAsset<QiyuLoader>();
#else
        return null;
#endif
    }

    public override bool ApplyAppId(string appId)
    {
        Debug.LogError("QiYuPlatform: temporary implementation, no AppId applied");
        return true;
    }
}