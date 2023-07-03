#if !UNITY_CLOUD_BUILD && UNITY_ANDROID
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class AndroidKeystoreBuildPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (PlayerSettings.Android.useCustomKeystore)
        {
            string lastAppliedConfig = PlayerPrefs.GetString(ConfigUtils.LastAppliedConfigKey);
            if (!string.IsNullOrEmpty(lastAppliedConfig))
            {
                AndroidKeystoreCache keystore = new AndroidKeystoreCache(lastAppliedConfig);

                if (keystore.CacheIsValid())
                    keystore.ApplyCache();
                else
                    Debug.LogWarning("AndroidKeystoreBuildPreprocessor: couldn't retrieve last build config");
            }
        }
    }
}
#endif