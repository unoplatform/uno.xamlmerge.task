// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Extract from https://github.com/dotnet/msbuild/blob/9bcc06cbe19ae2482ab18eab90a82fd079b26897/src/Deprecated/Engine/Shared/FileUtilities.cs#L791

using System;
using System.Collections.Generic;
using System.Text;

namespace Uno.XamlMerge.Task.Utilities
{
    internal class FileUtilities
    {
        internal static string MakeRelative(string basePath, string path)
        {
            if (basePath.Length == 0)
            {
                return path;
            }

            Uri baseUri = new Uri(FileUtilities.EnsureTrailingSlash(basePath), UriKind.Absolute); // May throw UriFormatException

            Uri pathUri = CreateUriFromPath(path);

            if (!pathUri.IsAbsoluteUri)
            {
                // the path is already a relative url, we will just normalize it...
                pathUri = new Uri(baseUri, pathUri);
            }

            Uri relativeUri = baseUri.MakeRelativeUri(pathUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.IsAbsoluteUri ? relativeUri.LocalPath : relativeUri.ToString());

            string result = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            return result;
        }

        /// <summary>
        /// If the given path doesn't have a trailing slash then add one.
        /// </summary>
        /// <param name="fileSpec">The path to check.</param>
        /// <returns>A path with a slash.</returns>
        internal static string EnsureTrailingSlash(string fileSpec)
        {
            if (!EndsWithSlash(fileSpec))
            {
                fileSpec += Path.DirectorySeparatorChar;
            }

            return fileSpec;
        }

        /// <summary>
        /// Helper function to create an Uri object from path.
        /// </summary>
        /// <param name="path">path string</param>
        /// <returns>uri object</returns>
        private static Uri CreateUriFromPath(string path)
        {
            Uri pathUri;

            // Try absolute first, then fall back on relative, otherwise it
            // makes some absolute UNC paths like (\\foo\bar) relative ...
            if (!Uri.TryCreate(path, UriKind.Absolute, out pathUri))
            {
                pathUri = new Uri(path, UriKind.Relative);
            }

            return pathUri;
        }

        /// <summary>
        /// Indicates if the given file-spec ends with a slash.
        /// </summary>
        /// <owner>SumedhK</owner>
        /// <param name="fileSpec">The file spec.</param>
        /// <returns>true, if file-spec has trailing slash</returns>
        internal static bool EndsWithSlash(string fileSpec)
        {
            return (fileSpec.Length > 0)
                ? IsSlash(fileSpec[fileSpec.Length - 1])
                : false;
        }

        /// <summary>
        /// Indicates if the given character is a slash. 
        /// </summary>
        /// <owner>SumedhK</owner>
        /// <param name="c"></param>
        /// <returns>true, if slash</returns>
        internal static bool IsSlash(char c)
        {
            return (c == Path.DirectorySeparatorChar) || (c == Path.AltDirectorySeparatorChar);
        }
    }
}
