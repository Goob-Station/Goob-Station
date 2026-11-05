using Content.Shared.Light;
using Content.Shared.Interaction;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;

namespace Content.Goobstation.Shared.JanitorCallButton;

public sealed partial class JanitorCallButtonSystem : EntitySystem
{
    [Dependency] private SlimPoweredLightSystem _light = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<JanitorCallButtonComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<JanitorCallButtonComponent, InteractHandEvent>(OnInteractHand);
    }

    private void OnMapInit(Entity<JanitorCallButtonComponent> ent, ref MapInitEvent args)
    {
        if (TryComp<SlimPoweredLightComponent>(ent.Owner, out var light))
            UpdateVisuals(ent.Owner, light.Enabled);
    }

    private void OnInteractHand(Entity<JanitorCallButtonComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled || !TryComp<SlimPoweredLightComponent>(ent.Owner, out var light))
            return;

        var enabled = !light.Enabled;
        _light.SetEnabled((ent.Owner, light), enabled);
        UpdateVisuals(ent.Owner, enabled);
        RaiseLocalEvent(ent.Owner, new JanitorCallButtonPressedEvent());
        args.Handled = true;
    }

    private void UpdateVisuals(EntityUid uid, bool enabled)
    {
        _appearance.SetData(uid, PoweredLightVisuals.BulbState, enabled ? PoweredLightState.On : PoweredLightState.Off);
    }
}
