// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Aspire.Dashboard.Model;

namespace Aspire.ResourceService.Proto.V1;

partial class Resource
{
    /// <summary>
    /// Converts this gRPC message object to a view model for use in the dashboard UI.
    /// </summary>
    public ResourceViewModel ToViewModel()
    {
        return new()
        {
            Name = ValidateNotNull(Name),
            ResourceType = ValidateNotNull(ResourceType),
            DisplayName = ValidateNotNull(DisplayName),
            Uid = ValidateNotNull(Uid),
            CreationTimeStamp = ValidateNotNull(CreatedAt).ToDateTime(),
            Properties = Properties.ToFrozenDictionary(property => ValidateNotNull(property.Name), property => ValidateNotNull(property.Value), StringComparers.ResourcePropertyName),
            Environment = GetEnvironment(),
            Urls = GetUrls(),
            State = HasState ? State : null,
            KnownState = HasState ? Enum.TryParse(State, out KnownResourceState knownState) ? knownState : null : null,
            StateStyle = HasStateStyle ? StateStyle : null,
            Commands = GetCommands()
        };

        ImmutableArray<EnvironmentVariableViewModel> GetEnvironment() => Environment
                .Select(e => new EnvironmentVariableViewModel(e.Name, e.Value, e.IsFromSpec))
                .ToImmutableArray();

        ImmutableArray<UrlViewModel> GetUrls() =>
            // Filter out bad urls
            (from u in Urls
             let parsedUri = Uri.TryCreate(u.FullUrl, UriKind.Absolute, out var uri) ? uri : null
             where parsedUri != null
             select new UrlViewModel(u.Name, parsedUri, u.IsInternal))
                    .ToImmutableArray();

        ImmutableArray<CommandViewModel> GetCommands() => Commands
                .Select(c => new CommandViewModel(c.CommandType, c.DisplayName, c.ConfirmationMessage, c.Parameter))
                .ToImmutableArray();

        T ValidateNotNull<T>(T value, [CallerArgumentExpression(nameof(value))] string? expression = null) where T : class
        {
            if (value is null)
            {
                throw new InvalidOperationException($"Message field '{expression}' on resource with name '{Name}' cannot be null.");
            }

            return value;
        }
    }
}

partial class ResourceCommandResponse
{
    public ResourceCommandResponseViewModel ToViewModel() => new ResourceCommandResponseViewModel()
    {
        ErrorMessage = ErrorMessage,
        Kind = (Dashboard.Model.ResourceCommandResponseKind)Kind
    };
}
