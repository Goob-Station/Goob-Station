using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Humanoid;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed partial class CurseOfBlindness : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseOfBlindnessComponent, BlindCurseEvent>(OnCurse);
    }
    public void OnCurse(Entity<CurseOfBlindnessComponent> ent, ref BlindCurseEvent args)
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
        var cursed = EnsureComp<CursedBlindComponent>(target);

        args.Handled = true;
    }
}
