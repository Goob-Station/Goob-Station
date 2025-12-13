using Content.Client.Alerts;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Goobstation.Shared.Changeling.Components;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Changeling;

public sealed partial class ChangelingChemicalSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingChemicalComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
    }

    private void OnUpdateAlert(Entity<ChangelingChemicalComponent> ent, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.ID != ent.Comp.AlertId)
            return;

        var alert = args.SpriteViewEnt;
        var normalized = Math.Round(ent.Comp.Chemicals / ent.Comp.MaxChemicals * 18);
        normalized = Math.Clamp(normalized, 0, 18);

        _sprite.LayerSetRsiState((alert.Owner, alert.Comp), AlertVisualLayers.Base, $"{normalized}");
    }
}
