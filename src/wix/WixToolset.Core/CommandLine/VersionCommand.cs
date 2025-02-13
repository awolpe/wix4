// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Core.CommandLine
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using WixToolset.Extensibility.Data;
    using WixToolset.Extensibility.Services;

    internal class VersionCommand : ICommandLineCommand
    {
        public bool ShowHelp { get; set; }

        public bool ShowLogo { get; set; }

        public bool StopParsing => true;

        public Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            // $(GitBaseVersionMajor).$(GitBaseVersionMinor).$(GitBaseVersionPatch)$(GitSemVerDashLabel)+$(Commit)
            Console.WriteLine("{0}.{1}.{2}{3}+{4}", ThisAssembly.Git.BaseVersion.Major
                                                  , ThisAssembly.Git.BaseVersion.Minor
                                                  , ThisAssembly.Git.BaseVersion.Patch
                                                  , ThisAssembly.Git.SemVer.DashLabel
                                                  , ThisAssembly.Git.Commit);
            return Task.FromResult(0);
        }

        public bool TryParseArgument(ICommandLineParser parseHelper, string argument)
        {
            return true; // eat any arguments
        }
    }
}
