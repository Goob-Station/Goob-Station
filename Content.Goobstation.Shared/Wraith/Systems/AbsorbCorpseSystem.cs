using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;

namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed partial class AbsorbCorpseSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AbsorbCorpseComponent, AbsorbCorpseEvent>(AbsorbCorpseAction);
    }

    private void AbsorbCorpseAction(Entity<AbsorbCorpseComponent> ent, ref AbsorbCorpseEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;

        if (args.Handled)
            return;

        if (args.Target == uid)
            return;

        if (HasComp<BorgChassisComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("wraith-fail-target-borg"), uid);
            return;
        }

        if (!_mobState.IsDead(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("wraith-fail-target-alive"), uid);
            return;
        }
    }
}
