// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Audio;
using Content.Shared._Lavaland.Audio;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Mobs;
using Robust.Client.Audio;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._Lavaland.Audio;

// TODO: Port this system to Shared and optimize it.
public sealed class BossMusicSystem : SharedBossMusicSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    [Dependency] private readonly ContentAudioSystem _audioContent = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private static float _volumeSlider;
    private Entity<AudioComponent?>? _bossMusicStream;
    private BossMusicPrototype? _musicProto;

    // Need how much volume to change per tick and just remove it when it drops below "0"
    private readonly Dictionary<EntityUid, float> _fadingOut = new();

    // Need volume change per tick + target volume.
    private readonly Dictionary<EntityUid, (float VolumeChange, float TargetVolume)> _fadingIn = new();

    private readonly List<EntityUid> _fadeToRemove = new();

    private const float MinVolume = -32f;
    private const float DefaultDuration = 1f;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_configManager, CCVars.LobbyMusicVolume, BossVolumeCVarChanged, true);

        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnMindRemoved);
        SubscribeLocalEvent<ActorComponent, MobStateChangedEvent>(OnPlayerDeath);
        SubscribeLocalEvent<ActorComponent, EntParentChangedMessage>(OnPlayerParentChange);
        SubscribeLocalEvent<RoundEndMessageEvent>(OnRoundEnd);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _bossMusicStream = _audio.Stop(_bossMusicStream);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        UpdateFade(frameTime);
    }

    private void BossVolumeCVarChanged(float obj)
    {
        _volumeSlider = SharedAudioSystem.GainToVolume(obj);

        if (_bossMusicStream != null
            && _musicProto != null)
            _audio.SetVolume(_bossMusicStream, _musicProto.Sound.Params.Volume + _volumeSlider);
    }

    public override void StartBossMusic(ProtoId<BossMusicPrototype> music)
    {
        if (_musicProto != null || _bossMusicStream != null)
            return;

        _audioContent.DisableAmbientMusic();

        var sound = _proto.Index(music);
        _musicProto = sound;

        var stream = _audio.PlayGlobal(
            sound.Sound,
            Filter.Local(),
            false,
            AudioParams.Default.WithVolume(sound.Sound.Params.Volume + _volumeSlider).WithLoop(true));

        if (_musicProto.FadeIn
            && stream != null)
        {
            _bossMusicStream = (stream.Value.Entity, stream.Value.Component);
            FadeIn(_bossMusicStream, stream.Value.Component, sound.FadeInTime);
        }
    }

    public override void EndAllMusic()
    {
        if (_musicProto == null
            || _bossMusicStream == null)
            return;

        if (_musicProto.FadeIn)
            FadeOut(_bossMusicStream, duration: _musicProto.FadeOutTime);
        else
            _audio.Stop(_bossMusicStream);

        _musicProto = null;
        _bossMusicStream = null;
    }

    private void OnMindRemoved(LocalPlayerDetachedEvent args)
    {
        EndAllMusic();
    }

    private void OnPlayerDeath(Entity<ActorComponent> ent, ref MobStateChangedEvent args)
    {
        if (ent.Comp.PlayerSession == _player.LocalSession &&
            args.NewMobState == MobState.Dead)
            EndAllMusic();
    }

    /// <summary>
    /// Raised when salvager escapes from lavaland (ohio reference)
    /// </summary>
    private void OnPlayerParentChange(Entity<ActorComponent> ent, ref EntParentChangedMessage args)
    {
        if (ent.Comp.PlayerSession == _player.LocalSession &&
            args.OldMapId != null)
            EndAllMusic();
    }

    private void OnRoundEnd(RoundEndMessageEvent args)
    {
        _bossMusicStream = _audio.Stop(_bossMusicStream);
    }

    #region Fades

    private void FadeOut(EntityUid? stream, AudioComponent? component = null, float duration = DefaultDuration)
    {
        if (stream == null || duration <= 0f || !Resolve(stream.Value, ref component))
            return;

        _fadingIn.Remove(stream.Value);
        var diff = component.Volume - MinVolume;
        _fadingOut.Add(stream.Value, diff / duration);
    }

    private void FadeIn(EntityUid? stream, AudioComponent? component = null, float duration = DefaultDuration)
    {
        if (stream == null || duration <= 0f || !Resolve(stream.Value, ref component) || component.Volume < MinVolume)
            return;

        _fadingOut.Remove(stream.Value);
        var curVolume = component.Volume;
        var change = (MinVolume - curVolume) / duration;
        _fadingIn.Add(stream.Value, (change, component.Volume));
        component.Volume = MinVolume;
    }

    private void UpdateFade(float frameTime)
    {
        _fadeToRemove.Clear();

        foreach (var (stream, change) in _fadingOut)
        {
            if (!TryComp(stream, out AudioComponent? component))
            {
                _fadeToRemove.Add(stream);
                continue;
            }

            var volume = component.Volume - change * frameTime;
            volume = MathF.Max(MinVolume, volume);
            _audio.SetVolume(stream, volume, component);

            if (component.Volume.Equals(MinVolume))
            {
                _audio.Stop(stream);
                _fadeToRemove.Add(stream);
            }
        }

        foreach (var stream in _fadeToRemove)
        {
            _fadingOut.Remove(stream);
        }

        _fadeToRemove.Clear();

        foreach (var (stream, (change, target)) in _fadingIn)
        {
            // Cancelled elsewhere
            if (!TryComp(stream, out AudioComponent? component))
            {
                _fadeToRemove.Add(stream);
                continue;
            }

            var volume = component.Volume - change * frameTime;
            volume = MathF.Min(target, volume);
            _audio.SetVolume(stream, volume, component);

            if (component.Volume.Equals(target))
            {
                _fadeToRemove.Add(stream);
            }
        }

        foreach (var stream in _fadeToRemove)
        {
            _fadingIn.Remove(stream);
        }
    }

    #endregion
}
