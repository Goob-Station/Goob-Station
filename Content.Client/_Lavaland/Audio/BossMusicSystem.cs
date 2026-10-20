// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Audio;
using Content.Shared._Lavaland.Aggression;
using Content.Shared._Lavaland.Audio;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Client.Audio;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._Lavaland.Audio;

/// <summary>
/// Plays the music of the boss the local player is fighting.
/// <see cref="BossMusicListenerComponent"/> holds the state on the local player entity.
/// </summary>
public sealed partial class BossMusicSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private AudioSystem _audio = default!;
    [Dependency] private ContentAudioSystem _contentAudio = default!;
    [Dependency] private MobStateSystem _mobState = default!;

    // I don't care enough to change the rest of the trycomps.
    [Dependency] private EntityQuery<BossMusicListenerComponent> _bossListenerQuery;

    /// <summary>
    /// Used to block ambient music.
    /// </summary>
    public bool Active => _player.LocalEntity is { } local && HasComp<BossMusicListenerComponent>(local);

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_cfg, CCVars.LobbyMusicVolume, OnVolumeChanged);
    }

    public override void Shutdown()
    {
        base.Shutdown();

        StopAll();
    }

    /// <summary>
    /// Only used for delayed starts.
    /// </summary>
    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_player.LocalEntity is not { } local
            || !_bossListenerQuery.TryComp(local, out var listener)
            || listener.StartAt is not { } startAt
            || _timing.CurTime < startAt)
            return;

        listener.StartAt = null;
        Play((local, listener));
    }

    #region Public API

    public void AdjustVolume(float decibels)
    {
        if (_player.LocalEntity is not { } local
            || !TryComp<BossMusicListenerComponent>(local, out var listener))
            return;

        listener.VolumeOffset = Math.Min(decibels, -_proto.Index(listener.Music).Sound.Params.Volume);
        if (listener.Stream is { } stream)
            _audio.SetVolume(stream, Volume(listener));
    }

    public void RestoreVolume()
        => AdjustVolume(0f);

    public void SetPaused(bool paused)
    {
        if (_player.LocalEntity is not { } local || !TryComp<BossMusicListenerComponent>(local, out var listener))
            return;

        listener.Paused = paused;
        if (listener.Stream is { } stream)
            _audio.SetState(stream, paused ? AudioState.Paused : AudioState.Playing);
    }

    #endregion

    #region spam

    [SubscribeLocalEvent]
    private void OnPlayAmbientMusic(ref PlayAmbientMusicEvent ev)
    {
        if (Active)
            ev.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void OnAggressorState(Entity<AggressorComponent> ent, ref AfterAutoHandleStateEvent args)
        => Refresh(ent);

    [SubscribeLocalEvent]
    private void OnAggressorShutdown(Entity<AggressorComponent> ent, ref ComponentShutdown args)
        => Refresh(ent);

    [SubscribeLocalEvent]
    private void OnAggressorMobState(Entity<AggressorComponent> ent, ref MobStateChangedEvent args)
        => Refresh(ent);

    [SubscribeLocalEvent]
    private void OnAggressorParentChanged(Entity<AggressorComponent> ent, ref EntParentChangedMessage args)
        => Refresh(ent);

    [SubscribeLocalEvent]
    private void OnBossMusicStartup(Entity<BossMusicComponent> ent, ref ComponentStartup args)
        => Refresh(_player.LocalEntity);

    [SubscribeLocalEvent]
    private void OnBossMobState(Entity<BossMusicComponent> ent, ref MobStateChangedEvent args)
        => Refresh(_player.LocalEntity);

    [SubscribeLocalEvent]
    private void OnPlayerAttached(LocalPlayerAttachedEvent args)
        => Refresh(args.Entity);

    [SubscribeLocalEvent]
    private void OnPlayerDetached(LocalPlayerDetachedEvent args)
        => RemComp<BossMusicListenerComponent>(args.Entity);

    [SubscribeLocalEvent]
    private void OnRoundEnd(RoundEndMessageEvent args)
        => StopAll();

    [SubscribeLocalEvent]
    private void OnListenerShutdown(Entity<BossMusicListenerComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Stream is not { } stream)
            return;

        if (_proto.Index(ent.Comp.Music) is { FadeIn: true, FadeOutTime: > 0f } proto)
            _contentAudio.FadeOut(stream, duration: proto.FadeOutTime);
        else
            _audio.Stop(stream);
    }

    #endregion

    private void Refresh(EntityUid? player)
    {
        if (player is not { } local || _player.LocalEntity != local)
            return;

        var playing = TryComp<BossMusicListenerComponent>(local, out var listener);

        if (!TryComp<AggressorComponent>(local, out var aggressor)
            || !IsFighting((local, aggressor))
            || !FindBoss(local, aggressor, listener, out var boss, out var music))
        {
            RemComp<BossMusicListenerComponent>(local);
            return;
        }

        if (playing && listener!.Music == music)
        {
            listener.Boss = boss;
            return;
        }

        RemComp<BossMusicListenerComponent>(local);
        Start(local, boss, music);
    }

    #region Helpers

    private bool IsFighting(Entity<AggressorComponent> ent)
    {
        return ent.Comp.LifeStage <= ComponentLifeStage.Running
            && ent.Comp.Aggressives.Count > 0
            && !_mobState.IsDead(ent);
    }

    private bool FindBoss(EntityUid local,
        AggressorComponent aggressor,
        BossMusicListenerComponent? listener,
        out EntityUid boss,
        out ProtoId<BossMusicPrototype> music)
    {
        if (listener != null && aggressor.Aggressives.Contains(listener.Boss) && !_mobState.IsDead(listener.Boss))
        {
            boss = listener.Boss;
            music = TryComp<BossMusicComponent>(boss, out var current) ? current.SoundId : listener.Music;
            return true;
        }

        var map = Transform(local).MapID;
        foreach (var uid in aggressor.Aggressives)
        {
            if (!CanPlay(uid, map))
                continue;

            boss = uid;
            music = Comp<BossMusicComponent>(uid).SoundId;
            return true;
        }

        boss = default;
        music = default;
        return false;
    }

    private bool CanPlay(EntityUid boss, MapId map)
    {
        return !TerminatingOrDeleted(boss)
            && HasComp<BossMusicComponent>(boss)
            && Transform(boss).MapID == map
            && !_mobState.IsDead(boss);
    }

    private void Start(EntityUid local, EntityUid boss, ProtoId<BossMusicPrototype> music)
    {
        var listener = AddComp<BossMusicListenerComponent>(local);
        listener.Boss = boss;
        listener.Music = music;

        var proto = _proto.Index(music);
        if (proto.StartDelay > 0f)
            listener.StartAt = _timing.CurTime + TimeSpan.FromSeconds(proto.StartDelay);
        else
            Play((local, listener));
    }

    private void Play(Entity<BossMusicListenerComponent> ent)
    {
        var proto = _proto.Index(ent.Comp.Music);
        var stream = _audio.PlayGlobal(proto.Sound, Filter.Local(), false, AudioParams.Default.WithVolume(Volume(ent.Comp)).WithLoop(true));

        if (stream == null)
            return;

        ent.Comp.Stream = stream.Value.Entity;
        if (proto is { FadeIn: true, FadeInTime: > 0f })
            _contentAudio.FadeIn(stream.Value.Entity, duration: proto.FadeInTime);

        if (ent.Comp.Paused)
            _audio.SetState(stream.Value.Entity, AudioState.Paused);

        _contentAudio.DisableAmbientMusic();
    }

    private void StopAll()
    {
        var query = EntityQueryEnumerator<BossMusicListenerComponent>();
        while (query.MoveNext(out var uid, out var listener))
        {
            RemCompDeferred(uid, listener);
        }
    }

    private float Volume(BossMusicListenerComponent listener)
    {
        return _proto.Index(listener.Music).Sound.Params.Volume
            + SharedAudioSystem.GainToVolume(_cfg.GetCVar(CCVars.LobbyMusicVolume))
            + listener.VolumeOffset;
    }

    private void OnVolumeChanged(float gain)
    {
        if (_player.LocalEntity is { } local
            && TryComp<BossMusicListenerComponent>(local, out var listener)
            && listener.Stream is { } stream)
            _audio.SetVolume(stream, Volume(listener));
    }

    #endregion
}
