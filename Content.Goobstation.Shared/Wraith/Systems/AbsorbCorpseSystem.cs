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
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AbsorbCorpseComponent, AbsorbCorpseEvent>(OnAbsorbTry);
        SubscribeLocalEvent<AbsorbCorpseComponent, AbsorbCorpseEvent>(OnAbsorbComplete);
    }

    private void OnAbsorbTry(Entity<AbsorbCorpseComponent> ent, ref AbsorbCorpseEvent args)
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

        //TO DO: Add an extra check to verify if the target has at least 25u of formaldehyde

        var doAfter = new DoAfterArgs(EntityManager, uid, comp.AbsorbDuration, new AbsorbCorpseEvent(), uid, target: args.Target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DistanceThreshold = 2
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
        {
            //If it fails to start for any one reason.
            _popup.PopupEntity(Loc.GetString("wraith-absorb-fail-start"), args.Target);
            args.Handled = true;
        }

        _popup.PopupEntity(Loc.GetString("wraith-absorb-start", ("target", args.Target)), uid, uid, PopupType.Medium);
        args.Handled = true;

    }
    private void OnAbsorbComplete(Entity<AbsorbCorpseComponent> ent, ref AbsorbCorpseEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;

        if (args.Cancelled || args.Handled)
            return;

        if (args.Args.Target == null)
            return;

        //Just to be sure the target is still dead.
        if (!_mobState.IsDead(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("wraith-absorb-fail-target-alive"), uid);
            return;
        }

        //TO DO: Transform target into a skeleton.
        //TO DO: Increase WP regeneration.
        //TO DO: Lessen the cooldown for next use of Absorb Corpse.

        comp.CorpsesAbsorbed++;
        _popup.PopupEntity(Loc.GetString("wraith-absorb-success"), uid);

        args.Handled = true;

    }

    private void Skeletonize(EntityUid corpse)
    {
        //TO DO: Logic for turning perrson into skeleton.
    }

    private void EmpowerWraith(EntityUid wraith, AbsorbCorpseComponent comp)
    {
        //TO DO: Logic for lowering cooldown and increaseing WP regeneration.

        //So that the cooldown does not get reduced too much.
        if (comp.CorpsesAbsorbed <= 3)
        {
            comp.AbsorbCooldown += comp.CooldownReducer;
        }
    }
}
