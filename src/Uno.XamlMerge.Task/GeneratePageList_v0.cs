#nullable enable

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Uno.UI.Tasks.BatchMerge;

public class GeneratePageList_v0 : Microsoft.Build.Utilities.Task
{
    [Required]
    public ITaskItem[]? Pages { get; set; }

    [Required]
    public ITaskItem[]? MergedXamlFiles { get; set; }

    [Required]
    public string ProjectDirectory { get; set; } = "";

    [Output]
    public ITaskItem[]? OutputPages { get; set; }

    public override bool Execute()
    {
        OutputPages = MergedXamlFiles
            .Select(f => new TaskItem(XamlMerge.Task.Utilities.FileUtilities.MakeRelative(ProjectDirectory, f.ItemSpec)))
            .Except(Pages, FullPathComparer.Default)
            .Distinct(FullPathComparer.Default)
            .ToArray();

        return true;
    }
}
