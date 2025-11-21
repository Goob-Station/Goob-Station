using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Emag.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed partial class DecaySystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly EmagSystem _emag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DecayComponent, DecayEvent>(OnDecay);
    }

    private void OnDecay(Entity<DecayComponent> ent, ref DecayEvent args)
    {
        if (HasComp<HumanoidAppearanceComponent>(args.Target))
        {
            _stamina.TakeOvertimeStaminaDamage(args.Target, ent.Comp.StaminaDamageAmount);
            _popup.PopupClient(Loc.GetString("wraith-decay-human-alert"), args.Target, args.Target);
            args.Handled = true;
            return;
        }

        if (_emag.TryEmagEffect(ent.Owner, ent.Owner, args.Target, ent.Comp.Emag))
        {
            args.Handled = true;
            return;
        }

        _popup.PopupClient(Loc.GetString("wraith-decay-nothing"), ent.Owner, ent.Owner);
    }
}
