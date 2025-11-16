using Content.Client.Alerts;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Changeling.Systems;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Changeling;

public sealed partial class ChangelingBiomassSystem : SharedChangelingBiomassSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingBiomassComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
    }

    private void OnUpdateAlert(Entity<ChangelingBiomassComponent> ent, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.ID != ent.Comp.AlertId)
            return;

        var alert = args.SpriteViewEnt;
        var normalized = Math.Round(ent.Comp.Biomass / ent.Comp.MaxBiomass * 16);
        normalized = Math.Clamp(normalized, 0, ent.Comp.MaxBiomass);

        _sprite.LayerSetRsiState((alert.Owner, alert.Comp), AlertVisualLayers.Base, $"{normalized}");
    }
}
