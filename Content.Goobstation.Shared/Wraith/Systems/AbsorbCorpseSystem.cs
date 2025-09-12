using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Goobstation.Shared.Wraith.WraithPoints;
using Content.Shared.Atmos.Rotting;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed partial class AbsorbCorpseSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    private static readonly EntProtoId SmokeProto = "AdminInstantEffectSmoke10";


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
            _popup.PopupEntity(Loc.GetString("wraith-fail-target-not-humanoid"), uid);
            return;
        }

        if (!_mobState.IsDead(target))
        {
            _popup.PopupEntity(Loc.GetString("wraith-fail-target-alive"), uid);
            return;
        }

        if (HasComp<WraithAbsorbableComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("wraith-fail-target-absorbed"), uid);
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
            _popup.PopupEntity(Loc.GetString("wraith-absorb-fail-start"), args.Target);
            args.Handled = true;
        }

        _popup.PopupEntity(Loc.GetString("wraith-absorb-start", ("target", args.Target)), uid, uid, PopupType.Medium);

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

            Spawn(SmokeProto, targetXform.MapPosition);

        //Lowers the cooldown for the next use.
        if (comp.CorpsesAbsorbed <= 3)
        {
            comp.AbsorbCooldown += comp.CooldownReducer;
        }
        // Increases WP regeneration
        if (TryComp<WraithPointsComponent>(uid, out var wpComp))
        {
            wpComp.WpRegeneration += 1;
        }

        comp.CorpsesAbsorbed++;
        _popup.PopupEntity(Loc.GetString("wraith-absorb-success"), uid);
        args.Handled = true;
    }
}
