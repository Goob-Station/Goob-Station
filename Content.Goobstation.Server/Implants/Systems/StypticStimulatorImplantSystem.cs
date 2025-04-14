// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Implants.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Implants;
using Content.Shared.Mobs;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Implants.Systems;

/// <summary>
/// Takes the entities current healing per second, uncaps it, and multiplies it a whole ton.
/// Deathsquad just got a WHOLE lot scarier.
/// </summary>
public sealed class StypticStimulatorImplantSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private readonly Dictionary<EntityUid, FixedPoint2> _originalDamageCaps = new();
    private readonly Dictionary<EntityUid, Dictionary<string, FixedPoint2>> _originalDamageSpecifiers = new();

    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromSeconds(1f);
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StypticStimulatorImplantComponent, ImplantImplantedEvent>(OnImplant);
        SubscribeLocalEvent<StypticStimulatorImplantComponent, EntGotRemovedFromContainerMessage>(OnUnimplanted);
    }

    // this whole fucking block hurts my head to think about
    // im so sorry
    private void OnImplant(Entity<StypticStimulatorImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (!args.Implanted.HasValue || TerminatingOrDeleted(args.Implanted.Value))
            return;

        var user = args.Implanted.Value;
        var damageComp = EnsureComp<PassiveDamageComponent>(user);

        // Store original allowed states.
        ent.Comp.OriginalAllowedMobStates?.Clear();
        foreach (var state in damageComp.AllowedStates)
            ent.Comp.OriginalAllowedMobStates?.Add(state);

        // Store original damage cap if not already stored
        if (!_originalDamageCaps.ContainsKey(user))
            _originalDamageCaps[user] = damageComp.DamageCap;

        // Store original damage specifiers
        var originalSpecifiers = new Dictionary<string, FixedPoint2>();

        foreach (var damage in damageComp.Damage.DamageDict)
            originalSpecifiers[damage.Key] = damage.Value;

        _originalDamageSpecifiers[user] = originalSpecifiers;

        // Get the new specifiers
        var damageDict = damageComp.Damage.DamageDict;

        var newSpecifiers = new Dictionary<string, FixedPoint2>();

        foreach (var damageType in damageDict)
            newSpecifiers[damageType.Key] = damageType.Value * 6;

        damageDict.Clear();

        damageComp.Damage.DamageDict = newSpecifiers;
        damageComp.DamageCap = FixedPoint2.Zero;

        // Set new allowed states.
        damageComp.AllowedStates.Clear();
        damageComp.AllowedStates = [MobState.Alive, MobState.Critical];

        damageComp.Interval = 0.20f;

        Dirty(user, damageComp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StypticStimulatorImplantComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.NextExecutionTime > _gameTiming.CurTime)
                continue;

            if (!TryComp<BloodstreamComponent>(uid, out var bloodstreamComponent))
                continue;

            _bloodstreamSystem.TryModifyBleedAmount(uid, comp.BleedingModifier, bloodstreamComponent);
            comp.NextExecutionTime = _gameTiming.CurTime + ExecutionInterval;
        }
    }

    private void OnUnimplanted(Entity<StypticStimulatorImplantComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (TerminatingOrDeleted(args.Container.Owner))
            return;

        var implanted = args.Container.Owner;
        if (TryComp<PassiveDamageComponent>(implanted, out var damageComp))
        {
            // Restore original damage cap
            if (_originalDamageCaps.TryGetValue(implanted, out var originalCap))
            {
                damageComp.DamageCap = originalCap;
                _originalDamageCaps.Remove(implanted);
            }

            // Restore original damage specifiers
            if (_originalDamageSpecifiers.TryGetValue(implanted, out var originalSpecifiers))
            {
                damageComp.Damage.DamageDict.Clear();

                foreach (var specifierPair in originalSpecifiers)
                    damageComp.Damage.DamageDict[specifierPair.Key] = specifierPair.Value;

                _originalDamageSpecifiers.Remove(implanted);
            }

            // Restore original allowed states.
            damageComp.AllowedStates.Clear();

            if (ent.Comp.OriginalAllowedMobStates != null)
                damageComp.AllowedStates = ent.Comp.OriginalAllowedMobStates;

            // blah blah
            damageComp.Interval = 1f;
        }
    }
}
