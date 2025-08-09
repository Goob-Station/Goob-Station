using System.Linq;
using Content.Shared.Body.Part;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared.Body.Events;
using Content.Shared.Body.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Rejuvenate;

namespace Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;

public partial class ConsciousnessSystem
{
    private void InitProcess()
    {
        SubscribeLocalEvent<ConsciousnessComponent, MobStateChangedEvent>(OnMobStateChanged);
        // To prevent people immediately falling down as rejuvenated
        SubscribeLocalEvent<ConsciousnessComponent, RejuvenateEvent>(OnRejuvenate, after: [typeof(SharedBodySystem)]);
        SubscribeLocalEvent<ConsciousnessRequiredComponent, BodyPartAddedEvent>(OnBodyPartAdded);
        SubscribeLocalEvent<ConsciousnessRequiredComponent, BodyPartRemovedEvent>(OnBodyPartRemoved);
        SubscribeLocalEvent<ConsciousnessRequiredComponent, OrganAddedToBodyEvent>(OnOrganAdded);
        SubscribeLocalEvent<ConsciousnessRequiredComponent, OrganRemovedFromBodyEvent>(OnOrganRemoved);
        SubscribeLocalEvent<ConsciousnessComponent, MapInitEvent>(OnConsciousnessMapInit);
    }

    private const string NerveSystemIdentifier = "nerveSystem";

    private void UpdatePassedOut(float frameTime)
    {
        var query = EntityQueryEnumerator<ConsciousnessComponent>();
        while (query.MoveNext(out var ent, out var consciousness))
        {
            if (consciousness.ForceDead
                || _timing.CurTime < consciousness.NextConsciousnessUpdate)
                continue;

            consciousness.NextConsciousnessUpdate = _timing.CurTime + consciousness.ConsciousnessUpdateTime;

            foreach (var modifier in consciousness.Modifiers.Where(modifier => modifier.Value.Time < _timing.CurTime))
                RemoveConsciousnessModifier(ent, modifier.Key.Item1, modifier.Key.Item2, consciousness);

            foreach (var multiplier in consciousness.Multipliers.Where(multiplier => multiplier.Value.Time < _timing.CurTime))
                RemoveConsciousnessMultiplier(ent, multiplier.Key.Item1, multiplier.Key.Item2, consciousness);

            if (consciousness.PassedOutTime < _timing.CurTime && consciousness.PassedOut)
            {
                consciousness.PassedOut = false;
                CheckConscious(ent, consciousness);
            }

            if (consciousness.ForceConsciousnessTime < _timing.CurTime && consciousness.ForceConscious)
            {
                consciousness.ForceConscious = false;
                CheckConscious(ent, consciousness);
            }
        }
    }

    private void OnMobStateChanged(EntityUid uid, ConsciousnessComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        AddConsciousnessModifier(uid, uid, -component.Cap, "DeathThreshold", ConsciousnessModType.Pain, consciousness: component);
        // To prevent people from suddenly resurrecting while being dead. whoops

        foreach (var multiplier in
                 component.Multipliers.Where(multiplier => multiplier.Value.Type != ConsciousnessModType.Pain))
            RemoveConsciousnessMultiplier(uid, multiplier.Key.Item1, multiplier.Key.Item2, component);

        foreach (var modifier in
                 component.Modifiers.Where(modifier => modifier.Value.Type != ConsciousnessModType.Pain))
            RemoveConsciousnessModifier(uid, modifier.Key.Item1, modifier.Key.Item2, component);
    }

    private void OnRejuvenate(EntityUid uid, ConsciousnessComponent component, RejuvenateEvent args)
    {
        foreach (var painModifier in component.NerveSystem.Comp.Modifiers)
            _pain.TryRemovePainModifier(component.NerveSystem.Owner, painModifier.Key.Item1, painModifier.Key.Item2, component.NerveSystem.Comp);

        foreach (var painMultiplier in component.NerveSystem.Comp.Multipliers)
            _pain.TryRemovePainMultiplier(component.NerveSystem.Owner, painMultiplier.Key, component.NerveSystem.Comp);

        foreach (var multiplier in
                 component.Multipliers.Where(multiplier => multiplier.Value.Type == ConsciousnessModType.Pain))
            RemoveConsciousnessMultiplier(uid, multiplier.Key.Item1, multiplier.Key.Item2, component);

        foreach (var modifier in
                 component.Modifiers.Where(modifier => modifier.Value.Type == ConsciousnessModType.Pain))
            RemoveConsciousnessModifier(uid, modifier.Key.Item1, modifier.Key.Item2, component);

        foreach (var nerve in component.NerveSystem.Comp.Nerves)
            foreach (var painFeelsModifier in nerve.Value.PainFeelingModifiers)
                _pain.TryRemovePainFeelsModifier(painFeelsModifier.Key.Item1, painFeelsModifier.Key.Item2, nerve.Key, nerve.Value);

        CheckRequiredParts(uid, component);
        ForceConscious(uid, TimeSpan.FromSeconds(1f), component);
    }

    private void OnConsciousnessMapInit(EntityUid uid, ConsciousnessComponent consciousness, MapInitEvent args)
    {
        if (consciousness.RawConsciousness < 0)
        {
            consciousness.RawConsciousness = consciousness.Cap;
            Dirty(uid, consciousness);
        }

        CheckConscious(uid, consciousness);
    }

    private void OnBodyPartAdded(EntityUid uid, ConsciousnessRequiredComponent component, ref BodyPartAddedEvent args)
    {
        if (!_timing.IsFirstTimePredicted
            || args.Part.Comp.Body == null
            || !TryComp<ConsciousnessComponent>(args.Part.Comp.Body, out var consciousness))
            return;

        if (consciousness.RequiredConsciousnessParts.TryGetValue(component.Identifier, out var value) && value.Item1 != null && value.Item1 != uid)
            Log.Warning($"ConsciousnessRequirementPart with duplicate Identifier {component.Identifier}:{uid} added to a body:" +
                        $" {args.Part.Comp.Body} this will result in unexpected behaviour!");

        consciousness.RequiredConsciousnessParts[component.Identifier] = (uid, component.CausesDeath, false);
        CheckRequiredParts(args.Part.Comp.Body.Value, consciousness);
    }

    private void OnBodyPartRemoved(EntityUid uid, ConsciousnessRequiredComponent component, ref BodyPartRemovedEvent args)
    {
        if (!_timing.IsFirstTimePredicted
            || args.Part.Comp.Body == null
            || !TryComp<ConsciousnessComponent>(args.Part.Comp.Body.Value, out var consciousness))
            return;

        if (!consciousness.RequiredConsciousnessParts.TryGetValue(component.Identifier, out var value))
        {
            Log.Warning($"ConsciousnessRequirementPart with identifier {component.Identifier}:{uid} not found on body:{args.Part.Comp.Body}");
            return;
        }

        consciousness.RequiredConsciousnessParts[component.Identifier] = (uid, value.Item2, true);
        CheckRequiredParts(args.Part.Comp.Body.Value, consciousness);
    }

    private void OnOrganAdded(EntityUid uid, ConsciousnessRequiredComponent component, ref OrganAddedToBodyEvent args)
    {
        if (!_timing.IsFirstTimePredicted
            || !TryComp<ConsciousnessComponent>(args.Body, out var consciousness))
            return;

        if (consciousness.RequiredConsciousnessParts.TryGetValue(component.Identifier, out var value) && value.Item1 != null && value.Item1 != uid)
            Log.Warning($"ConsciousnessRequirementPart with duplicate Identifier {component.Identifier}:{uid} added to a body:" +
                             $" {args.Body} this will result in unexpected behaviour! Old {component.Identifier} wielder: {value.Item1}");

        consciousness.RequiredConsciousnessParts[component.Identifier] = (uid, component.CausesDeath, false);

        if (component.Identifier == NerveSystemIdentifier)
            consciousness.NerveSystem = (uid, Comp<NerveSystemComponent>(uid));

        CheckRequiredParts(args.Body, consciousness);
    }

    private void OnOrganRemoved(EntityUid uid, ConsciousnessRequiredComponent component, ref OrganRemovedFromBodyEvent args)
    {
        if (!_timing.IsFirstTimePredicted
            || !TryComp<ConsciousnessComponent>(args.OldBody, out var consciousness))
            return;

        if (!consciousness.RequiredConsciousnessParts.TryGetValue(component.Identifier, out var value))
        {
            Log.Warning($"ConsciousnessRequirementPart with identifier {component.Identifier}:{uid} not found on body:{args.OldBody}");
            return;
        }

        consciousness.RequiredConsciousnessParts[component.Identifier] = (uid, value.Item2, true);
        CheckRequiredParts(args.OldBody, consciousness);
    }
}
