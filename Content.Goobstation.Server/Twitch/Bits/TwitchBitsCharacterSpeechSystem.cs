using Content.Goobstation.Common.CCVar;
using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Server.Twitch.Bits;

public sealed class TwitchBitsCharacterSpeechSystem : EntitySystem, ITwitchBitsAction
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly TwitchBitsSystem _twitchBits = default!;

    public string Id => "character-speech";
    public string DisplayName => "Make the Streamer Speak";
    public string DisplayDescription => "Make the streamer's character say your approved message.";
    public string Category => "Communication";
    public string Sku => "ss14-character-speech";
    public bool RequiresInput => true;
    public int? MaxInputLength => GetMaximumLength();
    public string? InputPlaceholder => "Enter what the character should say";

    public override void Initialize()
    {
        base.Initialize();
        _twitchBits.RegisterAction(this);
    }

    public TwitchBitsActionValidity IsCurrentlyValid(EntityUid target, TwitchBitsActionContext context)
    {
        if (!context.IsExecution)
            return TwitchBitsActionValidity.Valid;

        var message = NormalizeMessage(context.Input);
        if (string.IsNullOrEmpty(message))
            return TwitchBitsActionValidity.Invalid("Enter something for the streamer's character to say.");

        return message.Length <= GetMaximumLength()
            ? TwitchBitsActionValidity.Valid
            : TwitchBitsActionValidity.Invalid($"Character speech is limited to {GetMaximumLength()} characters.");
    }

    public bool Execute(EntityUid target, TwitchBitsActionContext context)
    {
        var message = NormalizeMessage(context.Input);
        if (string.IsNullOrEmpty(message) || message.Length > GetMaximumLength())
            return false;

        _chat.TrySendInGameICMessage(
            target,
            message,
            InGameICChatType.Speak,
            false,
            checkRadioPrefix: false,
            ignoreActionBlocker: true,
            forced: true);
        return true;
    }

    private int GetMaximumLength()
    {
        return Math.Clamp(_configuration.GetCVar(GoobCVars.TwitchBitsCharacterSpeechMaxLength), 1, 300);
    }

    private static string NormalizeMessage(string? input)
    {
        return string.IsNullOrWhiteSpace(input)
            ? string.Empty
            : string.Join(' ', input.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries));
    }
}
