#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AssetDatabaseUtils
{
    public static T FindAndLoadFirstAsset<T>(string[] searchFolders = null) where T : Object
    {
        T result = null;
        string[] guids = FindAllGuidsForType<T>(searchFolders);
        if (guids != null && guids.Length > 0)
            result = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        
        return result;
    }

    public static List<T> FindAndLoadAllAssets<T>(string searchFolders) where T : Object
    {
        return FindAndLoadAllAssets<T>(new string[] { searchFolders });
    }

    public static List<T> FindAndLoadAllAssets<T>(string[] searchFolders = null) where T : Object
    {
        string[] guids = FindAllGuidsForType<T>(searchFolders);

        List<T> results = null;
        if (guids != null && guids.Length > 0)
        {
            results = new List<T>(guids.Length);
            for (int i = 0; i < guids.Length; ++i)
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (asset)
                    results.Add(asset);
            }
        }

        return results;
    }

    public static string[] FindAllGuidsForType<T>(string[] searchFolders = null) where T : Object
    {
        string[] guids = null;

        if (searchFolders == null)
            guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
        else
            guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, searchFolders);

        return guids;
    }
}
#endif