using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed partial class DecaySystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DecayComponent, DecayEvent>(OnDecay);
    }

    public void OnDecay(Entity<DecayComponent> ent, ref DecayEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;
        var target = args.Target;

        if (args.Handled)
            return;

        if (HasComp<HumanoidAppearanceComponent>(target))
        {
            _stamina.TakeOvertimeStaminaDamage(target, comp.StaminaDamageAmount);
            _popup.PopupPredicted(Loc.GetString("wraith-decay"), uid, uid);
            args.Handled = true;
            return;
        }

        // Try emagging anything that can be emagged
        var emagType = EmagType.Access;
        var emagEvent = new GotEmaggedEvent(uid, emagType);
        RaiseLocalEvent(target, ref emagEvent);

        if (emagEvent.Handled)
        {
            _popup.PopupPredicted(Loc.GetString("wraith-decay-emagged"), uid, uid);
            args.Handled = true;
            return;
        }

        _popup.PopupPredicted(Loc.GetString("wraith-decay-fail"), uid, uid);
        return;
    }
}
