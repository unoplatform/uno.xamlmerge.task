using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Uno.UI.Tasks.BatchMerge;

namespace Uno.XamlMerge.Tests;

[TestClass]
public class Given_GeneratePageListTask
{
    [TestMethod]
    public void When_Pages_Contains_MergedPages()
    {
        GeneratePageList_v0 task = new();

        task.Pages = new[]{
            new TaskItem("Generated\\Test1.xaml"),
            new TaskItem("Generated\\Test2.xaml"),
            new TaskItem("Generated\\Res1.xaml"),
            new TaskItem("Generated\\Res2.xaml"),
        };
        task.MergedXamlFiles = new[]{
            new TaskItem("Generated\\Test1.xaml"),
            new TaskItem("Generated\\Test2.xaml"),
        };

        task.Execute();

        Assert.IsNotNull(task.OutputPages);
        Assert.AreEqual(0, task.OutputPages.Length);
    }

    [TestMethod]
    public void When_Pages_DoesNot_Contain_MergedPages()
    {
        GeneratePageList_v0 task = new();

        task.Pages = new[]{
            new TaskItem("Generated\\Res1.xaml"),
            new TaskItem("Generated\\Res2.xaml"),
        };
        task.MergedXamlFiles = new[]{
            new TaskItem("Generated\\Test1.xaml"),
            new TaskItem("Generated\\Test2.xaml"),
        };

        task.Execute();

        Assert.IsNotNull(task.OutputPages);
        Assert.AreEqual(2, task.OutputPages.Length);
    }
}
