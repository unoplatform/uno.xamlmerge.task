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
    public void When_HR_Enabled()
    {
        var task = CreateMerger(isHotReloadEnabled: true, isMainAssembly: false);

        Assert.IsTrue(task.Execute());

        ValidateOutput(task);
    }

    [TestMethod]
    public void When_HR_Enabled_Main_App()
    {
        var task = CreateMerger(isHotReloadEnabled: true, isMainAssembly: true);

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
    public void When_Different_Attributes_Different_Namespaces()
    {
        var task = CreateMerger();

        task.Execute();

        ValidateOutput(task);
    }

    [TestMethod]
    public void When_Different_Attributes_Different_Namespaces_Platform_Specific_first()
    {
        var task = CreateMerger();

        task.Execute();
        
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
    public void When_Duplicate_ThemeResources()
    {
        var task = CreateMerger();

        Assert.IsTrue(task.Execute());

        ValidateOutput(task);
    }

    [TestMethod]
    public void When_Duplicate_ThemeResources_With_Prefix()
    {
        var task = CreateMerger();

        Assert.IsTrue(task.Execute());

        ValidateOutput(task);
    }


    [TestMethod]
    public void When_Duplicate_ThemeResources_With_Ignored_Prefix()
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

    [TestMethod]
    public void When_Different_Namespace_Should_Be_Merged()
    {
        var task = CreateMerger();

        Assert.IsTrue(task.Execute());

        ValidateOutput(task);
    }

    [TestMethod]
    public void When_Different_Namespace_Should_Be_Merged_With_Members()
    {
        var task = CreateMerger();

        Assert.IsTrue(task.Execute());

        ValidateOutput(task);
    }

    [TestMethod]
    public void When_Different_Namespace_Should_Be_Merged_Multiple_Attributes()
    {
        var task = CreateMerger();

        Assert.IsTrue(task.Execute());

        ValidateOutput(task);
    }

#if false // Can be good for local testing against Uno repository.
    [TestMethod]
    public void TestUnoRepo()
    {
        BatchMergeXaml_v0 task = new();
        var repoRoot = @"C:\Users\PC\Desktop\uno\";
        task.Pages = new[]
        {
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\Breadcrumb\BreadcrumbBar.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\Breadcrumb\BreadcrumbBar_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\ColorPicker\ColorPicker.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\ColorPicker\ColorPicker_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\ColorPicker\ColorPicker_themeresources_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\ColorPicker\ColorPicker_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\ColorPicker\ColorSpectrum.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\Expander\Expander.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\Expander\Expander_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\InfoBadge\InfoBadge.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\InfoBadge\InfoBadge_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\InfoBar\InfoBar_themeresources_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\InfoBar\InfoBar_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\NavigationView\NavigationBackButton_rs1_themeresources_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\NavigationView\NavigationBackButton_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\NavigationView\NavigationView_rs1_themeresources_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\NavigationView\NavigationView_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\NumberBox\NumberBox.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\NumberBox\NumberBox_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\NumberBox\NumberBox_themeresources_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\NumberBox\NumberBox_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\PagerControl\PagerControl_themeresources_v2.5.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\PagerControl\PagerControl_v2.5.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\PipsPager\PipsPager.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\PipsPager\PipsPager_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\Primitives\CornerRadius_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\ProgressBar\ProgressBar.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\ProgressRing\ProgressRing.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\PullToRefresh\RefreshContainer\RefreshContainer.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\PullToRefresh\RefreshContainer\RefreshContainer_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\PullToRefresh\RefreshVisualizer\RefreshVisualizer.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\PullToRefresh\RefreshVisualizer\RefreshVisualizer_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\RadioButtons\RadioButton.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\RadioButtons\RadioButtons.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\RadioButtons\RadioButtons_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\RadioMenuFlyoutItem\RadioMenuFlyoutItem_rs1_themeresources_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\RadioMenuFlyoutItem\RadioMenuFlyoutItem_rs2_themeresources_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\RadioMenuFlyoutItem\RadioMenuFlyoutItem_rs4_themeresources_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\RadioMenuFlyoutItem\RadioMenuFlyoutItem_rs5_themeresources_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\RadioMenuFlyoutItem\RadioMenuFlyoutItem_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\RatingControl\RatingControl_themeresources_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\RatingControl\RatingControl_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\SplitButton\SplitButton.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\SplitButton\SplitButton_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\TabView\TabView_themeresources_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\TabView\TabView_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\TreeView\TreeView.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\TreeView\TreeViewItem.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\TreeView\TreeView_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\TwoPaneView\TwoPaneView_rs1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Controls\TwoPaneView\TwoPaneView_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Styles\AcrylicBrush_19h1_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Styles\AcrylicBrush_rs3_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\Microsoft\UI\Xaml\Styles\Common_themeresources_any.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\AppBar\AppBar.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\CalendarView\CalendarDatePicker_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\CalendarView\CalendarView_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\CommandBar\CommandBar.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\ContentDialog\ContentDialog.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\DatePicker\DatePicker_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\DatePicker\DateTimePickerFlyout_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\DropDownButton\DropDownButton.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\FlipView\FlipViewItem_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\FocusVisual\SystemFocusVisual.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\MenuBar\MenuBar.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\MenuBar\MenuBarItem.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\MenuBar\MenuBar_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\MenuFlyout\MenuFlyout_19h1_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\MenuFlyout\MenuFlyout_rs1_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\MenuFlyout\MenuFlyout_rs2_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\MenuFlyout\MenuFlyout_rs4_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\MenuFlyout\MenuFlyout_rs5_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\NavigationView\NavigationBackButton.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\NavigationView\NavigationBackButton_rs1_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\NavigationView\NavigationBackButton_rs4_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\NavigationView\NavigationBackButton_rs5_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\NavigationView\NavigationView.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\NavigationView\NavigationView_rs1_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\NavigationView\NavigationView_rs2_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\NavigationView\NavigationView_rs5_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\PersonPicture\PersonPicture.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\PersonPicture\PersonPicture_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\Primitives\LoopingSelector\LoopingSelector.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\ProgressRing\ProgressRing.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\ScrollBar\ScrollBar.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\ScrollViewer\ScrollViewer.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\SwipeControl\SwipeControl.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\SwipeControl\SwipeControl_rs1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\SwipeControl\SwipeControl_rs1_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\SwipeControl\SwipeControl_themeresources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\SwipeControl\SwipeControl_themeresources_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\SwipeControl\SwipeControl_v1.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Controls\ToolTip\ToolTip.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\DragDrop\DragView.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Style\Generic\FlyoutPresenter.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Style\Generic\Generic.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Style\Generic\SemanticStylesResources.xaml"),
            new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Style\mergedstyles.xaml"),
        };

        task.ProjectFullPath = $@"{repoRoot}src\Uno.UI\Uno.UI.Skia.csproj";
        task.MergedXamlFiles = new[] { new TaskItem($@"{repoRoot}src\Uno.UI\UI\Xaml\Style\mergedstyles.xaml") };
        task.Execute();
    }
#endif

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
            foreach (var file in task.MergedXamlFiles)
            {
                if (_expectedMetadataFilter.Match(file.ItemSpec) is { Success: true } result)
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

    private BatchMergeXaml_v0 CreateMerger(bool isHotReloadEnabled = false, bool isMainAssembly = false, [CallerMemberName] string testName = "")
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
            .Select(p => p.GetMetadata("MergeFile") is { Length: > 0 } m ? m : "merged.xaml")
            .Distinct()
            .Select(f => new TaskItem(Path.Combine(basePath, "Output", f)))
            .ToArray();
        task.IsHotReloadEnabled = isHotReloadEnabled;
        task.AssemblyName = "TestAssemblyName";
        task.OutputType = isMainAssembly ? "Exe" : "Library";
        return task;
    }

    private string? GetMergeFile(string f)
    {
        if (_inputMetadataFilter.Match(Path.GetFileName(f)) is { Success: true } result)
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
