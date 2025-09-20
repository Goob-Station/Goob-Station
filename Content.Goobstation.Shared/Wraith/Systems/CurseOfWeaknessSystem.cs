using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Humanoid;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed partial class CurseOfWeakness : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseOfWeaknessComponent, WeakCurseEvent>(OnCurse);
    }
    public void OnCurse(Entity<CurseOfWeaknessComponent> ent, ref WeakCurseEvent args)
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

        if (HasComp<CursedWeakComponent>(target))
        {
            _popup.PopupPredicted("wraith-already-cursed", uid, uid);
            return;
        }

        //TO DO: Give the icon on the target for readibility for the wraith.
        var cursed = EnsureComp<CursedWeakComponent>(target);

        args.Handled = true;
    }
}
