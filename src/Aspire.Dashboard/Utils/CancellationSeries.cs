// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Aspire.Dashboard.Utils;

/// <summary>
/// Produces a series of <see cref="CancellationToken"/>s that are used to coordinate
/// cancellation of non-overlapping operations in a concurrent environment.
/// </summary>
/// <remarks>
/// The class produces a stream of cancellation tokens, where each represents the lifetime of an operation that
/// started when <see cref="NextAsync"/> was called. Each call of that method cancels the last token it issued, then returns a new one.
/// In that way, you can set up operations with cancellation and no overlap.
/// The effectiveness of this approach depends upon good support for cancellation in operations under that token.
/// It's important that any await can be promptly cancelled.
/// </remarks>
internal sealed class CancellationSeries
{
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Produces the next token, cancelling the one before.
    /// </summary>
    /// <remarks>
    /// The returned token represents the lifetime of an operation, in a concurrent environment where operations will not overlap.
    /// This method cancels token it returned the last time it was called. In that way, you can set up operations with cancellation and no overlap.
    /// The effectiveness of this approach depends upon good support for cancellation in operations under that token.
    /// It's important that any await can be promptly cancelled.
    /// </remarks>
    /// <returns>A cancellation token that manages the lifetime of a non-overlapping operation in a concurrent environment.</returns>
    public async Task<CancellationToken> NextAsync()
    {
        var nextCts = new CancellationTokenSource();

        // Obtain the token before exchange, as otherwise the CTS may be cancelled before
        // we request the Token, which will result in an ObjectDisposedException.
        // This way we would return a cancelled token, which is reasonable.
        var nextToken = nextCts.Token;

        await Next(nextCts).ConfigureAwait(false);

        return nextToken;
    }

    /// <summary>
    /// Cancels the current <see cref="CancellationToken"/> but doesn't create a new one.
    /// </summary>
    /// <remarks>
    /// Multiple calls to this method will be no-ops until <see cref="NextAsync"/> is called,
    /// at which point there's something to cancel.
    /// </remarks>
    /// <returns></returns>
    public Task ClearAsync() => Next(null);

    private async Task Next(CancellationTokenSource? next)
    {
        using var priorCts = Interlocked.Exchange(ref _cts, next);

        if (priorCts is not null)
        {
            await priorCts.CancelAsync().ConfigureAwait(false);
        }
    }
}
