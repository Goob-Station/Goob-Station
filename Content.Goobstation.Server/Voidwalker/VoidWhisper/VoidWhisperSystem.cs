using Content.Goobstation.Server.Speech;
using Content.Server.Administration;
using Content.Shared.IdentityManagement;
using Content.Server.Popups;
using Content.Server.Prayer;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Voidwalker.VoidWhisper;

/// <summary>
/// This is just the demonic whisper system if it wasn't hardcoded as shit.
/// </summary>
public sealed class VoidWhisperSystem : EntitySystem
{
    [Dependency] private readonly QuickDialogSystem _quickDialog = null!;
    [Dependency] private readonly PrayerSystem _prayer = null!;
    [Dependency] private readonly PopupSystem _popup = null!;
    [Dependency] private readonly IdentitySystem _identity = null!;
    [Dependency] private readonly VoidAccentSystem _voidAccent = null!;

    private EntityQuery<ActorComponent> _actorQuery;

    public override void Initialize()
    {
        base.Initialize();

        _actorQuery = GetEntityQuery<ActorComponent>();
        SubscribeLocalEvent<VoidWhisperComponent, VoidWhisperEvent>(OnVoidWhisper);
    }

    private void OnVoidWhisper(Entity<VoidWhisperComponent> entity, ref VoidWhisperEvent args)
    {
        var target = args.Target;

        if (!_actorQuery.TryComp(entity.Owner, out var actor)
            || !_actorQuery.TryComp(target, out var actorTarget))
            return;

        _quickDialog.OpenDialog(actor.PlayerSession,
            Loc.GetString(entity.Comp.DialogueTitle),
            "Message",
            (string message) =>
        {
            if (entity.Comp.ApplyAccent)
                message = _voidAccent.ApplyLegallyDistinctVoidSpeechPattern(message);

            _prayer.SendSubtleMessage(actorTarget.PlayerSession,
                actor.PlayerSession,
                message,
                Loc.GetString(entity.Comp.PopupTitle));

            _popup.PopupEntity(Loc.GetString(entity.Comp.WhisperTitle,
                ("name", _identity.GetEntityIdentity(target)),
                ("message", FormattedMessage.EscapeText(message))),
                entity.Owner,
                entity.Owner);
        });
    }
}
