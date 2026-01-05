using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.Damage;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Surgery.Steps;

public sealed class SurgeryDamageUserSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SurgeryDamageUserComponent, SurgeryStepEvent>(OnSurgeryStep);
    }

    private void OnSurgeryStep(Entity<SurgeryDamageUserComponent> ent, ref SurgeryStepEvent args)
    {
        _damage.TryChangeDamage(args.User, ent.Comp.Damage);
        if (ent.Comp.Popup is {} popup)
        {
            var msg = Loc.GetString(popup, ("target", args.Body), ("part", args.Part));
            _popup.PopupPredicted(msg, args.Body, args.User, PopupType.SmallCaution);
        }
    }
}
