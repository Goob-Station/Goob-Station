using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Components;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;

namespace Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;

public partial class ConsciousnessSystem
{

    #region PublicApi

    /// <summary>
    /// Gets a nerve system off a body, if has one.
    /// </summary>
    /// <param name="body">Target entity</param>
    /// <param name="consciousness">Consciousness component</param>
    public Entity<NerveSystemComponent>? GetNerveSystem(EntityUid body, ConsciousnessComponent? consciousness = null)
    {
        if (!Resolve(body, ref consciousness))
            return null;

        return consciousness.NerveSystem;
    }

    /// <summary>
    /// Gets a nerve system off a body, if has one.
    /// </summary>
    /// <param name="body">Target entity</param>
    /// <param name="nerveSys">The nerve system you wanted.</param>
    public bool TryGetNerveSystem(
        EntityUid body,
        [NotNullWhen(true)] out Entity<NerveSystemComponent>? nerveSys)
    {
        nerveSys = null;
        if (!TryComp<ConsciousnessComponent>(body, out var consciousness))
            return false;

        nerveSys = consciousness.NerveSystem;
        return true;
    }

    /// <summary>
    /// Checks to see if an entity should be made unconscious, this is called whenever any consciousness values are changed.
    /// Unless you are directly modifying a consciousness component (pls dont) you don't need to call this.
    /// </summary>
    /// <param name="target">Target entity</param>
    /// <param name="consciousness">ConsciousnessComponent</param>
    /// <param name="mobState">MobStateComponent</param>
    public bool CheckConscious(EntityUid target, ConsciousnessComponent? consciousness = null, MobStateComponent? mobState = null)
    {
        if (!Resolve(target, ref consciousness, ref mobState, false))
            return false;

        var shouldBeConscious =
            consciousness.Consciousness > consciousness.Threshold || consciousness is { ForceUnconscious: false, ForceConscious: true };

        if (shouldBeConscious != consciousness.IsConscious)
        {
            var ev = new ConsciousnessUpdatedEvent(shouldBeConscious);
            RaiseLocalEvent(target, ref ev);
        }

        SetConscious(target, shouldBeConscious, consciousness);
        UpdateMobState(target, consciousness, mobState);

        return shouldBeConscious;
    }

    /// <summary>
    /// Force passes out an entity with consciousness component.
    /// </summary>
    /// <param name="target">Target to pass out.</param>
    /// <param name="time">Time.</param>
    /// <param name="consciousness"><see cref="ConsciousnessComponent"/> of an entity.</param>
    public void ForcePassOut(EntityUid target, TimeSpan time, ConsciousnessComponent? consciousness = null)
    {
        if (!Resolve(target, ref consciousness))
            return;

        consciousness.PassedOut = true;
        consciousness.PassedOutTime = _timing.CurTime + time;

        CheckConscious(target, consciousness);
    }

    /// <summary>
    /// Forces the entity to stay alive even if on 0 Consciousness, unless induced injuries that cause direct death, like getting your brain blown out
    /// Overrides ForcePassout and all other factors, the only requirement is entity being able to live
    /// </summary>
    /// <param name="target">Target to pass out.</param>
    /// <param name="time">Time.</param>
    /// <param name="consciousness"><see cref="ConsciousnessComponent"/> of an entity.</param>
    public void ForceConscious(EntityUid target, TimeSpan time, ConsciousnessComponent? consciousness = null)
    {
        if (!Resolve(target, ref consciousness))
            return;

        consciousness.ForceConscious = true;
        consciousness.ForceConsciousnessTime = _timing.CurTime + time;

        CheckConscious(target, consciousness);
    }

    #endregion

    #region Private Implementation

    private void UpdateConsciousnessModifiers(EntityUid uid, ConsciousnessComponent? consciousness)
    {
        if (!Resolve(uid, ref consciousness))
            return;

        var totalDamage
            = consciousness.Modifiers.Aggregate(FixedPoint2.Zero,
                (current, modifier) => current + modifier.Value.Change * consciousness.Multiplier);

        consciousness.RawConsciousness = consciousness.Cap + totalDamage;

        CheckConscious(uid, consciousness);
        Dirty(uid, consciousness);
    }

    private void UpdateConsciousnessMultipliers(EntityUid uid, ConsciousnessComponent? consciousness)
    {
        if (!Resolve(uid, ref consciousness))
            return;

        if (consciousness.Multipliers.Count > 0)
            consciousness.Multiplier = consciousness.Multipliers.Aggregate(FixedPoint2.Zero,
                (current, multiplier) => current + multiplier.Value.Change) / consciousness.Multipliers.Count;
        else
            consciousness.Multiplier = 1.0; // Just in case i guess?

        UpdateConsciousnessModifiers(uid, consciousness);
    }

    /// <summary>
    /// Only used internally. Do not use this, instead use consciousness modifiers/multipliers!
    /// </summary>
    /// <param name="target">target entity</param>
    /// <param name="isConscious">should this entity be conscious</param>
    /// <param name="consciousness">consciousness component</param>
    private void SetConscious(EntityUid target, bool isConscious, ConsciousnessComponent? consciousness = null)
    {
        if (!Resolve(target, ref consciousness))
            return;

        consciousness.IsConscious = isConscious;
        Dirty(target, consciousness);
    }

    private void UpdateMobState(EntityUid target, ConsciousnessComponent? consciousness = null, MobStateComponent? mobState = null)
    {
        if (TerminatingOrDeleted(target) || !Resolve(target, ref consciousness, ref mobState) || _net.IsClient)
            return;


        /* Im not a big fan of how janky the UpdateMobState is for this, so its getting commented out for now.

        var newMobState = consciousness.IsConscious
            ? MobState.Alive
            : MobState.Critical;

        if (consciousness.PassedOut)
            newMobState = MobState.Critical;

        if (consciousness.ForceUnconscious)
            newMobState = MobState.Critical;

        if (consciousness.Consciousness <= 0 && !consciousness.ForceConscious)
            newMobState = MobState.Dead;

        if (consciousness.ForceDead)
            newMobState = MobState.Dead;

        _mobStateSystem.ChangeMobState(target, newMobState, mobState);*/
    }

    private void CheckRequiredParts(EntityUid bodyId, ConsciousnessComponent consciousness)
    {
        var alive = true;
        var conscious = true;

        foreach (var (identifier, (entity, forcesDeath, isLost)) in consciousness.RequiredConsciousnessParts)
        {
            if (entity == null || !isLost)
                continue;

            if (forcesDeath)
            {
                consciousness.ForceDead = true;
                Dirty(bodyId, consciousness);

                alive = false;
                break;
            }

            conscious = false;
        }

        if (alive)
        {
            consciousness.ForceDead = false;
            consciousness.ForceUnconscious = !conscious;

            Dirty(bodyId, consciousness);
        }

        CheckConscious(bodyId, consciousness);
    }

    #endregion

    #region Multipliers and Modifiers

    /// <summary>
    /// Get all consciousness multipliers present on an entity. Note: these are copies, do not try to edit the values
    /// </summary>
    /// <param name="target">target entity</param>
    /// <param name="consciousness">consciousness component</param>
    /// <returns>Enumerable of Modifiers</returns>
    public IEnumerable<((EntityUid, string), ConsciousnessModifier)> GetAllModifiers(EntityUid target,
        ConsciousnessComponent? consciousness = null)
    {
        if (!Resolve(target, ref consciousness))
            yield break;

        foreach (var (owner, modifier) in consciousness.Modifiers)
        {
            yield return (owner, modifier);
        }
    }

    /// <summary>
    /// Get all consciousness multipliers present on an entity. Note: these are copies, do not try to edit the values
    /// </summary>
    /// <param name="target">target entity</param>
    /// <param name="consciousness">consciousness component</param>
    /// <returns>Enumerable of Multipliers</returns>
    public IEnumerable<((EntityUid, string), ConsciousnessMultiplier)> GetAllMultipliers(EntityUid target,
        ConsciousnessComponent? consciousness = null)
    {
        if (!Resolve(target, ref consciousness))
            yield break;

        foreach (var (owner, multiplier) in consciousness.Multipliers)
        {
            yield return (owner, multiplier);
        }
    }

    /// <summary>
    /// Add a unique consciousness modifier. This value gets added to the raw consciousness value.
    /// The owner and type combo must be unique, if you are adding multiple values from a single owner and type, combine them into one modifier
    /// </summary>
    /// <param name="target">Target entity</param>
    /// <param name="modifierOwner">Owner of a modifier</param>
    /// <param name="modifier">Value of the modifier</param>
    /// <param name="consciousness">ConsciousnessComponent</param>
    /// <param name="identifier">Localized text name for the modifier (for debug/admins)</param>
    /// <param name="type">Modifier type, defaults to generic</param>
    /// <param name="time">Time spawn for which the consciousness modifier will exist</param>
    /// <returns>Successful</returns>
    public bool AddConsciousnessModifier(EntityUid target,
        EntityUid modifierOwner,
        FixedPoint2 modifier,
        string identifier = "Unspecified",
        ConsciousnessModType type = ConsciousnessModType.Generic,
        TimeSpan? time = null,
        ConsciousnessComponent? consciousness = null)
    {
        if (!Resolve(target, ref consciousness))
            return false;

        if (!consciousness.Modifiers.TryAdd((modifierOwner, identifier), new ConsciousnessModifier(modifier, _timing.CurTime + time, type)))
            return false;

        UpdateConsciousnessModifiers(target, consciousness);
        Dirty(target, consciousness);

        return true;
    }

    /// <summary>
    /// Get a copy of a consciousness modifier. This value gets added to the raw consciousness value.
    /// </summary>
    /// <param name="target">Target entity</param>
    /// <param name="modifierOwner">Owner of a modifier</param>
    /// <param name="modifier">copy of the found modifier, changes are NOT saved</param>
    /// <param name="identifier">Identifier of the requested modifier</param>
    /// <param name="consciousness">Consciousness component</param>
    /// <returns>Successful</returns>
    public bool TryGetConsciousnessModifier(EntityUid target,
        EntityUid modifierOwner,
        [NotNullWhen(true)] out ConsciousnessModifier? modifier,
        string identifier,
        ConsciousnessComponent? consciousness = null)
    {
        modifier = null;
        if (!Resolve(target, ref consciousness) ||
            !consciousness.Modifiers.TryGetValue((modifierOwner, identifier), out var rawModifier))
            return false;

        modifier = rawModifier;

        return true;
    }

    /// <summary>
    /// Remove a consciousness modifier. This value gets added to the raw consciousness value.
    /// </summary>
    /// <param name="target">Target entity</param>
    /// <param name="modifierOwner">Owner of a modifier</param>
    /// <param name="consciousness">Consciousness component</param>
    /// <param name="identifier">Identifier of the modifier to remove</param>
    /// <returns>Successful</returns>
    public bool RemoveConsciousnessModifier(EntityUid target,
        EntityUid modifierOwner,
        string identifier,
        ConsciousnessComponent? consciousness = null)
    {
        if (!Resolve(target, ref consciousness))
            return false;

        if (!consciousness.Modifiers.Remove((modifierOwner, identifier), out var foundModifier))
            return false;

        UpdateConsciousnessModifiers(target, consciousness);
        Dirty(target, consciousness);

        return true;
    }

    /// <summary>
    /// Edit a consciousness modifier. This value gets set to the raw consciousness value.
    /// </summary>
    /// <param name="target">Target entity</param>
    /// <param name="modifierOwner">Owner of a modifier</param>
    /// <param name="modifierChange">Value that is being added onto the modifier</param>
    /// <param name="consciousness">Consciousness component</param>
    /// <param name="identifier">The string identifier of this modifier.</param>
    /// <param name="type">Modifier type, defaults to generic</param>
    /// <param name="time">Time span for which the component will exist</param>
    /// <returns>Successful</returns>
    public bool SetConsciousnessModifier(EntityUid target,
        EntityUid modifierOwner,
        FixedPoint2 modifierChange,
        string identifier = "Unspecified",
        ConsciousnessModType type = ConsciousnessModType.Generic,
        TimeSpan? time = null,
        ConsciousnessComponent? consciousness = null)
    {
        if (!Resolve(target, ref consciousness))
            return false;

        var newModifier = new ConsciousnessModifier(Change: modifierChange, Time: _timing.CurTime + time, Type: type);
        consciousness.Modifiers[(modifierOwner, identifier)] = newModifier;

        UpdateConsciousnessModifiers(target, consciousness);
        Dirty(target, consciousness);

        return true;
    }

    /// <summary>
    /// Edit a consciousness modifier. This value gets added to the raw consciousness value.
    /// </summary>
    /// <param name="target">Target entity</param>
    /// <param name="modifierOwner">Owner of a modifier</param>
    /// <param name="modifierChange">Value that is being added onto the modifier</param>
    /// <param name="identifier">The string identifier of the modifier to change</param>
    /// <param name="time">Time span for which this modifier shall exist</param>
    /// <param name="consciousness">Consciousness component</param>
    /// <returns>Successful</returns>
    public bool EditConsciousnessModifier(EntityUid target,
        EntityUid modifierOwner,
        FixedPoint2 modifierChange,
        string identifier,
        TimeSpan? time = null,
        ConsciousnessComponent? consciousness = null)
    {
        if (!Resolve(target, ref consciousness) ||
            !consciousness.Modifiers.TryGetValue((modifierOwner, identifier), out var oldModifier))
            return false;

        var newModifier =
            oldModifier with {Change = oldModifier.Change + modifierChange, Time = _timing.CurTime + time ?? oldModifier.Time};

        consciousness.Modifiers[(modifierOwner, identifier)] = newModifier;

        UpdateConsciousnessModifiers(target, consciousness);
        Dirty(target, consciousness);

        return true;
    }

    /// <summary>
    /// Add a unique consciousness multiplier. This value gets added onto the multiplier used to calculate consciousness.
    /// The owner and type combo must be unique, if you are adding multiple values from a single owner and type, combine them into one multiplier
    /// </summary>
    /// <param name="target">Target entity</param>
    /// <param name="multiplierOwner">Owner of a multiplier</param>
    /// <param name="multiplier">Value of the multiplier</param>
    /// <param name="consciousness">ConsciousnessComponent</param>
    /// <param name="identifier">Localized text name for the multiplier (for debug/admins)</param>
    /// <param name="type">Multiplier type, defaults to generic</param>
    /// <param name="time">Time span for which this multiplier will exist</param>
    /// <returns>Successful</returns>
    public bool AddConsciousnessMultiplier(EntityUid target,
        EntityUid multiplierOwner,
        FixedPoint2 multiplier,
        string identifier = "Unspecified",
        ConsciousnessModType type = ConsciousnessModType.Generic,
        TimeSpan? time = null,
        ConsciousnessComponent? consciousness = null)
    {
        if (!Resolve(target, ref consciousness))
            return false;

        if (!consciousness.Multipliers.TryAdd((multiplierOwner, identifier), new ConsciousnessMultiplier(multiplier, _timing.CurTime + time ?? time, type)))
            return false;

        UpdateConsciousnessMultipliers(target, consciousness);
        Dirty(target, consciousness);

        return true;
    }

    /// <summary>
    /// Get a copy of a consciousness multiplier. This value gets added onto the multiplier used to calculate consciousness.
    /// </summary>
    /// <param name="target">Target entity</param>
    /// <param name="multiplierOwner">Owner of a multiplier</param>
    /// <param name="identifier">String identifier of the multiplier to get</param>
    /// <param name="multiplier">Copy of the found multiplier, changes are NOT saved</param>
    /// <param name="consciousness">Consciousness component</param>
    /// <returns>Successful</returns>
    public bool TryGetConsciousnessMultiplier(EntityUid target,
        EntityUid multiplierOwner,
        string identifier,
        out ConsciousnessMultiplier? multiplier,
        ConsciousnessComponent? consciousness = null)
    {
        multiplier = null;
        if (!Resolve(target, ref consciousness) ||
            !consciousness.Multipliers.TryGetValue((multiplierOwner, identifier), out var rawMultiplier))
            return false;

        multiplier = rawMultiplier;

        return true;
    }

    /// <summary>
    /// Remove a consciousness multiplier. This value gets added onto the multiplier used to calculate consciousness.
    /// </summary>
    /// <param name="target">Target entity</param>
    /// <param name="multiplierOwner">Owner of a multiplier</param>
    /// <param name="identifier">String identifier of the multiplier to remove</param>
    /// <param name="consciousness">Consciousness component</param>
    /// <returns>Successful</returns>
    public bool RemoveConsciousnessMultiplier(EntityUid target,
        EntityUid multiplierOwner,
        string identifier,
        ConsciousnessComponent? consciousness = null)
    {
        if (!Resolve(target, ref consciousness))
            return false;

        if (!consciousness.Multipliers.Remove((multiplierOwner, identifier)))
            return false;

        UpdateConsciousnessMultipliers(target, consciousness);
        Dirty(target, consciousness);

        return true;
    }

    #endregion
}
