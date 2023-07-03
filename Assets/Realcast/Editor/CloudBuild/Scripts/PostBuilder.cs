using System;
using System.IO;
using UnityEditor;

namespace CloudBuild
{
    public static class PostBuilder
    {
        public static void PrepareNightlyBuilds()
        {
            PatchProjectSettings.GeneratePatch();
        }

        public static void CopyAndroidDebugSymbols(string exportPath)
        {
            var buildTargetGroup = BuildConfig.BuildTargetGroupFromBuildTarget(EditorUserBuildSettings.activeBuildTarget);

            if (PlayerSettings.stripEngineCode && PlayerSettings.GetManagedStrippingLevel(buildTargetGroup) != ManagedStrippingLevel.Disabled)
            {
                string targetDirectory = @"Unity-Cloud-Build/Debug";
                string sourceDirectory = @"Temp/StagingArea/libs/arm64-v8a";
                Copy(sourceDirectory, targetDirectory, ".so");
            }
            else
                Console.WriteLine($"[{nameof(PostBuilder)}]: error, exporting debug symbols while stripEngineCode is disabled not supported yet");
        }

        private static void Copy(string sourceFolder, string targetFolder, string fileFilter)
        {
            var sourceDirectory = new DirectoryInfo(sourceFolder);
            var targetDirectory = new DirectoryInfo(targetFolder);

            Directory.CreateDirectory(targetDirectory.FullName);

            foreach (FileInfo fi in sourceDirectory.GetFiles())
            {
                if (fi.FullName.EndsWith(fileFilter))
                {
                    Console.WriteLine(@"Copying {0}\{1}", targetDirectory.FullName, fi.Name);
                    fi.CopyTo(Path.Combine(targetDirectory.FullName, fi.Name), true);
                }
            }
        }
    }
}