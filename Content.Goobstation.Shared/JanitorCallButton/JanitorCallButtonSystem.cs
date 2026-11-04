using Content.Shared.Interaction;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;

namespace Content.Goobstation.Shared.JanitorCallButton;

public sealed partial class JanitorCallButtonSystem : EntitySystem
{
    [Dependency] private SlimPoweredLightSystem _light = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<JanitorCallButtonComponent, InteractHandEvent>(OnInteractHand);
    }

    private void OnInteractHand(Entity<JanitorCallButtonComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled || !TryComp<SlimPoweredLightComponent>(ent.Owner, out var light))
            return;

        _light.SetEnabled((ent.Owner, light), !light.Enabled);
        RaiseLocalEvent(ent.Owner, new JanitorCallButtonPressedEvent());
        args.Handled = true;
    }
}
