using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Server.Traits.Assorted;
using Content.Shared.Humanoid;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Wraith.Systems;
public sealed partial class CurseOfRot : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseOfRotComponent, RotCurseEvent>(OnCurse);
    }
    public void OnCurse(Entity<CurseOfRotComponent> ent, ref RotCurseEvent args)
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

        if (HasComp<CursedRotComponent>(target))
        {
            _popup.PopupPredicted("wraith-already-cursed", uid, uid);
            return;
        }

        //TO DO: Give the cursed icon on the target for readibility for the wraith.
        var cursed = EnsureComp<CursedRotComponent>(target);
        //var cursed2 = EnsureComp<HemophiliaComponent>(target); Hemophilia from EE is unstable. Waiting for hemophilia from upstream.

        args.Handled = true;
    }
}
