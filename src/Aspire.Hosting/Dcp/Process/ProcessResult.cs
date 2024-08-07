// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Aspire.Hosting.Dcp.Process;

internal sealed class ProcessResult(int exitCode)
{
    public int ExitCode { get; } = exitCode;
}
