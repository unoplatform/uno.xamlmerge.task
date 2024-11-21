# Uno.XamlMerge.Task

An MSBuild task which enables the merging of WinUI XAML resource dictionaries into a single flat `ResourceDictionary`.

This code is derived from WinUI's [`BatchMergeXaml`](https://github.com/microsoft/microsoft-ui-xaml/blob/a1ace7957abc19bce86141203560e15a737ccac7/tools/CustomTasks/BatchMergeXaml.cs) class, to be reused in any WinUI and Uno Platform Controls library.

## Why merge ResourceDictionary instances?

This task is generally used to allow for separate resource dictionary authoring (which makes them easier to load and read) to avoid impacting resources lookup runtime performance. In WinUI, `ResourceDictionary` resolution is performed through a graph traversal of MergedDictionaries, which generally implies that a worse case resolution can require as many lookups as there are dictionaries.

To limit the impact of the traversal, this task takes all resource dictionaries found in a specific MSBuild item group, and merges them into a single file. This file is then generally either named `Themes\Generic.xaml` or referenced as a merged dictionary from another `Themes\Generic.xaml` file.

## Using the task

1. Add a package reference:

    * If your project does not use [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/Central-Package-Management#enabling-central-package-management), in your `csproj` file, include the following block:

      ```xml
      <PropertyGroup>
        <_Uno_XamlMerge_Task_Version>1.0.0</_Uno_XamlMerge_Task_Version>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference Include="Uno.XamlMerge.Task" Version="$(_Uno_XamlMerge_Task_Version)" />
      </ItemGroup>
      ```

    * If your project is using [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/Central-Package-Management#enabling-central-package-management), add the following to your `Directory.Packages.props`:

      ```xml
      <PropertyGroup>
        <_Uno_XamlMerge_Task_Version>1.0.0</_Uno_XamlMerge_Task_Version>
      </PropertyGroup>
      <ItemGroup>
        <PackageVersion Include="Uno.XamlMerge.Task" Version="$(_Uno_XamlMerge_Task_Version)" />
      </ItemGroup>
      ```

      Then the following to your `.csproj`:

      ```xml
      <ItemGroup>
        <PackageVersion Include="Uno.XamlMerge.Task" Version="$(_Uno_XamlMerge_Task_Version)" />
      </ItemGroup>
      ```

1. Specify the resource dictionaries to be merged:

    ```xml
    <ItemGroup>
      <XamlMergeInput Include="Styles\**\*.xaml" Exclude="Styles\Generic.xaml" />
    </ItemGroup>
    ```

    > [!NOTE]
    > The `Exclude` portion is only needed if some of your dictionaries still need to be referenced explicitly as some libraries require valid full path, such as [Uno.Themes](https://github.com/unoplatform/uno.themes).

1. Add the following block at the end your project library or app:

    ```xml
    <Import Project="$(NuGetPackageRoot)uno.xamlmerge.task\$(_Uno_XamlMerge_Task_Version)\build\Uno.XamlMerge.Task.targets"
            Condition="'$(TargetFramework)' == '' and '$(TargetFrameworks)'!='' and exists('$(NuGetPackageRoot)\uno.xamlmerge.task\$(_Uno_XamlMerge_Task_Version)')" />
    ```

1. By default, the generated file is called `Generated\mergedpages.xaml`, but can be overridden as follows:

    ```xml
    <PropertyGroup>
      <XamlMergeOutputFile>Themes\Generic.xaml</XamlMergeOutputFile>
    </PropertyGroup>
    ```

1. The generated file can be referenced as follows:

    ```xml
    <ResourceDictionary
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="using:TestLibrary">
    
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="ms-appx:///REPLACE_ME/Generated/mergedpages.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
    ```

    Then replace `REPLACE_ME/` with either the library name if you're adding the package to a class library, or with nothing if you're adding it to your main project (e.g. `ms-appx:///Generated/mergedpages.xaml`).

## Multiple generated files

There are scenarios where placing groups of XAML files in separate merged files are useful, particularly, when IL Linking must be used.

In this case, the project file must be defined as follows:

```xml
<ItemGroup>
  <!-- Output files -->
  <XamlMergeOutputFiles Include="Generated\MergedPart1.xaml"  />
  <XamlMergeOutputFiles Include="Generated\MergedPart2.xaml"  />

  <!-- Input files, with the `MergeFile` metadata -->
  <XamlMergeInput Include="Styles\Part1\**\*.xaml" Exclude="Styles\Generic.xaml" MergeFile="MergedPart1.xaml" />
  <XamlMergeInput Include="Styles\Part2\**\*.xaml" Exclude="Styles\Generic.xaml" MergeFile="MergedPart2.xaml" />
</ItemGroup>
```

In this case, the `XamlMergeOutputFile` cannot be used, as the `XamlMergeOutputFiles` is defined.

## Further reading

- [Troubleshooting the Uno.XamlMerge.Task generation](troubleshooting.md)
