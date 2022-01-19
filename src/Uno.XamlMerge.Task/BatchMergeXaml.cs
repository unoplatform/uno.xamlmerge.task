﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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

namespace Uno.UI.Tasks.BatchMerge
{
	public class BatchMergeXaml_v0 : CustomTask
	{
		[Required]
		public ITaskItem[] Pages { get; set; }

		[Required]
		public string MergedXamlFile { get; set; }

		[Required]
		public string ProjectFullPath { get; set; }

		[Required]
		public string TlogReadFilesOutputPath { get; set; }

		[Required]
		public string TlogWriteFilesOutputPath { get; set; }

		[Output]
		public string[] FilesWritten
		{
			get { return filesWritten.ToArray(); }
		}

		private List<string> filesWritten = new List<string>();

		public override bool Execute()
		{
			MergedDictionary mergedDictionary = MergedDictionary.CreateMergedDicionary();
			List<string> pages = new List<string>();

			if (Pages != null)
			{
				foreach (ITaskItem pageItem in Pages)
				{
					string page = pageItem.ItemSpec;
					if (File.Exists(page))
					{
						pages.Add(page);
					}
					else
					{
						LogError($"Can't find page {page}!");
					}
				}
			}

			if (HasLoggedErrors)
			{
				return false;
			}

			LogMessage($"Merging XAML files into {MergedXamlFile}...");

			var projectBasePath = Path.GetDirectoryName(Path.GetFullPath(ProjectFullPath));

			foreach (string page in pages)
			{
				try
				{
					mergedDictionary.MergeContent(
						content: File.ReadAllText(page),
						filePath: Path.GetFullPath(page)
							.Replace(projectBasePath, "")
							.TrimStart(Path.DirectorySeparatorChar));
				}
				catch (Exception)
				{
					LogError($"Exception found when merging page {page}!");
					throw;
				}
			}

			Directory.CreateDirectory(Path.GetDirectoryName(MergedXamlFile));
			Directory.CreateDirectory(Path.GetDirectoryName(TlogReadFilesOutputPath));
			Directory.CreateDirectory(Path.GetDirectoryName(TlogWriteFilesOutputPath));

			mergedDictionary.FinalizeXaml();
			filesWritten.Add(Utils.RewriteFileIfNecessary(MergedXamlFile, mergedDictionary.ToString()));

			File.WriteAllLines(TlogReadFilesOutputPath, Pages.Select(page => page.ItemSpec));
			File.WriteAllLines(TlogWriteFilesOutputPath, FilesWritten);

			return !HasLoggedErrors;
		}
	}
}
