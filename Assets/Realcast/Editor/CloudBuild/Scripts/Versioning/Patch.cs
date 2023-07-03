using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CloudBuild.Versioning
{
    /*
     * Parsing and editing git patch
     * Infos on how to parse and edit a git patch : http://joaquin.windmuller.ca/2011/11/16/selectively-select-changes-to-commit-with-git-or-imma-edit-your-hunk
     */
    public class Patch
    {
        public class Hunk
        {
            public int FromFileStartLine;
            public int FromFileNumberOfLines;
            public int ToFileStartLine;
            public int ToFileNumberOfLines;

            public List<string> Content = new List<string>();

            public void RevertChange(int changeIndex)
            {
                if (IsAddition(Content[changeIndex]))
                {
                    Content.RemoveAt(changeIndex);
                    --ToFileNumberOfLines;
                }
                else if (IsDeletion(Content[changeIndex]))
                {
                    Content[changeIndex] = ' ' + Content[changeIndex].Remove(0, 1);
                    ++ToFileNumberOfLines;
                }
            }

            public void RefreshHunkHeader()
            {
                string header = "@@ -" + FromFileStartLine + "," + FromFileNumberOfLines + " +" + ToFileStartLine + "," + ToFileNumberOfLines + " @@";
                int endheaderDelimiterIndex = Content[0].IndexOf("@@", 2);

                Content[0] = header + Content[0].Substring(endheaderDelimiterIndex + 2);
            }
        }

        public List<string> Header = new List<string>();
        public List<Hunk> Hunks = new List<Hunk>();

        public static Patch ParsePatch(string patchContent)
        {
            Patch patch = new Patch();
            bool readingPatchHeader = true;
            Hunk currentHunk = null;

            string[] lines = patchContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; ++i)
            {
                string currentLine = lines[i];

                if (IsHunkHeader(currentLine))
                {
                    readingPatchHeader = false;
                    currentHunk = ParseHunkHeader(currentLine);
                    patch.Hunks.Add(currentHunk);
                }
                else if (readingPatchHeader)
                    patch.Header.Add(currentLine);
                else
                    currentHunk.Content.Add(currentLine);
            }

            return patch;
        }

        public void WriteUpdatedPatch(string outputFile)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string line in Header)
                builder.AppendLine(line);

            foreach (Hunk hunk in Hunks)
            {
                foreach (string line in hunk.Content)
                    builder.AppendLine(line);
            }

            File.WriteAllText(outputFile, builder.ToString());
        }

        public static bool IsUnchanged(string line)
        {
            return line.StartsWith(" ");
        }

        public static bool IsAddition(string line)
        {
            return line.StartsWith("+");
        }

        public static bool IsDeletion(string line)
        {
            return line.StartsWith("-");
        }

        public static bool IsHunkHeader(string line)
        {
            return line.StartsWith("@@");
        }

        public static bool ContainsNoChanges(Hunk hunk)
        {
            for (int i = 1; i < hunk.Content.Count; ++i)
            {
                if (!IsUnchanged(hunk.Content[i]))
                    return false;
            }

            return true;
        }

        private static Hunk ParseHunkHeader(string line)
        {
            int fromFileSectionIndex = line.IndexOf("-");
            int toFileSectionIndex = line.IndexOf("+");
            int toFileSectionEndIndex = line.LastIndexOf("@@");

            string fromFileSectionContent = line.Substring(fromFileSectionIndex + 1, toFileSectionIndex - fromFileSectionIndex - 1);
            string toFileSectionContent = line.Substring(toFileSectionIndex + 1, toFileSectionEndIndex - toFileSectionIndex - 1);

            string[] fromFileSection = fromFileSectionContent.Split(',');
            string[] toFileSection = toFileSectionContent.Split(',');

            Hunk hunk = new Hunk
            {
                FromFileStartLine = int.Parse(fromFileSection[0]),
                FromFileNumberOfLines = int.Parse(fromFileSection[1]),
                ToFileStartLine = int.Parse(toFileSection[0]),
                ToFileNumberOfLines = int.Parse(toFileSection[1])
            };

            hunk.Content.Add(line);

            return hunk;
        }
    }
}

