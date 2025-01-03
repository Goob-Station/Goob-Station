using Content.Client.Alerts;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Shared.Flockmind;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client.Flockmind;
    public sealed partial class FlockmindSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlockmindComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
    }
    private void OnUpdateAlert(Entity<FlockmindComponent> ent, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.ID != ent.Comp.ResourceAlert)
            return;

        var sprite = args.SpriteViewEnt.Comp;
        var resource = Math.Clamp(ent.Comp.Resource.Int(), 0, 9999);
        sprite.LayerSetState(FlockVisualLayers.Digit1, $"{(resource / 1000) % 10}");
        sprite.LayerSetState(FlockVisualLayers.Digit2, $"{(resource / 100) % 10}");
        sprite.LayerSetState(FlockVisualLayers.Digit3, $"{(resource / 10) % 10}");
        sprite.LayerSetState(FlockVisualLayers.Digit4, $"{resource % 10}");

    }

}
