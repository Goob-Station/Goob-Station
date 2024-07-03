using Content.Client.Alerts;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Shared.Changeling;

namespace Content.Client.Changeling;

public sealed partial class ChangelingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
    }

    private void OnUpdateAlert(EntityUid uid, ChangelingComponent comp, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.AlertKey.AlertType != "Chemicals")
            return;

        var chemicalsNormalised = (int) (comp.Chemicals / comp.MaxChemicals * 16); // hardcoded because uhh umm
        var sprite = args.SpriteViewEnt.Comp;
        sprite.LayerSetState(AlertVisualLayers.Base, $"{chemicalsNormalised}");
    }
}
