// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus
//
// SPDX-License-Identifier: MPL-2.0

// some parts taken and modified from https://github.com/TornadoTechnology/finster/blob/1af5daf6270477a512ee9d515371311443e97878/Content.Shared/_Finster/Audio/SharedAudioEffectsSystem.cs#L13 , credit to docnite
// they're under WTFPL so its quite allowed

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Content.Shared.GameTicking;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Goobstation.Client.Audio;

/// <summary>
///     Handler for client-side audio effects.
/// </summary>
public sealed class AudioEffectSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    /// <summary>
    ///     Whether creating new auxiliaries is safe.
    ///         This is done because integration tests
    ///         apparently can't handle them.
    /// 
    ///     Null means this value wasn't determined yet.
    /// 
    ///     Any not-null value here is the final result,
    ///         and no more attempts to determine this
    ///         will be made afterwards.
    /// </summary>
    // actually this problem applies for effects too
    private bool? _auxiliariesSafe = null;

    private static readonly Dictionary<ProtoId<AudioPresetPrototype>, (EntityUid AuxiliaryUid, EntityUid EffectUid)> CachedEffects = new();

    /// <summary>
    ///     An auxiliary with no effect; for removing effects.
    /// </summary>
    // TODO: remove this when an rt method to actually remove effects gets added
    private EntityUid _cachedBlankAuxiliaryUid;

    public override void Initialize()
    {
        base.Initialize();

        // You can't keep references to this past round-end so it must be cleaned up.
        SubscribeNetworkEvent<RoundRestartCleanupEvent>(_ => Cleanup()); // its not raised on client
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypeReload);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        Cleanup();
    }

    private void OnPrototypeReload(PrototypesReloadedEventArgs args)
    {
        if (!args.WasModified<AudioPresetPrototype>())
            return;

        // get rid of all old cached entities, and replace them with new ones
        var oldPresets = new List<ProtoId<AudioPresetPrototype>>();
        foreach (var cache in CachedEffects)
        {
            oldPresets.Add(cache.Key);

            TryQueueDel(cache.Value.AuxiliaryUid);
            TryQueueDel(cache.Value.EffectUid);
        }
        CachedEffects.Clear();

        foreach (var oldPreset in oldPresets)
        {
            if (!ResolveCachedEffect(oldPreset, out var cachedAuxiliaryUid, out var cachedEffectUid))
                continue;

            CachedEffects[oldPreset] = (cachedAuxiliaryUid.Value, cachedEffectUid.Value);
        }
    }

    private void Cleanup()
    {
        foreach (var cache in CachedEffects)
        {
            TryQueueDel(cache.Value.AuxiliaryUid);
            TryQueueDel(cache.Value.EffectUid);
        }
        CachedEffects.Clear();

        if (_cachedBlankAuxiliaryUid.IsValid())
            TryQueueDel(_cachedBlankAuxiliaryUid);

        _cachedBlankAuxiliaryUid = EntityUid.Invalid;
    }


    /// <summary>
    ///     Figures out whether auxiliaries are safe to use. Returns
    ///         whether a safe auxiliary pair has been outputted
    ///         for use.
    /// </summary>
    private bool DetermineAuxiliarySafety([NotNullWhen(true)] out (EntityUid Entity, AudioAuxiliaryComponent Component)? auxiliaryPair, bool destroyPairAfterUse = true)
    {
        (EntityUid Entity, AudioAuxiliaryComponent Component)? maybeAuxiliaryPair = null;
        try
        {
            maybeAuxiliaryPair = _audioSystem.CreateAuxiliary();
            _auxiliariesSafe = true;
        }
        catch (Exception ex)
        {
            Log.Info($"Determined audio auxiliaries are unsafe in this run! If this is not an integration test, report this immediately. Exception: {ex}");
            _auxiliariesSafe = false;

            TryQueueDel(maybeAuxiliaryPair?.Entity);

            auxiliaryPair = null;
            return false;
        }

        if (destroyPairAfterUse)
        {
            QueueDel(maybeAuxiliaryPair.Value.Entity);

            auxiliaryPair = null;
            return false;
        }

        auxiliaryPair = maybeAuxiliaryPair.Value;
        return true;
    }

    /// <summary>
    ///     Returns whether auxiliaries are definitely safe to use.
    ///         Determines auxiliary safety if not already.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AuxiliariesAreDefinitelySafe()
    {
        if (_auxiliariesSafe == null)
            DetermineAuxiliarySafety(out _, destroyPairAfterUse: true);

        return _auxiliariesSafe == true;
    }

    /// <summary>
    ///     Tries to resolve a cached audio auxiliary entity corresponding to the prototype to apply
    ///         to the given entity.
    /// </summary>
    public bool TryAddEffect(in Entity<AudioComponent> entity, in ProtoId<AudioPresetPrototype> preset)
    {
        if (!AuxiliariesAreDefinitelySafe() ||
            !ResolveCachedEffect(preset, out var auxiliaryUid, out _))
            return false;

        _audioSystem.SetAuxiliary(entity, entity.Comp, auxiliaryUid);
        return true;
    }

    /// <summary>
    ///     Tries to remove effects from the given audio. Returns whether the attempt was successful,
    ///         or no auxiliary is applied to the audio.
    /// </summary>
    public bool TryRemoveEffect(in Entity<AudioComponent> entity)
    {
        if (!AuxiliariesAreDefinitelySafe())
            return false;

        if (entity.Comp.Auxiliary is not { } existingAuxiliaryUid ||
            !existingAuxiliaryUid.IsValid())
            return true;

        // resolve the cached auxiliary
        if (!_cachedBlankAuxiliaryUid.IsValid())
            _cachedBlankAuxiliaryUid = _audioSystem.CreateAuxiliary().Entity;

        _audioSystem.SetAuxiliary(entity, entity.Comp, _cachedBlankAuxiliaryUid);
        return true;
    }

    /// <summary>
    ///     Tries to resolve an audio auxiliary and effect entity, creating and caching one if one doesn't already exist,
    ///         for a prototype. Do not modify it in any way.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ResolveCachedEffect(in ProtoId<AudioPresetPrototype> preset, [NotNullWhen(true)] out EntityUid? auxiliaryUid, [NotNullWhen(true)] out EntityUid? effectUid)
    {
        if (_auxiliariesSafe == false)
        {
            auxiliaryUid = null;
            effectUid = null;
            return false;
        }

        if (CachedEffects.TryGetValue(preset, out var cached))
        {
            auxiliaryUid = cached.AuxiliaryUid;
            effectUid = cached.EffectUid;
            return true;
        }

        return TryCacheEffect(preset, out auxiliaryUid, out effectUid);
    }

    /// <summary>
    ///     Tries to initialise and cache effect and auxiliary entities corresponding to a prototype,
    ///         in the system's internal cache.
    /// 
    ///     Does nothing if the entity already exists in the cache.
    /// </summary>
    /// <returns>Whether the entity was successfully initialised, and it did not previously exist in the cache.</returns>
    public bool TryCacheEffect(in ProtoId<AudioPresetPrototype> preset, [NotNullWhen(true)] out EntityUid? auxiliaryUid, [NotNullWhen(true)] out EntityUid? effectUid)
    {

        effectUid = null;
        auxiliaryUid = null;

        if (_auxiliariesSafe == false ||
            !_prototypeManager.TryIndex(preset, out var presetPrototype))
            return false;

        // i cant `??=` it
        (EntityUid Entity, AudioAuxiliaryComponent Component)? maybeAuxiliaryPair = null;

        // if undetermined, determine and keep the pair if confirmed safe
        if (_auxiliariesSafe == null)
        {
            // if determined unsafe, cleanup the pair
            if (!DetermineAuxiliarySafety(out maybeAuxiliaryPair, destroyPairAfterUse: false))
                return false;
        }

        // now, auxiliaries are known to be safe.
        // only when initially determining if auxiliaries are safe will we have a pair to use. in future attempts, we won't so just make one if necessary
        var auxiliaryPair = maybeAuxiliaryPair ?? _audioSystem.CreateAuxiliary();

        DebugTools.Assert(Exists(auxiliaryPair.Entity), "Audio auxiliary pair's entity does not exist!");
        if (!Exists(auxiliaryPair.Entity))
            return false;

        var effectPair = _audioSystem.CreateEffect();
        _audioSystem.SetEffectPreset(effectPair.Entity, effectPair.Component, presetPrototype);
        _audioSystem.SetEffect(auxiliaryPair.Entity, auxiliaryPair.Component, effectPair.Entity);

        effectUid = effectPair.Entity;
        auxiliaryUid = auxiliaryPair.Entity;

        return CachedEffects.TryAdd(preset, (auxiliaryPair.Entity, effectPair.Entity));
    }
}
