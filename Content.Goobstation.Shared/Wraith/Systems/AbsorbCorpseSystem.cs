using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Goobstation.Shared.Wraith.WraithPoints;
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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AbsorbCorpseComponent, AbsorbCorpseEvent>(OnAbsorbTry);
        SubscribeLocalEvent<AbsorbCorpseComponent, AbsorbCorpseDoAfter>(OnAbsorbCorpseDoAfter);
    }

    private void OnAbsorbTry(Entity<AbsorbCorpseComponent> ent, ref AbsorbCorpseEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;
        var target = args.Target;

        if (args.Handled)
            return;

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            _popup.PopupPredicted(Loc.GetString("wraith-fail-target-not-humanoid"), uid, uid);
            return;
        }

        if (!_mobState.IsDead(target))
        {
            _popup.PopupPredicted(Loc.GetString("wraith-fail-target-alive"), uid, uid);
            return;
        }

        if (HasComp<WraithAbsorbableComponent>(target))
        {
            _popup.PopupPredicted(Loc.GetString("wraith-fail-target-absorbed"), uid, uid);
            return;
        }

        // TO DO: Add an extra check to verify if the target has at least 25u of formaldehyde
        var doAfter = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(comp.AbsorbDuration), new AbsorbCorpseDoAfter(), uid, target: args.Target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            DistanceThreshold = 2
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
        {
            // If it fails to start for any one reason.
            _popup.PopupPredicted(Loc.GetString("wraith-absorb-fail-start"), args.Target, args.Target);
            args.Handled = true;
        }

        _popup.PopupPredicted(Loc.GetString("wraith-absorb-start", ("target", args.Target)), uid, uid, PopupType.Medium);

        //TO DO: Make the wraith corporial during the do after.

        args.Handled = true;
    }

    private void OnAbsorbCorpseDoAfter(Entity<AbsorbCorpseComponent> ent, ref AbsorbCorpseDoAfter args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;
        var target = args.Target;

        if (args.Handled)
            return;

        if (target == null)
            return;

        //Rots the targetted corpse
        if (!HasComp<RottingComponent>(target.Value))
        {
            var rot = EntityManager.AddComponent<RottingComponent>(target!.Value);
        }
        if (!HasComp<WraithAbsorbableComponent>(target.Value))
        {
            var absorbable = EntityManager.AddComponent<WraithAbsorbableComponent>(target!.Value);
        }
        if (TryComp<TransformComponent>(target.Value, out var targetXform))


           PredictedSpawnAtPosition(comp.SmokeProto, targetXform.Coordinates);
        _audio.PlayPredicted(comp.AbsorbSound, uid, uid);

        //Lowers the cooldown for the next use.
        if (comp.CorpsesAbsorbed <= 3)
        {
            comp.AbsorbCooldown += comp.CooldownReducer;
        }
        // Increases WP regeneration
        _wraithPoints.AdjustWpGenerationRate(comp.WpPassiveAdd, uid);

        comp.CorpsesAbsorbed++;
        _popup.PopupPredicted(Loc.GetString("wraith-absorb-success"), uid, uid);
        args.Handled = true;
    }
}
