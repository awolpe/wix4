// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolset.Core.Burn.CommandLine
{
    using System.Threading;
    using System.Threading.Tasks;
    using WixToolset.Extensibility.Services;

    internal abstract class BurnSubcommandBase
    {
        public abstract bool TryParseArgument(ICommandLineParser parser, string argument);

        public abstract Task<int> ExecuteAsync(CancellationToken cancellationToken);
    }
}
