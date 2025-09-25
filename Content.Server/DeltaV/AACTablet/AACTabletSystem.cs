using Content.Server.Chat.Systems;
using Content.Shared.DeltaV.AACTablet;
using Content.Shared.DeltaV.QuickPhrase;
using Content.Shared.IdentityManagement;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Chat;


namespace Content.Server.DeltaV.AACTablet;

public sealed class AACTabletSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly ILocalizationManager _loc = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AACTabletComponent, AACTabletSendPhraseMessage>(OnSendPhrase);
    }

    private void OnSendPhrase(EntityUid uid, AACTabletComponent component, AACTabletSendPhraseMessage message)
    {
        if (component.NextPhrase > Timing.CurTime)
            return;

        // the AAC tablet uses the name of the person who pressed the tablet button
        // for quality of life
        var senderName = Identity.Entity(message.Actor, EntityManager);
        var speakerName = Loc.GetString("speech-name-relay",
            ("speaker", Name(uid)),
            ("originalName", senderName));

        if (!_prototypeManager.TryIndex<QuickPhrasePrototype>(message.PhraseID, out var phrase))
            return;

        _chat.TrySendInGameICMessage(uid,
            _loc.GetString(phrase.Text),
            InGameICChatType.Speak,
            hideChat: false,
            nameOverride: speakerName);

        var curTime = Timing.CurTime;
        component.NextPhrase = curTime + component.Cooldown;
    }
}
