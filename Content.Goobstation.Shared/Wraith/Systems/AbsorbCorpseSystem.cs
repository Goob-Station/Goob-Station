using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Goobstation.Shared.Wraith.WraithPoints;
using Content.Shared.Actions;
using Content.Shared.Atmos.Rotting;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;


namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed partial class AbsorbCorpseSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly WraithPointsSystem _wraithPoints = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedRottingSystem _rotting = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AbsorbCorpseComponent, AbsorbCorpseEvent>(OnAbsorbTry);
        SubscribeLocalEvent<AbsorbCorpseComponent, AbsorbCorpseDoAfter>(OnAbsorbCorpseDoAfter);

        SubscribeLocalEvent<WraithAbsorbableComponent, AbsorbCorpseAttemptEvent>(OnAbsorbableAttempt);
    }

    private void OnAbsorbTry(Entity<AbsorbCorpseComponent> ent, ref AbsorbCorpseEvent args)
    {
        var attemptEv = new AbsorbCorpseAttemptEvent(args.Performer, args.Target);
        RaiseLocalEvent(args.Target, ref attemptEv);

        if (attemptEv.Cancelled || !attemptEv.Handled)
        {
            // todo: fail popup here
            return;
        }

        // TO DO: Add an extra check to verify if the target has at least 25u of formaldehyde (Formaldehyde does not exist.)
        var doAfter = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            ent.Comp.AbsorbDuration,
            new AbsorbCorpseDoAfter(),
            ent.Owner,
            target: args.Target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DistanceThreshold = 2
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
        {
            // If it fails to start for any one reason.
            _popup.PopupPredicted(Loc.GetString("wraith-absorb-fail-start"), ent.Owner, ent.Owner);
            return;
        }

        _popup.PopupPredicted(Loc.GetString("wraith-absorb-start", ("target", args.Target)), ent.Owner, ent.Owner, PopupType.Medium);
        args.Handled = true; // todo: do it through cooldown action component every time its handled (check wraith actions folder)
    }

    private void OnAbsorbCorpseDoAfter(Entity<AbsorbCorpseComponent> ent, ref AbsorbCorpseDoAfter args)
    {
        if (args.Handled || args.Cancelled || args.Target == null)
            return;

        //TO DO: Rotting or bloated corpses give way more WP
        EnsureComp<RottingComponent>(args.Target.Value);
        PredictedSpawnAtPosition(ent.Comp.SmokeProto, Transform(args.Target.Value).Coordinates);

        _audio.PlayPredicted(ent.Comp.AbsorbSound, ent.Owner, ent.Owner);
        _wraithPoints.AdjustWpGenerationRate(ent.Comp.WpPassiveAdd, ent.Owner);

        ent.Comp.CorpsesAbsorbed++;
        Dirty(ent);

        _popup.PopupPredicted(Loc.GetString("wraith-absorb-success"), ent.Owner, ent.Owner);
        args.Handled = true;
    }

    private void OnAbsorbableAttempt(Entity<WraithAbsorbableComponent> ent, ref AbsorbCorpseAttemptEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        // if user is already absorbed then do nothing
        if (ent.Comp.Absorbed)
        {
            args.Cancelled = true;
            return;
        }

        // if user is dead do nothing
        if (!_mobState.IsDead(args.Target))
        {
            _popup.PopupPredicted(Loc.GetString("wraith-absorb-living"), args.User, args.User);
            args.Cancelled = true;
            return;
        }

        if (_rotting.IsRotten(args.Target))
        {
            // todo: rotting popup here
            // Comment: Do we really need a pop-up? 
            args.Cancelled = true;
            return;
        }
        args.Handled = true;
    }
}
