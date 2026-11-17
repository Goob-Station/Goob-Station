using Content.Server.Pinpointer;
using Content.Server.Radio.EntitySystems;
using Content.Goobstation.Shared.JanitorCallButton;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.JanitorCallButton;

public sealed partial class JanitorCallButtonSystem : EntitySystem
{
    [Dependency] private NavMapSystem _navMap = default!;
    [Dependency] private RadioSystem _radio = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<JanitorCallButtonComponent, JanitorCallButtonPressedEvent>(OnButtonPressed);
    }

    private void OnButtonPressed(Entity<JanitorCallButtonComponent> ent, ref JanitorCallButtonPressedEvent args)
    {
        if (ent.Comp.enabled)
        {
            var stationBeacon = _navMap.TryGetNearestBeacon(ent.Owner, out var beacon, out _)
                ? beacon.Value.Comp.Text!
                : Loc.GetString("janitor-call-button-unknown-location");

            var message = Loc.GetString("janitor-call-button-message", ("location", stationBeacon));
            _radio.SendRadioMessage(
                ent.Owner,
                message,
                _prototypeManager.Index<RadioChannelPrototype>("Service"),
                ent.Owner);
        }
    }
}
