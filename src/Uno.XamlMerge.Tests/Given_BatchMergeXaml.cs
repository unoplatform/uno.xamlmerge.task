using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Uno.UI.Tasks.BatchMerge;

namespace Uno.XamlMerge.Tests
{
    [TestClass]
    public class Given_BatchMergeXaml
    {
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

        private void ValidateOutput(BatchMergeXaml_v0 task, [CallerMemberName] string testName = "")
        {
            var basePath = GetBasePath(testName);

            Assert.AreEqual(
                File.ReadAllText(Path.Combine(basePath, "expected.xml")).Replace("\t", "  ").Trim(),
                File.ReadAllText(task.MergedXamlFile).Replace("\t", "  ").Trim()
            );
        }

        private BatchMergeXaml_v0 CreateMerger([CallerMemberName] string testName = "")
        {
            var basePath = GetBasePath(testName);

            BatchMergeXaml_v0 task = new();
            task.Pages = Directory.GetFiles(basePath, "Input_*.xml").Select(f => new TaskItem(f)).ToArray();
            task.ProjectFullPath = basePath;
            task.MergedXamlFile = Path.Combine(basePath, "Output", "merged.xaml");
            task.TlogReadFilesOutputPath = Path.Combine(basePath, "Output", "readlog.txt");
            task.TlogWriteFilesOutputPath = Path.Combine(basePath, "Output", "writelog.txt");

            return task;
        }

        private string GetBasePath(string name)
        {
            return Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location)!, "Scenarios", name);
        }
    }
}