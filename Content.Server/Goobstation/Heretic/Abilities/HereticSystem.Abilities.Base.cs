using Content.Shared.Actions;
using Content.Shared.Heretic;
using Content.Shared.Store.Components;

namespace Content.Server.Heretic;

public sealed partial class HereticSystem : EntitySystem
{
    private void SubscribeAbilities()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticOpenStore>(OnStore);
        SubscribeLocalEvent<HereticComponent, EventHereticMansusGrasp>(OnMansusGrasp);
    }

    private bool TryUseAbility(EntityUid ent, HereticComponent comp, BaseActionEvent args)
    {
        if (args.Handled)
            return false;

        if (!TryComp<HereticActionComponent>(args.Action, out var actionComp))
            return false;

        // skip check
        if (comp.Ascended)
            return true;

        // TODO: check if any magic items are parented to our heretic
        if (actionComp.RequireMagicItem)
        {
            if (comp.CodexActive)
                return true;

            if (comp.CanCastSpells)
                return true;

            return false;
        }

        return true;
    }

    private void OnStore(Entity<HereticComponent> ent, ref EventHereticOpenStore args)
    {
        if (!TryComp<StoreComponent>(ent, out var store))
            return;

        _store.ToggleUi(ent, ent, store);
    }
    private void OnMansusGrasp(Entity<HereticComponent> ent, ref EventHereticMansusGrasp args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        if (ent.Comp.MansusGraspActive)
        {
            _popup.PopupEntity(Loc.GetString("heretic-ability-fail"), ent, ent);
            return;
        }

        var st = Spawn("TouchSpellMansus", Transform(ent).Coordinates);

        if (!_hands.TryForcePickupAnyHand(ent, st))
        {
            _popup.PopupEntity(Loc.GetString("heretic-ability-fail"), ent, ent);
            QueueDel(st);
            return;
        }

        ent.Comp.MansusGraspActive = true;
        args.Handled = true;
    }
}
