using Content.Shared.Interaction;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;

namespace Content.Goobstation.Shared.JanitorCallButton;

public sealed partial class SharedJanitorCallButtonSystem : EntitySystem
{
    [Dependency] private SlimPoweredLightSystem _light = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<JanitorCallButtonComponent> ent, ref MapInitEvent args)
    {
        if (TryComp<SlimPoweredLightComponent>(ent.Owner, out var light))
            SetEnabled(ent, light.Enabled);
    }

    [SubscribeLocalEvent]
    private void OnInteractHand(Entity<JanitorCallButtonComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<SlimPoweredLightComponent>(ent.Owner, out var light))
            return;

        var enabled = !light.Enabled;

        _light.SetEnabled((ent.Owner, light), enabled);
        SetEnabled(ent, enabled);

        RaiseLocalEvent(ent.Owner, new JanitorCallButtonPressedEvent());
        args.Handled = true;
    }

    private void SetEnabled(Entity<JanitorCallButtonComponent> ent, bool enabled)
    {
        ent.Comp.Enabled = enabled;
        _appearance.SetData(
            ent.Owner,
            PoweredLightVisuals.BulbState,
            enabled ? PoweredLightState.On : PoweredLightState.Off);
    }
}
