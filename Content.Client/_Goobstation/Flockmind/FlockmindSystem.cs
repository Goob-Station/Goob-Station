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
        var resource = Math.Clamp((int)ent.Comp.Resource, 0, 9999);
        sprite.LayerSetState(FlockmindVisualLayers.Digit1, $"{(resource / 1000) % 10}");
        sprite.LayerSetState(FlockmindVisualLayers.Digit2, $"{(resource / 100) % 10}");
        sprite.LayerSetState(FlockmindVisualLayers.Digit3, $"{(resource / 10) % 10}");
        sprite.LayerSetState(FlockmindVisualLayers.Digit4, $"{resource % 10}");

    }

}
