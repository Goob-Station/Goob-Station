using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared._Lavaland.Megafauna.Mercury.Events;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Analyze the highest damage type of a target, heal that damage, and then deal that same amount of damage as genetic damage.
/// If the highest damage type is genetic, does nothing except a pop-up.
/// </summary>
public sealed class ParadigmInflationSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ParadigmInflationComponent, ParadigmInflationActionEvent>(OnAnalyze);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ParadigmInflationComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.IsAnalyzing || !comp.Target.HasValue)
                continue;

            comp.Accumulator += frameTime;

            if (comp.Accumulator >= comp.AnalyzeTime)
            {
                comp.Accumulator = 0f;
                comp.IsAnalyzing = false;
                DoParadigm(uid, comp, comp.Target.Value);
                comp.Target = null;
            }
        }
    }

    private void OnAnalyze(EntityUid uid, ParadigmInflationComponent comp, ParadigmInflationActionEvent args)
    {
        if (args.Handled)
            return;

        if (comp.IsAnalyzing)
            return;

        if (_mobState.IsDead(args.Target))
            return;

        comp.IsAnalyzing = true;
        comp.Target = args.Target;

        // Spawn warning on top of target and parent it to them, literally entirely for visual flare and feedback
        comp.WarningEntity = PredictedSpawnAttachedTo(comp.WarningPrototype, Transform(args.Target).Coordinates);
        if (comp.WarningEntity.HasValue)
        {
            _transform.SetParent(comp.WarningEntity.Value, args.Target);
        }
        // I genuinely have no clue why, but this was the only way I could get the pop ups to show.
        // I tried making it PopUpPredicted. nothing. I tried PopUpEntity. Nothing. For some UNGODLY reason, only PopUpEntity inside a server check works.
        if (_net.IsServer)
        {
            _popup.PopupEntity(Loc.GetString("ort-paradigm-start"), args.Target, args.Target, PopupType.Medium);
        }
        _audio.PlayPredicted(comp.AnalyzeSound, uid, uid, AudioParams.Default.WithVolume(-5f));

        args.Handled = true;
    }

    private void DoParadigm(EntityUid uid, ParadigmInflationComponent comp, EntityUid target)
    {
        if (_mobState.IsDead(target))
            return;

        if (!TryComp<DamageableComponent>(target, out var damageable))
            return;

        // Check which damage type is the highest.
        var damageKey = (string?) null;
        var highestDamage = FixedPoint2.Zero;
        foreach (var (key, value) in damageable.Damage.DamageDict)
        {
            if (value > highestDamage)
            {
                damageKey = key;
                highestDamage = value;
            }
        }

        // Impressive, no damage at all? This far in?
        if (damageKey is null)
        {
            if (_net.IsServer) // refer to comment on first popup
            {
                _popup.PopupEntity(Loc.GetString("ort-paradigm-no-damage"), target, target, PopupType.Medium);
            }
            return;
        }

        // looking rough buddy, here's a handout
        var geneticGroup = _prototype.Index<DamageGroupPrototype>("Genetic");
        if (geneticGroup.DamageTypes.Contains(damageKey))
        {
            if (_net.IsServer) // refer to comment on first popup
            {
                _popup.PopupEntity(Loc.GetString("ort-paradigm-genetic-highest"), target, target, PopupType.Medium);
            }
            return;
        }

        // the carrot
        var healAmount = new DamageSpecifier();
        healAmount.DamageDict.Add(damageKey, -highestDamage);
        _damageable.TryChangeDamage(target, healAmount);

        // the stick
        var geneticDamage = new DamageSpecifier(_prototype.Index<DamageGroupPrototype>("Genetic"), highestDamage);
        _damageable.TryChangeDamage(target, geneticDamage);

        _audio.PlayPredicted(comp.ParadigmSound, uid, uid);

        if (_net.IsServer) // refer to comment on first popup
        {
            _popup.PopupEntity(Loc.GetString("ort-paradigm-finished"), target, target, PopupType.MediumCaution);
        }

    }
}
