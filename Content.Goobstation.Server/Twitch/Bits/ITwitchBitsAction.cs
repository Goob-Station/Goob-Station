using Robust.Shared.Configuration;

namespace Content.Goobstation.Server.Twitch.Bits;

public sealed record TwitchBitsActionContext(
    TwitchBitsTransaction? Transaction,
    string? Input);

public sealed record TwitchBitsActionValidity(bool IsValid, string? Reason)
{
    public static readonly TwitchBitsActionValidity Valid = new(true, null);

    public static TwitchBitsActionValidity Invalid(string reason)
    {
        return new TwitchBitsActionValidity(false, reason);
    }
}

public interface ITwitchBitsAction
{
    string Id { get; }

    string DisplayName { get; }

    string DisplayDescription { get; }

    CVarDef<string> Sku { get; }

    bool RequiresInput => false;

    int? MaxInputLength => null;

    string? InputPlaceholder => null;

    TwitchBitsActionValidity IsCurrentlyValid(EntityUid target, TwitchBitsActionContext context);

    bool Execute(EntityUid target, TwitchBitsActionContext context);
}
