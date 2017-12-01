﻿// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace Example.Extension
{
    using System;
    using WixToolset.Extensibility;

    public class ExampleExtensionFactory : IExtensionFactory
    {
        public bool TryCreateExtension(Type extensionType, out object extension)
        {
            if (extensionType == typeof(IPreprocessorExtension))
            {
                extension = new ExamplePreprocessorExtension();
            }
            else if (extensionType == typeof(ICompilerExtension))
            {
                extension = new ExampleCompilerExtension();
            }
            else if (extensionType == typeof(IExtensionData))
            {
                extension = new ExampleExtensionData();
            }
            else
            {
                extension = null;
            }
            
            return extension != null;
        }
    }
}
