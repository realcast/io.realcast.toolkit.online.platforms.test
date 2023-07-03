using UnityEngine;
using UnityEngine.XR.Management;

public abstract class PlatformDiscriminator : ScriptableObject
{
    public abstract string ConfigFileName { get; }

    public abstract bool IsBuildable();

    public abstract XRLoader GetXRLoader();

    public virtual bool ApplyAppId(string appId)
    {
        return true;
    }
}