using Content.Goobstation.Shared.DynamicAudio.Effects;
using Content.Shared.GameTicking;
using Content.Shared.Maps;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Physics;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Content.Goobstation.Shared.DynamicAudio;

public abstract class SharedDynamicAudioSystem : EntitySystem
{
    private Dictionary<ProtoId<AudioPresetPrototype>, EntityUid> _presets = new();

    private string _defaultPreset = "Generic"; // change this to "LivingRoom" for normal audio
    private string _inSpacePreset = "InSpace";
    private int _soundMuffleInSpace = -10;

    [Dependency] protected readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] protected readonly IConfigurationManager _cfg = default!;
    [Dependency] protected readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly EntityLookupSystem _lookUp = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    /// <summary>
    /// Applies sound effects by it's environment.
    /// </summary>
    public void ApplyAudioEffect(Entity<AudioComponent> audio, EntityUid? source = null)
    {
        if (!_playerManager.LocalEntity.HasValue)
            return;

        var preset = GetAreaPrototypePreset(audio, source);

        if (!_presets.TryGetValue(preset, out var audioAux) && !TryCreateAudioEffect(preset, out audioAux))
            return;

        if (_net.IsServer)
            Timer.Spawn(TimeSpan.FromTicks(10L), () => _audio.SetAuxiliary(audio, audio, audioAux));
        else
            _audio.SetAuxiliary(audio, audio, audioAux);
    }

    /// <summary>
    /// Calculates sound area by it's effects components, local player position etc.
    /// </summary>
    private string GetAreaPrototypePreset(Entity<AudioComponent> audio, EntityUid? source = null)
    {
        var soundPosition = source is null ? _xform.GetWorldPosition(audio) : _xform.GetWorldPosition(source.Value);
        var soundTransform = source is null ? Transform(audio) : Transform(source.Value);

        if (soundPosition == Vector2.Zero && _playerManager.LocalEntity.HasValue)
        {
            soundPosition = _xform.GetWorldPosition(_playerManager.LocalEntity.Value);
            soundTransform = Transform(_playerManager.LocalEntity.Value);
        }

        // checks if sound or player in space
        if (soundTransform.GridUid is null || HasComp<InBarotraumaAudioEffectComponent>(soundTransform.Owner)
            || _playerManager.LocalEntity.HasValue && HasComp<InBarotraumaAudioEffectComponent>(_playerManager.LocalEntity))
        {
            _audio.SetVolume(audio, _soundMuffleInSpace);
            return _inSpacePreset;
        }

        return _defaultPreset;
    }

    private bool TryCreateAudioEffect(ProtoId<AudioPresetPrototype> preset, [NotNullWhen(true)] out EntityUid auxUid)
    {
        auxUid = default;

        if (!_prototype.TryIndex<AudioPresetPrototype>(preset, out var presetPrototype))
            return false;

        var effectEntity = _audio.CreateEffect();
        var auxEntity = _audio.CreateAuxiliary();

        _audio.SetEffectPreset(effectEntity.Entity, effectEntity.Component, presetPrototype);
        _audio.SetEffect(auxEntity.Entity, auxEntity.Component, effectEntity.Entity);

        if (Exists(auxEntity.Entity))
        {
            auxUid = auxEntity.Entity;
            _presets.Add(preset, auxUid);
            return true;
        }

        return false;
    }
}
