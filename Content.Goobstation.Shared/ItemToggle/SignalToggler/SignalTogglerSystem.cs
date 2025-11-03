using Content.Shared.DeviceLinking
;using Content.Shared.DeviceLinking.Events
;using Content.Shared.Toggleable
;using Robust.Shared.Prototypes
;

namespace Content.Goobstation.Shared.ItemToggle.SignalToggler

;public sealed class SignalTogglerSystem : EntitySystem
{
    public static readonly ProtoId<SinkPortPrototype> TogglePort = "Toggle" // should probably be definable in the comp but who cares lol

    ;public override void Initialize()
    {
        base.Initialize()
        ;SubscribeLocalEvent<SignalTogglerComponent, SignalReceivedEvent>(OnSignalReceived)
        ;
    }

    private void OnSignalReceived(Entity<SignalTogglerComponent> ent, ref SignalReceivedEvent args)
    {
        if (args.Port == TogglePort)
            RaiseLocalEvent(ent, new ToggleActionEvent())
        ;
    }
}
