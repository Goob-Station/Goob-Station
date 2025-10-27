using Content.Client.Alerts;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Goobstation.Shared.Alert.Components;
using Content.Goobstation.Shared.Alert.Events;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Alert.EntitySystems;
public sealed class ValueRelatedAlertSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ValueRelatedAlertComponent, UpdateAlertSpriteEvent>(OnAlertSpriteUpdate);
    }

    private void OnAlertSpriteUpdate(Entity<ValueRelatedAlertComponent> alert, ref UpdateAlertSpriteEvent args)
    {
        var sprite = args.SpriteViewEnt;

        var ev = new GetValueRelatedAlertValuesEvent(args.Alert);
        RaiseLocalEvent(args.ViewerEnt, ref ev);

        if (!ev.Handled || ev.MaxValue == null || ev.MaxValue == 0 || ev.CurrentValue == null)
            return;

        var normalized = (int) (ev.CurrentValue / ev.MaxValue * alert.Comp.MaxSeverity);
        normalized = Math.Clamp(normalized, (int) ev.MinValue, alert.Comp.MaxSeverity);

        var rsiString = (string.IsNullOrEmpty(alert.Comp.IconPrefix) ? "" : $"{alert.Comp.IconPrefix}_") + $"{normalized}";

        _spriteSystem.LayerSetRsiState(sprite.Owner, AlertVisualLayers.Base, rsiString);
    }
}
