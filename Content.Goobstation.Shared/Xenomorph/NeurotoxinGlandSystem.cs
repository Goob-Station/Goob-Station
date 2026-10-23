using Content.Shared._White.Xenomorphs;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Goobstation.Shared.Xenomorph;

public sealed partial class NeurotoxinGlandSystem : EntitySystem
{
    [Dependency] private SharedBodySystem _body = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyComponent, ShotAttemptedEvent>(OnShotAttempted);
        SubscribeLocalEvent<NeurotoxinGlandComponent, ToggleAcidSpitEvent>(OnToggleAcidSpit);
    }

    private void OnShotAttempted(Entity<BodyComponent> ent, ref ShotAttemptedEvent args)
    {
        if (args.Cancelled || args.Used.Owner != ent.Owner)
            return;

        var glands = _body.GetBodyOrganEntityComps<NeurotoxinGlandComponent>((ent.Owner, ent.Comp));
        if (glands.Count == 0)
            return;

        foreach (var gland in glands)
            if (gland.Comp1.Active)
                return;

        args.Cancel();
    }

    private void OnToggleAcidSpit(Entity<NeurotoxinGlandComponent> ent, ref ToggleAcidSpitEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.Active = !ent.Comp.Active;
        Dirty(ent);
        _popup.PopupPredicted(Loc.GetString(ent.Comp.Active ? "neurotoxin-gland-activated" : "neurotoxin-gland-deactivated"), args.Performer, args.Performer);
        args.Handled = true;
    }
}
