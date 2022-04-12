using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Uno.UI.Tasks.BatchMerge;

namespace Uno.XamlMerge.Tests;

[TestClass]
public class Given_BatchMergeXaml
{
    static Regex _inputMetadataFilter = new Regex(@"Input_.*?\.(?<metadata>.*?)\.xml");
    static Regex _expectedMetadataFilter = new Regex(@"merged.*?\.(?<metadata>.*?)\.xaml");

    [TestMethod]
    public void When_Empty()
    {
        var task = CreateMerger();

        Assert.IsTrue(task.Execute());

        ValidateOutput(task);
    }

    [TestMethod]
    public void When_Key_TargeType_Conflict()
    {
        var task = CreateMerger();

        Assert.IsTrue(task.Execute());

        ValidateOutput(task);
    }

    [TestMethod]
    public void When_Duplicate_Keys_Different_Namespace_Single_Input()
    {
        var task = CreateMerger();

        Assert.IsTrue(task.Execute());

        ValidateOutput(task);
    }

    [TestMethod]
    public void When_Duplicate_Keys_Different_Namespace_Multiple_Input()
    {
        var task = CreateMerger();

        Assert.IsTrue(task.Execute());

        ValidateOutput(task);
    }

    [TestMethod]
    public void When_Duplicate_Keys_on_Theme_Resources_And_Comments_As_FirstNode()
    {
        var task = CreateMerger();

        Assert.IsTrue(task.Execute());

        ValidateOutput(task);
    }

    [TestMethod]
    public void When_Two_MergeFiles_Single_Input()
    {
        var task = CreateMerger();

        Assert.IsTrue(task.Execute());

        ValidateOutput(task);
    }

    [TestMethod]
    public void When_Three_MergeFiles_Single_Input()
    {
        var task = CreateMerger();

        Assert.IsTrue(task.Execute());

        ValidateOutput(task);
    }

    [TestMethod]
    public void When_Two_MergeFiles_Single_Input_With_Common_File()
    {
        var task = CreateMerger();

        Assert.IsTrue(task.Execute());

        ValidateOutput(task);
    }

    private void ValidateOutput(BatchMergeXaml_v0 task, [CallerMemberName] string testName = "")
    {
        var basePath = GetBasePath(testName);

        if (task.MergedXamlFiles.Length == 1)
        {
            Assert.AreEqual(
                File.ReadAllText(Path.Combine(basePath, "expected.xml")).Replace("\t", "  ").Trim(),
                File.ReadAllText(task.MergedXamlFiles[0].ItemSpec).Replace("\t", "  ").Trim()
            );
        }
        else
        {
            foreach(var file in task.MergedXamlFiles)
            {
                if(_expectedMetadataFilter.Match(file.ItemSpec) is { Success: true } result)
                {
                    var assemblyName = result.Groups[1].ToString();

                    Assert.AreEqual(
                        File.ReadAllText(Path.Combine(basePath, $"expected.{assemblyName}.xml")).Replace("\t", "  ").Trim(),
                        File.ReadAllText(file.ItemSpec).Replace("\t", "  ").Trim()
                    );
                }
                else
                {
                    throw new InvalidOperationException($"Unable to find merge target in file name {file.ItemSpec}");
                }
            }
        }
    }

    private BatchMergeXaml_v0 CreateMerger([CallerMemberName] string testName = "")
    {
        var basePath = GetBasePath(testName);

        BatchMergeXaml_v0 task = new();

        task.Pages = Directory
            .GetFiles(basePath, "Input_*.xml")
            .OrderBy(f => f)
            .SelectMany(inputFile =>
            {
                var mergeFileMetadata = GetMergeFile(inputFile);

                if (mergeFileMetadata != null)
                {
                    var mergeFiles = mergeFileMetadata.Split(',');

                    return mergeFiles.Select(mergedFile =>
                    {
                        var metadata = new Dictionary<string, string>()
                        {
                            ["MergeFile"] = $"merged.{mergedFile}.xaml",
                        };

                        return new TaskItem(inputFile, metadata);
                    });
                }
                else
                {
                    return new[] { new TaskItem(inputFile) };
                }
            })
            .ToArray();

        task.ProjectFullPath = basePath;
        task.MergedXamlFiles = task.Pages
            .Select(p => p.GetMetadata("MergeFile") is { Length: >  0} m ? m : "merged.xaml" )
            .Distinct()
            .Select(f => new TaskItem(Path.Combine(basePath, "Output", f)))
            .ToArray();

        return task;
    }

    private string? GetMergeFile(string f)
    {
        if(_inputMetadataFilter.Match(Path.GetFileName(f)) is { Success: true } result)
        {
            return result.Groups[1].Value;
        }

        return null;
    }

    private string GetBasePath(string name)
    {
        return Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location)!, "Scenarios", name);
    }
}
