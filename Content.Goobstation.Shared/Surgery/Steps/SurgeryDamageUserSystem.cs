using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Surgery.Steps;

public sealed partial class SurgeryDamageUserSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damage = default!;
    [Dependency] private SharedPopupSystem _popup = default!;

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
