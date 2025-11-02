using Content.Shared.DeviceLinking
;using Content.Shared.DeviceLinking.Events
;using Content.Shared.Item.ItemToggle.Components
;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Robust.Shared.Prototypes
;namespace Content.Goobstation.Shared.Events.Toggle

;public sealed class SignalTogglerSystem : EntitySystem
{
    // [Dependency] private readonly ItemTogglePointLightSystem _shitcode = default!;
    public static readonly ProtoId<SinkPortPrototype> TogglePort = "Toggle";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SignalTogglerComponent, SignalReceivedEvent>(OnLightToggledGoidaEvent);
    }

    private void OnLightToggledGoidaEvent(Entity<SignalTogglerComponent> ent, ref SignalReceivedEvent args)
    {
        if (args.Port != TogglePort)
            return;

        if (TryComp<ItemToggleComponent>(ent, out var comp))
        {
            var ev = new ItemToggledEvent();
            RaiseLocalEvent(ent, ref ev);
        }

        if (TryComp<ItemTogglePointLightComponent>(ent, out var compShitcode))
        {
            var ev = new ItemToggledEvent();
            RaiseLocalEvent(ent, ref ev);
        }
    }
}

// true goida.
