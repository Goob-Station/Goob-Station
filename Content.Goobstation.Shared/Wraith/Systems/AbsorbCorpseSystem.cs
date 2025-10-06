using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Goobstation.Shared.Wraith.WraithPoints;
using Content.Shared.Actions;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed partial class AbsorbCorpseSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly WraithPointsSystem _wraithPoints = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedRottingSystem _rotting = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AbsorbCorpseComponent, AbsorbCorpseEvent>(OnAbsorb);
    }

    private void OnAbsorb(Entity<AbsorbCorpseComponent> ent, ref AbsorbCorpseEvent args)
    {
        var user = args.Performer;
        var target = args.Target;
        var comp = ent.Comp;

        if (!_mobState.IsDead(target))
        {
            _popup.PopupPredicted(Loc.GetString("wraith-absorb-living"), user, user);
            return;
        }

        // This checks for a specific chemical in their bloodstream, removes it and damages the wraith in the process.
        if (TryComp<BloodstreamComponent>(target, out var blood)
    && _solution.ResolveSolution(target, blood.ChemicalSolutionName, ref blood.ChemicalSolution, out var chemSolution))
        {
            //TPart 2 TO DO: Add formaldehyde, a chemical that prevents rotting of corpses.
            var formalProto = new ProtoId<ReagentPrototype>("Opporozidone"); //TO DO: Unhardcode this

            foreach (var (reagentId, qty) in chemSolution.Contents)
            {
                if (reagentId.Prototype == formalProto)
                {
                    if (qty >= comp.FormaldehydeThreshhold)
                    {
                        _solution.RemoveReagent(blood.ChemicalSolution.Value, reagentId, comp.ChemToRemove);

                        _damageable.TryChangeDamage(user, ent.Comp.Damage, ignoreResistances: true);
                        _popup.PopupClient(Loc.GetString("wraith-absorb-tainted"), user, user, PopupType.MediumCaution);
                    }
                    break;
                }
            }
        }

        var absorbable = CompOrNull<WraithAbsorbableComponent>(target);
        if (absorbable != null && absorbable.Absorbed)
        {
            args.Handled = true;
            return;
        }

        // Handle rotting state
        bool isRotten = _rotting.IsRotten(target);

        // Spawn visual/sound effects
        PredictedSpawnAtPosition(ent.Comp.SmokeProto, Transform(target).Coordinates); //Part 2 TO DO: Port nice smoke visuals from Goonstation instead of spawning this generic smoke.
        _audio.PlayPredicted(ent.Comp.AbsorbSound, ent.Owner, user);

        // Adjust Wraith Points — more if target is already rotten
        var wpGain = isRotten
            ? ent.Comp.WpPassiveAdd * ent.Comp.RottenBonusMultiplier
            : ent.Comp.WpPassiveAdd;

        _wraithPoints.AdjustWpGenerationRate(wpGain, ent.Owner);

        // Notify the user if bonus applies
        if (isRotten)
            _popup.PopupPredicted(Loc.GetString("wraith-absorb-rotbonus"), user, user);
        else // else just do the generic pop up for success.
        {
            _popup.PopupPredicted(Loc.GetString("wraith-absorb-draw2"), user, user);
        }

        // If not rotten yet, apply rot
        if (!isRotten)
            EnsureComp<RottingComponent>(target);

        //Pop-up for everyone
        _popup.PopupPredicted(Loc.GetString("wraith-absorb-smoke1"), target, target);
        ent.Comp.CorpsesAbsorbed++;
        Dirty(ent);

        // Mark as absorbed
        if (absorbable != null)
            absorbable.Absorbed = true;

        args.Handled = true;
    }
}
