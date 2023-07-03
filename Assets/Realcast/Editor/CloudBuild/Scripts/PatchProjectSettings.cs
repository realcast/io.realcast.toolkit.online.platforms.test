using System;
using System.Collections.Generic;
using CloudBuild.Versioning;

public static class PatchProjectSettings
{
    public static void GeneratePatch()
    {
        string output = Git.Run("diff ../ProjectSettings/ProjectSettings.asset");
        Patch patch = Patch.ParsePatch(output);
        KeepOnlyVersionChanges(patch);
        patch.WriteUpdatedPatch("projectsettings.patch");
    }

    private static void KeepOnlyVersionChanges(Patch patch)
    {
        List<int> hunksToDrop = new List<int>();
        var hunks = patch.Hunks;
        for (int i = 0; i < hunks.Count; ++i)
        {
            KeepOnlyVersionChanges(hunks[i]);

            if (Patch.ContainsNoChanges(hunks[i]))
                hunksToDrop.Add(i);
        }

        for (int i = hunksToDrop.Count - 1; i >= 0; --i)
            hunks.RemoveAt(hunksToDrop[i]);
    }

    private static void KeepOnlyVersionChanges(Patch.Hunk hunk)
    {
        string[] versionMarkers = new string[] { "bundleVersion:", "AndroidBundleVersionCode:" };

        List<int> changesIndexToRevert = new List<int>();
        for (int i = 0; i < hunk.Content.Count; ++i)
        {
            if (!Patch.IsUnchanged(hunk.Content[i]))
            {
                if (Array.Find(versionMarkers, marker => hunk.Content[i].Contains(marker)) == null)
                    changesIndexToRevert.Add(i);
            }
        }

        for (int i = changesIndexToRevert.Count - 1; i >= 0; --i)
            hunk.RevertChange(changesIndexToRevert[i]);

        hunk.RefreshHunkHeader();
    }
}
