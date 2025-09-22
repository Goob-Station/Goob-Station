using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Humanoid;
using Content.Shared.Jittering;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Wraith.Systems;
public sealed partial class CurseOfDeath : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CurseOfDeathComponent, DeathCurseEvent>(OnCurse);
    }
    public void OnCurse(Entity<CurseOfDeathComponent> ent, ref DeathCurseEvent args)
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

        if (HasComp<CursedDeathComponent>(target))
        {
            _popup.PopupPredicted("wraith-already-cursed", uid, uid);
            return;
        }

        //TO DO: Give the cursed icon on the target for readibility for the wraith.
        var cursed = EnsureComp<CursedDeathComponent>(target);
        var jitter = EnsureComp<JitteringComponent>(target);

        args.Handled = true;
    }
}
