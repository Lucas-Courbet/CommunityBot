using System.Diagnostics.CodeAnalysis;

namespace CommunityBot.Application.Common.Results;

public abstract record Result
{
    [MemberNotNullWhen(false, nameof(Message))]
    public bool IsSuccess { get; protected init; }

    public string? Message { get; protected init; }
}