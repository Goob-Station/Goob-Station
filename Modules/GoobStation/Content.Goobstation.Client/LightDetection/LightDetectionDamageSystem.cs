using Content.Client.Alerts;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Goobstation.Shared.LightDetection.Components;
using Content.Goobstation.Shared.LightDetection.Systems;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.LightDetection;

public sealed class LightDetectionDamageSystem : SharedLightDetectionDamageSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LightDetectionDamageComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
    }

    private void OnUpdateAlert(Entity<LightDetectionDamageComponent> ent, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.ID != ent.Comp.AlertProto)
            return;

        var alert = args.SpriteViewEnt;
        var normalized = (int)( (ent.Comp.DetectionValue / ent.Comp.DetectionValueMax) * ent.Comp.AlertMaxSeverity);
        normalized = Math.Clamp(normalized, 0, ent.Comp.AlertMaxSeverity);

        _sprite.LayerSetRsiState((alert.Owner, alert.Comp), AlertVisualLayers.Base, $"{normalized}");
    }
}
