using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class DependencyManager
{
    public delegate IEnumerator CoroutineCallback<T>(T arg);

    public class Dependency
    {
        public string Identifier;
        public string Source;
        public DependencyType Type;
    }

    public enum DependencyType
    {
        URI,
        Registry
    }

    private static string ManifestJsonPath => Path.Combine(Path.GetDirectoryName(Application.dataPath), "Packages/manifest.json");

    public static void UpdateDependencies(string[] commonDependencies, string[] additionalDependencies = null)
    {
        List<string> wantedDependenciesRaw = new List<string>();

        wantedDependenciesRaw.AddRange(commonDependencies);

        if (additionalDependencies != null && additionalDependencies.Length > 0)
            wantedDependenciesRaw.AddRange(additionalDependencies);

        StringBuilder manifestBuilder = new StringBuilder();

        manifestBuilder.AppendLine("{\n  \"dependencies\": {");

        for (int i = 0; i < wantedDependenciesRaw.Count; ++i)
        {
            manifestBuilder.Append("    " + wantedDependenciesRaw[i]);

            if (i != wantedDependenciesRaw.Count - 1)
                manifestBuilder.AppendLine(",");
            else
                manifestBuilder.AppendLine();
        }

        manifestBuilder.AppendLine("  }\n}");

        File.WriteAllText(ManifestJsonPath, manifestBuilder.ToString()); 
    }

    public static Dependency ParseDependency(string line)
    {
        try
        {
            string cleanedLine = line.Trim();
            if (cleanedLine.EndsWith(","))
                cleanedLine = cleanedLine.TrimEnd(',');

            int separatorIndex = cleanedLine.IndexOf(':');
            string identifier = cleanedLine.Substring(0, separatorIndex);
            identifier = identifier.Trim('"');

            string source = cleanedLine.Substring(separatorIndex + 1);
            source = source.Trim();
            source = source.Trim('"');

            DependencyType type = Uri.CheckSchemeName(source) ? DependencyType.URI : DependencyType.Registry;

            return new Dependency() { Identifier = identifier, Source = source, Type = type };
        }
        catch
        {
            return null;
        }
    }

    public static IEnumerator GetInstalledDependencies(List<Dependency> packages)
    {
        var packageListRequest = UnityEditor.PackageManager.Client.List(offlineMode: true);

        while (!packageListRequest.IsCompleted)
            yield return null;

        foreach (var package in packageListRequest.Result)
        {
            var packageIdParts = package.packageId.Split('@');
            if (packageIdParts.Length == 2)
            {
                DependencyType type = Uri.CheckSchemeName(packageIdParts[1]) ? DependencyType.URI : DependencyType.Registry;
                packages.Add(new Dependency { Identifier = package.name, Source = packageIdParts[1], Type = type});
            }
            else
                Debug.LogError(nameof(DependencyManager) + ": couldn't parse packageId " + package.packageId);
        }
    }
}
