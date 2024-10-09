using Content.Client.Alerts;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared._Goobstation.Changeling.EntitySystems;

namespace Content.Client._Goobstation.Changeling;

public sealed partial class ChangelingSystem : SharedChangelingSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
        // MOVE TO ABILITIES SubscribeLocalEvent<ChangelingComponent, GetStatusIconsEvent>(GetChanglingIcon);
    }

    private void OnUpdateAlert(Entity<ChangelingComponent> changeling, ref UpdateAlertSpriteEvent args)
    {
        var stateNormalized = 0;
        var comp = changeling.Comp;

        // hardcoded because uhh umm i don't know. send help.
        switch (args.Alert.AlertKey.AlertType)
        {
            case "ChangelingChemicals":
                stateNormalized = (int) (comp.Chemicals / comp.MaxChemicals * 18);
                break;

            case "ChangelingBiomass":
                stateNormalized = (int) (comp.Biomass / comp.MaxBiomass * 16);
                break;
            default:
                return;
        }
        var sprite = args.SpriteViewEnt.Comp;
        sprite.LayerSetState(AlertVisualLayers.Base, $"{stateNormalized}");
    }

    /* MOVE TO ABILITIES private void GetChanglingIcon(Entity<ChangelingComponent> ent, ref GetStatusIconsEvent args)
     {
         if (HasComp<HivemindComponent>(ent) && _prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
             args.StatusIcons.Add(iconPrototype);
     }*/
}
