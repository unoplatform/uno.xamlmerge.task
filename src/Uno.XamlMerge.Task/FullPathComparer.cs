#nullable enable

using Microsoft.Build.Framework;

namespace Uno.UI.Tasks.BatchMerge;

internal class FullPathComparer : IEqualityComparer<ITaskItem>
{
    public static IEqualityComparer<ITaskItem> Default { get; } = new FullPathComparer();

    public bool Equals(ITaskItem x, ITaskItem y)
    {
        return x.GetMetadata("FullPath") == y.GetMetadata("Fullpath");
    }

    public int GetHashCode(ITaskItem obj)
    {
        return obj.GetHashCode();
    }
}
