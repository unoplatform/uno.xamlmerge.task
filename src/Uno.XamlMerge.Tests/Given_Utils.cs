using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Uno.UI.Tasks.BatchMerge;

namespace Uno.XamlMerge.Tests;

[TestClass]
public class Given_Utils
{
    private string CreateScratchFolder([System.Runtime.CompilerServices.CallerMemberName] string name = "")
    {
        // GetFileName keeps the caller-member name a bare name, so it can never root the combine.
        var safeName = Path.GetFileName(name);
        var path = Path.Combine(Path.GetTempPath(), "Uno.XamlMerge.Tests", safeName + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    [TestMethod]
    public void When_File_Missing_Then_Written()
    {
        var folder = CreateScratchFolder();
        var target = Path.Combine(folder, "merged.xaml");

        Utils.RewriteFileIfNecessary(target, "hello");

        Assert.AreEqual("hello", File.ReadAllText(target));
    }

    [TestMethod]
    public void When_Content_Unchanged_Then_File_Not_Touched()
    {
        var folder = CreateScratchFolder();
        var target = Path.Combine(folder, "merged.xaml");

        Utils.RewriteFileIfNecessary(target, "hello");

        // A sentinel rather than the real write time: any rewrite moves the timestamp to "now",
        // which is detectable regardless of the file system's timestamp resolution.
        var sentinel = new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(target, sentinel);

        Utils.RewriteFileIfNecessary(target, "hello");

        Assert.AreEqual(sentinel, File.GetLastWriteTimeUtc(target), "An unchanged merge must not rewrite the shared output.");
    }

    [TestMethod]
    public void When_Written_Then_No_Temporary_Files_Left_Behind()
    {
        var folder = CreateScratchFolder();
        var target = Path.Combine(folder, "merged.xaml");

        Utils.RewriteFileIfNecessary(target, "one");
        Utils.RewriteFileIfNecessary(target, "two");

        Assert.AreEqual(1, Directory.GetFiles(folder).Length, "Only the merged output should remain in the folder.");
    }

    /// <summary>
    /// Several cross-targeting variants of the same project merge into one shared output file.
    /// A concurrent reader (the XAML source generator) must observe either the previous or the
    /// next content, never a partially written file.
    /// </summary>
    [TestMethod]
    public void When_Rewritten_While_Read_Then_Reader_Never_Sees_Partial_Content()
    {
        var folder = CreateScratchFolder();
        var target = Path.Combine(folder, "merged.xaml");

        // Large enough that a non-atomic write is observably partial.
        var contentA = new string('a', 4 * 1024 * 1024);
        var contentB = new string('b', 4 * 1024 * 1024);

        Utils.RewriteFileIfNecessary(target, contentA);

        using var stop = new CancellationTokenSource();

        // The reader returns its result rather than assigning a captured local, so awaiting the
        // task is what publishes it to this thread.
        var reader = System.Threading.Tasks.Task.Run(() =>
        {
            while (!stop.IsCancellationRequested)
            {
                string observed;
                try
                {
                    // Share like a source generator does, so the reader observes the file
                    // being rewritten instead of locking the writer out of it.
                    using var stream = new FileStream(
                        target,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite | FileShare.Delete);
                    using var text = new StreamReader(stream);
                    observed = text.ReadToEnd();
                }
                catch (IOException)
                {
                    // A transient sharing violation is acceptable; a torn read is not.
                    continue;
                }

                if (observed != contentA && observed != contentB)
                {
                    return $"length {observed.Length}, expected {contentA.Length}";
                }
            }

            return null;
        });

        for (var i = 0; i < 20; i++)
        {
            Utils.RewriteFileIfNecessary(target, i % 2 == 0 ? contentB : contentA);
        }

        stop.Cancel();

        Assert.IsTrue(reader.Wait(TimeSpan.FromSeconds(30)), "The reader did not stop within the timeout.");

        var partial = reader.Result;

        Assert.IsNull(partial, $"A concurrent reader observed a partially written file ({partial}).");
    }
}
