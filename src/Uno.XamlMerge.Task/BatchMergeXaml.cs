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
using System.Text;
using System.Xml;

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

        [Required]
        public string AssemblyName { get; set; }

        public bool IsHotReloadEnabled { get; set; }

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

            if (IsHotReloadEnabled)
            {
                GenerateForHotReload(filteredPages);
                return !HasLoggedErrors;
            }

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

        private string GenerateMergedDictionariesForHotReload(IEnumerable<ITaskItem> filteredPages)
        {
            var projectBasePath = Path.GetDirectoryName(Path.GetFullPath(ProjectFullPath));

            var builder = new StringBuilder();
            builder.Append("""
                        <!-- Generating a resource dictionary that references existing dictionaries for HotReload support -->
                        <!-- Proper merging of XAML files will happen if you build in Release configuration -->
                        <ResourceDictionary
                            xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                            xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006">
                            <ResourceDictionary.MergedDictionaries>

                        """);

            foreach (var page in filteredPages)
            {
                var pagePath = Path.GetFullPath(page.ItemSpec).Replace(projectBasePath, "").TrimStart(Path.DirectorySeparatorChar).Replace('\\', '/');
                builder.Append($"""
                                    <ResourceDictionary Source="ms-appx:///{AssemblyName}/{pagePath}" />

                            """);
            }

            builder.Append("""
                            </ResourceDictionary.MergedDictionaries>
                        </ResourceDictionary>

                        """);

            return builder.ToString();
        }

        /// <summary>
        /// When HotReload is enabled, we want modifications to the original XAML files to be reflected.
        /// This cannot be achieved when an actual merge happens.
        /// So, for HotReload, we'll generate a XAML file that only references the original XAML files and not do an actual merge.
        /// This way, it can work with HotReload perfectly.
        /// </summary>
        private void GenerateForHotReload(List<ITaskItem> filteredPages)
        {
            if (MergedXamlFiles.Length > 1)
            {
                foreach (var mergedXamlFile in MergedXamlFiles)
                {
                    var mergeFileName = Path.GetFileName(mergedXamlFile.ItemSpec);
                    string fileContents = GenerateMergedDictionariesForHotReload(filteredPages.Where(p => string.Equals(p.GetMetadata("MergeFile"), mergeFileName, StringComparison.OrdinalIgnoreCase)));

                    Directory.CreateDirectory(Path.GetDirectoryName(mergedXamlFile.ItemSpec));
                    Utils.RewriteFileIfNecessary(mergedXamlFile.ItemSpec, fileContents);
                }
            }
            else if (MergedXamlFiles.Length == 1)
            {
                // Single target file, without "MergeFile" attribution
                var mergedXamlFile = MergedXamlFiles[0];
                var mergeFileName = Path.GetFileName(mergedXamlFile.ItemSpec);
                string fileContents = GenerateMergedDictionariesForHotReload(filteredPages);

                Directory.CreateDirectory(Path.GetDirectoryName(mergedXamlFile.ItemSpec));
                Utils.RewriteFileIfNecessary(mergedXamlFile.ItemSpec, fileContents);
            }
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
            private static bool TryMergeHashUsings(string existingNamespaceString, string newNamespaceString, out string mergedNamespaceString)
            {
                if (existingNamespaceString.Equals(newNamespaceString, StringComparison.Ordinal))
                {
                    mergedNamespaceString = existingNamespaceString;
                    return true;
                }

                var (existingUri, existingUsings) = TryStripHashUsingToTheEnd(existingNamespaceString);
                var (newUri, newUsings) = TryStripHashUsingToTheEnd(newNamespaceString);
                if (!existingUri.Equals(newUri, StringComparison.Ordinal))
                {
                    mergedNamespaceString = null;
                    return false;
                }

                mergedNamespaceString = existingUri + "#using:" + string.Join(";", existingUsings.Concat(newUsings).Distinct());
                return true;

                static (string NamespaceUri, string[] Usings) TryStripHashUsingToTheEnd(string namespaceString)
                {
                    var indexOfHashUsing = namespaceString.IndexOf("#using:", StringComparison.Ordinal);
                    if (indexOfHashUsing == -1)
                    {
                        return (namespaceString, Array.Empty<string>());
                    }

                    return (namespaceString.Substring(0, indexOfHashUsing), namespaceString.Substring(indexOfHashUsing + "#using:".Length).Split(';'));
                }
            }

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

                var dictionary = new Dictionary<string, string>();
                var documents = new List<XmlDocument>();

                // This dictionary handles elements, e.g, <android:MyAndroid />, where the key is the XElement
                // and the value is the prefix (e.g, "android")
                var elementsToUpdate = new Dictionary<XmlElement, string>();

                // This dictionary handles attributes that are namespace declarations, e.g xmlns:android="..."
                // where the key is the XAttribute and the value is the namespace name (e.g, "android")
                var attributesToUpdate = new Dictionary<XmlAttribute, string>();

                // This dictionary handles attributes that are property prefixes, e.g, <MyElement android:MyProp="Value" />
                var propertyAttributesToUpdate = new Dictionary<XmlAttribute, string>();

                
                foreach (string page in pages)
                {
                    try
                    {
                        var document = new XmlDocument();
                        var pageContent = File.ReadAllText(page);
                        pageContent = Utils.EscapeAmpersand(pageContent);
                        document.LoadXml(pageContent);

                        string[] ignorables = Array.Empty<string>();
                        foreach (XmlNode node in document.SelectNodes("descendant::node()"))
                        {
                            if (node.ParentNode.NodeType == XmlNodeType.Document &&
                                node.Attributes is not null &&
                                node.Attributes["Ignorable", "http://schemas.openxmlformats.org/markup-compatibility/2006"] is { } ignorableAttribute)
                            {
                                ignorables = ignorableAttribute.Value.Split(' ');
                            }

                            if (node is XmlElement element)
                            {
                                var prefix = element.GetPrefixOfNamespace(element.NamespaceURI);
                                if (ignorables.Contains(prefix))
                                {
                                    elementsToUpdate.Add(element, prefix);
                                }

                                foreach (XmlAttribute att in element.Attributes)
                                {
                                    if (att.Name.StartsWith("xmlns:"))
                                    {
                                        string name = att.LocalName;
                                        if (ignorables.Contains(name))
                                        {
                                            attributesToUpdate.Add(att, name);
                                            if (dictionary.TryGetValue(name, out var existing))
                                            {
                                                if (TryMergeHashUsings(existing, att.Value, out var merged))
                                                {
                                                    dictionary[name] = merged;
                                                }
                                                else
                                                {
                                                    throw new Exception($"Cannot merge '{existing}' with '{att.Value}' for '{name}'");
                                                }
                                            }
                                            else
                                            {
                                                dictionary.Add(name, att.Value);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var attributePrefix = element.GetPrefixOfNamespace(att.NamespaceURI);
                                        if (ignorables.Contains(attributePrefix))
                                        {
                                            propertyAttributesToUpdate.Add(att, attributePrefix);
                                        }
                                    }
                                }
                            }
                        }

                        documents.Add(document);
                    }
                    catch (Exception)
                    {
                        owner.LogError($"Exception found when merging namespaces for page {page}!");
                        throw;
                    }
                }

                foreach (var attributeToUpdate in attributesToUpdate)
                {
                    if (dictionary.TryGetValue(attributeToUpdate.Value, out var merged))
                    {
                        attributeToUpdate.Key.Value = merged;
                    }
                }

                foreach (var propertyAttributeToUpdate in propertyAttributesToUpdate)
                {
                    if (dictionary.TryGetValue(propertyAttributeToUpdate.Value, out var merged))
                    {
                        var ownerElement = propertyAttributeToUpdate.Key.OwnerElement;
                        var newAttribute = ownerElement.OwnerDocument.CreateAttribute(propertyAttributeToUpdate.Key.Prefix, propertyAttributeToUpdate.Key.LocalName, merged);
                        newAttribute.Value = propertyAttributeToUpdate.Key.Value;
                        
                        var refNode = propertyAttributeToUpdate.Key;
                        
                        if (newAttribute.Name == refNode.Name || newAttribute.NamespaceURI == refNode.NamespaceURI)
                        {
                            // This is a workaround for a bug in net461's InsertAfter that crashes if refNode has the same name as newAttribute
                            // If refNode and newAttribute are duplicates, it doesn't matter whether we insert before or after,
                            // since refNode will be removed before the insertion anyway.
                            ownerElement.Attributes.InsertBefore(newNode: newAttribute, refNode);
                        }
                        else
                        {
                            ownerElement.Attributes.InsertAfter(newNode: newAttribute, refNode);
                        }

                        ownerElement.RemoveAttributeNode(refNode);
                    }
                }

                foreach (var elementToUpdate in elementsToUpdate)
                {
                    if (dictionary.TryGetValue(elementToUpdate.Value, out var merged))
                    {
                        var newElement = elementToUpdate.Key.OwnerDocument.CreateElement(elementToUpdate.Key.Prefix, elementToUpdate.Key.LocalName, merged);

                        foreach (XmlNode oldNode in elementToUpdate.Key.ChildNodes.Cast<XmlNode>().ToArray())
                        {
                            newElement.AppendChild(oldNode);
                        }
                        foreach (XmlAttribute oldAttribute in elementToUpdate.Key.Attributes.Cast<XmlAttribute>().ToArray())
                        {
                            newElement.Attributes.Append(oldAttribute);
                        }

                        elementToUpdate.Key.ParentNode.ReplaceChild(newElement, elementToUpdate.Key);
                    }
                }

                for (int i = 0; i < pages.Count; i++)
                {
                    var page = pages[i];
                    var document = documents[i];
                    try
                    {
                        mergedDictionary.MergeContent(
                            content: document.OuterXml,
                            filePath: Path.GetFullPath(page)
                                .Replace(projectBasePath, "")
                                .TrimStart(Path.DirectorySeparatorChar));
                    }
                    catch (Exception)
                    {
                        owner.LogError($"Exception found when merging page {page}!");
                        throw;
                    }
                }

                mergedDictionary.FinalizeXaml();

                Directory.CreateDirectory(Path.GetDirectoryName(mergedXamlFile));
                Utils.RewriteFileIfNecessary(mergedXamlFile, mergedDictionary.ToString());
            }
        }
    }
}
