using Content.Server.Pinpointer;
using Content.Shared.Chat;
using Content.Goobstation.Shared.JanitorCallButton;

namespace Content.Goobstation.Server.JanitorCallButton;

public sealed partial class JanitorCallButtonSystem : EntitySystem
{
    [Dependency] private NavMapSystem _navMap = default!;
    [Dependency] private SharedChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<JanitorCallButtonComponent, JanitorCallButtonPressedEvent>(OnButtonPressed);
    }

    private void OnButtonPressed(Entity<JanitorCallButtonComponent> ent, ref JanitorCallButtonPressedEvent args)
    {
        var stationBeacon = _navMap.TryGetNearestBeacon(ent.Owner, out var beacon, out _)
            ? beacon.Value.Comp.Text!
            : "неизвестное место";

        _chat.TrySendInGameICMessage(
            ent.Owner,
            $":s Called janitor near {stationBeacon}",
            InGameICChatType.Speak,
            ChatTransmitRange.Normal);
    }
}
