using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Server.Traits.Assorted;
using Content.Shared.Humanoid;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Wraith.Systems;
public sealed partial class CurseOfBlood : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseOfBloodComponent, BloodCurseEvent>(OnCurse);
    }
    public void OnCurse(Entity<CurseOfBloodComponent> ent, ref BloodCurseEvent args)
    {
        var uid = ent.Owner;
        var target = args.Target;
        var comp = ent.Comp;

        if (args.Handled)
            return;

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            _popup.PopupPredicted("wraith-fail-target-not-valid", uid, uid);
            return;
        }

        if (HasComp<CursedBlindComponent>(target))
        {
            _popup.PopupPredicted("wraith-already-cursed", uid, uid);
            return;
        }

        //TO DO: Give the cursed icon on the target for readibility for the wraith.
        var cursed = EnsureComp<CursedBloodComponent>(target);
        var cursed2 = EnsureComp<HemophiliaComponent>(target);

        args.Handled = true;
    }
}
