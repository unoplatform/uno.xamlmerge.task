// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// Modified for Uno support by David John Oliver, Jerome Laban
#nullable disable

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Uno.UI.Tasks.BatchMerge
{
    public class BatchMergeXaml_v0 : CustomTask
    {
        private List<string> _filesWritten = new();

        [Required]
        public ITaskItem[] Pages { get; set; }

        [Required]
        public ITaskItem[] MergedXamlFiles { get; set; }

        [Required]
        public string ProjectFullPath { get; set; }

        [Output]
        public string[] FilesWritten
            => _filesWritten.ToArray();

        public override bool Execute()
        {
            ValidatePageMergeFileMetadata();

            if (HasLoggedErrors)
            {
                return false;
            }

            var filteredPages = Pages.ToList();
            filteredPages.RemoveAll(e => MergedXamlFiles.Any(m => FullPathComparer.Default.Equals(e, m)));

            if (MergedXamlFiles.Length > 1)
            {
                foreach (var mergedXamlFile in MergedXamlFiles)
                {
                    var mergeFileName = Path.GetFileName(mergedXamlFile.ItemSpec);

                    BatchMerger.Merge(this,
                          mergedXamlFile.ItemSpec,
                          ProjectFullPath,
                          filteredPages.Where(p => string.Equals(p.GetMetadata("MergeFile"), mergeFileName, StringComparison.OrdinalIgnoreCase)).ToArray());
                }
            }
            else if (MergedXamlFiles.Length == 1)
            {
                // Single target file, without "MergeFile" attribution

                BatchMerger.Merge(this,
                        MergedXamlFiles[0].ItemSpec,
                        ProjectFullPath,
                        filteredPages.ToArray());
            }

            return !HasLoggedErrors;
        }

        private void ValidatePageMergeFileMetadata()
        {
            if (MergedXamlFiles.Length > 1)
            {
                foreach (var page in Pages)
                {
                    if (string.IsNullOrEmpty(page.GetMetadata("MergeFile")))
                    {
                        LogError($"The page {page.ItemSpec} does not define a `MergeFile` metadata, when multiple `MergedXamlFiles` are specified.");
                    }
                }
            }
        }

        class BatchMerger
        {
            internal static void Merge(
                CustomTask owner,
                string mergedXamlFile,
                string projectFullPath,
                ITaskItem[] pageItems)
            {
                var mergedDictionary = MergedDictionary.CreateMergedDicionary();
                List<string> pages = new();

                if (pageItems != null)
                {
                    foreach (var pageItem in pageItems)
                    {
                        var page = pageItem.ItemSpec;

                        if (File.Exists(page))
                        {
                            pages.Add(page);
                        }
                        else
                        {
                            owner.LogError($"Can't find page {page}!");
                        }
                    }
                }

                if (owner.HasLoggedErrors)
                {
                    return;
                }

                owner.LogMessage($"Merging XAML files into {mergedXamlFile}...");

                var projectBasePath = Path.GetDirectoryName(Path.GetFullPath(projectFullPath));

                foreach (string page in pages)
                {
                    try
                    {
                        mergedDictionary.Prepare(
                            content: File.ReadAllText(page),
                            filePath: Path.GetFullPath(page)
                                .Replace(projectBasePath, "")
                                .TrimStart(Path.DirectorySeparatorChar));
                    }
                    catch (Exception)
                    {
                        owner.LogError($"Exception found when merging namespaces for page {page}!");
                        throw;
                    }
                }

                mergedDictionary.MergeContent();

                mergedDictionary.FinalizeXaml();

                Directory.CreateDirectory(Path.GetDirectoryName(mergedXamlFile));
                Utils.RewriteFileIfNecessary(mergedXamlFile, mergedDictionary.ToString());
            }
        }
    }
}
