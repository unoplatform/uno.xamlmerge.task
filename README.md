# Uno.XamlMerge.Task
An msbuild task which enables the merging of WinUI XAML resource dictionaries into a single flat ResourceDictionary.

This code is derived from WinUI's [`BatchMergeXaml`](https://github.com/microsoft/microsoft-ui-xaml/blob/a1ace7957abc19bce86141203560e15a737ccac7/tools/CustomTasks/BatchMergeXaml.cs) class, to be reused in any WinUI and Uno Platform Controls library.

## Why merge ResourceDictionary instances ?
This task is generally used to allow for separate resource dictionary authoring (which makes them easier to load and read) to avoid impacting resources lookup runtime performance. In WinUI, ResourceDictionary resolution is performed through a graph traversal of MergedDictionaries, which generally implies that a worse case resolution can require as many lookups as there are dictionaries.

To limit the impact of the traversal, this task task takes all resource dictionaries found in a specific MSBuild item group, and merges them into a single file. This file is then generally either named `Themes\Generic.xaml` or referenced as a merged dictionary from another `Themes\Generic.xaml` file.

## Using the task
See [our documentation](doc/using-xamlmerge.md) for more details on how to use this task.