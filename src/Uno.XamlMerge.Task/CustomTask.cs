﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// Modified for Uno support by David John Oliver, Jerome Laban

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uno.UI.Tasks.BatchMerge
{
    public abstract class CustomTask : Microsoft.Build.Utilities.Task
    {
        internal protected bool HasLoggedErrors
        {
            get
            {
                if (BuildEngine != null)
                {
                    return Log.HasLoggedErrors;
                }
                else
                {
                    return hasLoggedErrors;
                }
            }
        }

        private bool hasLoggedErrors = false;

        internal protected void LogMessage(string message)
        {
            if (BuildEngine != null)
            {
                Log.LogMessage(message);
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        internal protected void LogWarning(string message)
        {
            LogWarning(string.Empty, string.Empty, string.Empty, string.Empty, -1, -1, message);
        }

        internal protected void LogWarning(string subcategory, string code, string helpKeyword, string file, int lineNumber, int columnNumber, string message)
        {
            if (BuildEngine != null)
            {
                Log.LogWarning(subcategory, code, helpKeyword, file, lineNumber, columnNumber, -1, -1, message);
            }
            else
            {
                Console.WriteLine($"Warning: {file} (Line {lineNumber}, column {columnNumber}: {message}");
            }
        }

        internal protected void LogError(string message)
        {
            LogError(string.Empty, string.Empty, string.Empty, string.Empty, -1, -1, message);
        }

        internal protected void LogError(string subcategory, string code, string helpKeyword, string file, int lineNumber, int columnNumber, string message)
        {
            if (BuildEngine != null)
            {
                Log.LogError(subcategory, code, helpKeyword, file, lineNumber, columnNumber, -1, -1, message);
            }
            else
            {
                Console.WriteLine($"Error: {file} (Line {lineNumber}, column {columnNumber}: {message}");
                hasLoggedErrors = true;
            }
        }
    }
}
