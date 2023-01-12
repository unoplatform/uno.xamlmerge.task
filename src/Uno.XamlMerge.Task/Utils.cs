// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// Modified for Uno support by David John Oliver, Jerome Laban

#nullable disable

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace Uno.UI.Tasks.BatchMerge
{
    class Utils
    {
        //The XmlWriter can't handle &#xE0E5 unless we escape/unescape the ampersand
        public static string UnEscapeAmpersand(string s)
        {
            return s.Replace("&amp;", "&");
        }

        public static string EscapeAmpersand(string s)
        {
            return s.Replace("&", "&amp;");
        }

        public static string DocumentToString(Action<XmlWriter> action)
        {
            StringWriter sw = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, Encoding = Encoding.UTF8 };
            XmlWriter writer = XmlWriter.Create(sw, settings);
            action(writer);
            writer.Flush();
            return Utils.UnEscapeAmpersand(sw.ToString());
        }

        public static string RewriteFileIfNecessary(string path, string contents)
        {
            var sw = Stopwatch.StartNew();
            Exception lastException = null;

            while (sw.Elapsed < TimeSpan.FromSeconds(3))
            {
                try
                {
                    bool rewrite = true;
                    var fullPath = Path.GetFullPath(path);
                    try
                    {
                        string existingContents = File.ReadAllText(fullPath);
                        if (String.Equals(existingContents, contents))
                        {
                            rewrite = false;
                        }
                    }
                    catch
                    {
                    }

                    if (rewrite)
                    {
                        File.WriteAllText(fullPath, contents);
                    }

                    return fullPath;
                }
                catch(Exception e)
                {
                    lastException = e;

                    // Retry on any exception
                    Thread.Sleep(250);
                }
            }

            throw new InvalidOperationException($"Failed to write file {path}: {lastException}");
        }
    }
}
