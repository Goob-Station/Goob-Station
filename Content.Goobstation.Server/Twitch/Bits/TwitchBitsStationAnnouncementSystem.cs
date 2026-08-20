using Content.Goobstation.Common.CCVar;
using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Server.Twitch.Bits;

public sealed class TwitchBitsStationAnnouncementSystem : EntitySystem, ITwitchBitsAction
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly TwitchBitsSystem _twitchBits = default!;

    public string Id => "station-announcement";
    public string DisplayName => "Station Announcement";
    public string DisplayDescription => "Broadcast your message to everyone on the station.";
    public CVarDef<string> Sku => GoobCVars.TwitchBitsStationAnnouncementSku;
    public bool RequiresInput => true;
    public int? MaxInputLength => GetMaximumLength();
    public string? InputPlaceholder => "Enter a station announcement";

    public override void Initialize()
    {
        base.Initialize();
        _twitchBits.RegisterAction(this);
    }

    public TwitchBitsActionValidity IsCurrentlyValid(EntityUid target, TwitchBitsActionContext context)
    {
        if (_station.GetOwningStation(target) == null)
            return TwitchBitsActionValidity.Invalid("The streamer's character is not currently on a station.");

        if (!context.IsExecution)
            return TwitchBitsActionValidity.Valid;

        var message = NormalizeMessage(context.Input);
        if (string.IsNullOrEmpty(message))
            return TwitchBitsActionValidity.Invalid("Enter a station announcement before purchasing this action.");

        if (message.Length > GetMaximumLength())
            return TwitchBitsActionValidity.Invalid($"Station announcements are limited to {GetMaximumLength()} characters.");

        return TwitchBitsActionValidity.Valid;
    }

    public bool Execute(EntityUid target, TwitchBitsActionContext context)
    {
        var message = NormalizeMessage(context.Input);
        if (string.IsNullOrEmpty(message) || message.Length > GetMaximumLength())
            return false;

        var twitchUser = context.TwitchUserName ?? "Twitch";
        _chat.DispatchStationAnnouncement(target, $"{message} -{twitchUser}", "Station Announcement");
        return true;
    }

    private int GetMaximumLength()
    {
        return Math.Clamp(
            _configuration.GetCVar(GoobCVars.TwitchBitsStationAnnouncementMaxLength),
            1,
            500);
    }

    private static string NormalizeMessage(string? input)
    {
        return string.IsNullOrWhiteSpace(input)
            ? string.Empty
            : string.Join(' ', input.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries));
    }
}
