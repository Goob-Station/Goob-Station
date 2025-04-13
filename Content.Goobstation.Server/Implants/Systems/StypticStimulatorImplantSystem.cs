// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
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
/// Really, this should just take the current multipliers and add five.
/// Fix this later dummy.
/// </summary>
public sealed class StypticStimulatorImplantSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private readonly Dictionary<EntityUid, FixedPoint2> _originalDamageCaps = new();
    private readonly Dictionary<EntityUid, Dictionary<string, FixedPoint2>> _originalDamageSpecifiers = new();
    private List<MobState> _originalAllowedStates = new();
    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromSeconds(1f);
    private TimeSpan _nextExecutionTime = TimeSpan.Zero;

    private static readonly string[] AffectedDamageTypes =
    {
        "Heat", "Cold", "Slash", "Blunt", "Piercing", "Poison", "Asphyxiation"
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StypticStimulatorImplantComponent, ImplantImplantedEvent>(OnImplant);
        SubscribeLocalEvent<StypticStimulatorImplantComponent, EntGotRemovedFromContainerMessage>(OnUnimplanted);
    }

    private void OnImplant(Entity<StypticStimulatorImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (!args.Implanted.HasValue)
            return;

        var user = args.Implanted.Value;
        var damageComp = EnsureComp<PassiveDamageComponent>(user);

        // Store original allowed states.
        foreach (var state in damageComp.AllowedStates)
            _originalAllowedStates.Add(state);

        // Store original damage cap if not already stored
        if (!_originalDamageCaps.ContainsKey(user))
            _originalDamageCaps[user] = damageComp.DamageCap;

        // Store original damage specifiers
        var originalSpecifiers = new Dictionary<string, FixedPoint2>();

        foreach (var damage in damageComp.Damage.DamageDict)
            originalSpecifiers[damage.Key] = damage.Value;

        _originalDamageSpecifiers[user] = originalSpecifiers;

        // Apply new damage modifications
        var damageDict = damageComp.Damage.DamageDict;
        damageDict.Clear();
        foreach (var damageType in AffectedDamageTypes)
        {
            damageDict[damageType] = -0.5;
        }
        damageComp.DamageCap = FixedPoint2.Zero;

        // Set new allowed states.
        damageComp.AllowedStates.Clear();
        damageComp.AllowedStates = [MobState.Alive, MobState.Critical];

        Dirty(user, damageComp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_nextExecutionTime > _gameTiming.CurTime)
            return;

        var query = EntityQueryEnumerator<StypticStimulatorImplantComponent>();
        while (query.MoveNext(out var comp))
        {
            if (!TryComp<BloodstreamComponent>(comp.Owner, out var bloodstreamComponent))
                return;

            _bloodstreamSystem.TryModifyBleedAmount(comp.Owner, -25f, bloodstreamComponent);
        }


        _nextExecutionTime = _gameTiming.CurTime + ExecutionInterval;

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
                foreach (var kvp in originalSpecifiers)
                {
                    damageComp.Damage.DamageDict[kvp.Key] = kvp.Value;
                }
                _originalDamageSpecifiers.Remove(implanted);
            }

            // Restore original allowed states.
            damageComp.AllowedStates.Clear();
            damageComp.AllowedStates = [MobState.Alive];
        }
    }
}
