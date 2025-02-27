using Content.Server.Atmos.Rotting;
using Content.Server.DoAfter;
using Content.Server.Nutrition.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Content.Shared.Medical;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.Medical.CPR;

public sealed class CPRSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly FoodSystem _foodSystem = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly RottingSystem _rottingSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CPRTrainingComponent, GetVerbsEvent<InnateVerb>>(AddCPRVerb);
        SubscribeLocalEvent<CPRTrainingComponent, CPRDoAfterEvent>(OnCPRDoAfter);
    }

    private void AddCPRVerb(Entity<CPRTrainingComponent> performer, ref GetVerbsEvent<InnateVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || !TryComp<MobStateComponent>(args.Target, out var targetState)
            || targetState.CurrentState == MobState.Alive)
            return;

        var target = args.Target;
        InnateVerb verb = new()
        {
            Act = () => { StartCPR(performer, target); },
            Text = Loc.GetString("cpr-verb"),
            Icon = new SpriteSpecifier.Rsi(new("Interface/Alerts/human_alive.rsi"), "health4"),
            Priority = 2
        };

        args.Verbs.Add(verb);
    }

    private void StartCPR(Entity<CPRTrainingComponent> performer, EntityUid target)
    {
        if (HasComp<RottingComponent>(target))
        {
            _popupSystem.PopupEntity(Loc.GetString("cpr-target-rotting", ("entity", target)), performer, performer);
            return;
        }

        if (_inventory.TryGetSlotEntity(target, "outerClothing", out var outer))
        {
            _popupSystem.PopupEntity(Loc.GetString("cpr-must-remove", ("clothing", outer)), performer, performer);
            return;
        }

        if (_foodSystem.IsMouthBlocked(performer, performer) || _foodSystem.IsMouthBlocked(target, performer))
            return;

        _popupSystem.PopupEntity(Loc.GetString("cpr-start-second-person", ("target", target)), target, performer);
        _popupSystem.PopupEntity(Loc.GetString("cpr-start-second-person-patient", ("user", performer)), target, target);

        var doAfterArgs = new DoAfterArgs(
            EntityManager, performer, performer.Comp.DoAfterDuration, new CPRDoAfterEvent(), performer, target,
            performer)
        {
            BreakOnMove = true,
            NeedHand = true,
            BlockDuplicate = true
        };

        _doAfterSystem.TryStartDoAfter(doAfterArgs);

        var playingStream = _audio.PlayPvs(performer.Comp.CPRSound, performer, AudioParams.Default.WithLoop(true));
        if (!playingStream.HasValue)
            return;

        performer.Comp.CPRPlayingStream = playingStream.Value.Entity;
    }

    private void OnCPRDoAfter(Entity<CPRTrainingComponent> performer, ref CPRDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !args.Target.HasValue)
        {
            performer.Comp.CPRPlayingStream = _audio.Stop(performer.Comp.CPRPlayingStream);
            return;
        }

        if (!performer.Comp.CPRHealing.Empty)
            _damageable.TryChangeDamage(args.Target, performer.Comp.CPRHealing, true, origin: performer);

        if (performer.Comp.RotReductionMultiplier > 0)
            _rottingSystem.ReduceAccumulator(
                (EntityUid)args.Target, performer.Comp.DoAfterDuration * performer.Comp.RotReductionMultiplier);

        if (_robustRandom.Prob(performer.Comp.ResuscitationChance)
            && _mobThreshold.TryGetThresholdForState((EntityUid)args.Target, MobState.Dead, out var threshold)
            && TryComp<DamageableComponent>(args.Target, out var damageableComponent)
            && TryComp<MobStateComponent>(args.Target, out var state)
            && damageableComponent.TotalDamage < threshold)
            _mobStateSystem.ChangeMobState(args.Target.Value, MobState.Critical, state, performer);

        var isAlive = _mobStateSystem.IsAlive(args.Target.Value);
        args.Repeat = !isAlive;
        if (isAlive)
            performer.Comp.CPRPlayingStream = _audio.Stop(performer.Comp.CPRPlayingStream);
    }
}
