// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Audio.Jukebox;
using Content.Shared.Power;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using JukeboxComponent = Content.Shared.Audio.Jukebox.JukeboxComponent;
using Robust.Shared.Random; // Pirate

namespace Content.Server.Audio.Jukebox;


public sealed class JukeboxSystem : SharedJukeboxSystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!; // Pirate
    [Dependency] private readonly TransformSystem _transform = default!; // Pirate
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!; // Pirate

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<JukeboxComponent, JukeboxSelectedMessage>(OnJukeboxSelected);
        SubscribeLocalEvent<JukeboxComponent, JukeboxPlayingMessage>(OnJukeboxPlay);
        SubscribeLocalEvent<JukeboxComponent, JukeboxPauseMessage>(OnJukeboxPause);
        SubscribeLocalEvent<JukeboxComponent, JukeboxStopMessage>(OnJukeboxStop);
        SubscribeLocalEvent<JukeboxComponent, JukeboxSetPlaybackModeMessage>(OnJukeboxSetPlayback); // Pirate
        SubscribeLocalEvent<JukeboxComponent, JukeboxSetTimeMessage>(OnJukeboxSetTime);
        SubscribeLocalEvent<JukeboxComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<JukeboxComponent, ComponentShutdown>(OnComponentShutdown);

        SubscribeLocalEvent<JukeboxComponent, ComponentStartup>(OnComponentStartup); // Pirate
        SubscribeLocalEvent<JukeboxComponent, PowerChangedEvent>(OnPowerChanged);
    }

    private void OnComponentInit(EntityUid uid, JukeboxComponent component, ComponentInit args)
    {
        if (HasComp<ApcPowerReceiverComponent>(uid))
        {
            TryUpdateVisualState(uid, component);
        }
    }

    // Pirate: Shuffle & Repeat
    private void OnComponentStartup(Entity<JukeboxComponent> entity, ref ComponentStartup ev)
    {
        UpdateUI(entity);
    }

    private void UpdateUI(Entity<JukeboxComponent> ent)
    {
        var state = new JukeboxInterfaceState(ent.Comp.PlaybackMode);
        _userInterface.SetUiState(ent.Owner, JukeboxUiKey.Key, state);
    }
    // End Pirate: Shuffle & Repeat

    private void OnJukeboxPlay(EntityUid uid, JukeboxComponent component, ref JukeboxPlayingMessage args)
    {
        if (Exists(component.AudioStream))
        {
            Audio.SetState(component.AudioStream, AudioState.Playing);
        }
        else
        {
            component.AudioStream = Audio.Stop(component.AudioStream);

            // Pirate: Shuffling feature.
            if (component.PlaybackMode == JukeboxPlaybackMode.Shuffle
                && !component.FirstPlay
                && _protoManager.TryGetRandom<JukeboxPrototype>(_random, out var newProto)
                && newProto is JukeboxPrototype newJukeboxProto)
            {
                component.SelectedSongId = newJukeboxProto;
            }
            // End Pirate

            if (string.IsNullOrEmpty(component.SelectedSongId) ||
                !_protoManager.Resolve(component.SelectedSongId, out var jukeboxProto))
            {
                return;
            }

            component.AudioStream = Audio.PlayPvs(jukeboxProto.Path, uid, AudioParams.Default.WithMaxDistance(10f).WithVolume(-6f))?.Entity;

            // Pirate: shuffle state
            component.FirstPlay = false;
            // End Pirate

            Dirty(uid, component);
        }
    }

    private void OnJukeboxPause(Entity<JukeboxComponent> ent, ref JukeboxPauseMessage args)
    {
        Audio.SetState(ent.Comp.AudioStream, AudioState.Paused);
    }

    // Pirate: Shuffle & Repeat
    private void OnJukeboxSetPlayback(Entity<JukeboxComponent> ent, ref JukeboxSetPlaybackModeMessage playbackModeMessage)
    {
        if (ent.Comp.PlaybackMode != playbackModeMessage.PlaybackMode)
        {
            ent.Comp.PlaybackMode = playbackModeMessage.PlaybackMode;
            UpdateUI(ent);
            Dirty(ent);
        }
    }

    public AudioState GetAudioState(EntityUid? entity, AudioComponent? component = null)
    {
        if (entity == null || !Resolve(entity.Value, ref component, false))
            return AudioState.Stopped; // Consider no audio as stopped.

        return component.State;
    }
    // End Pirate: Shuffle & Repeat

    private void OnJukeboxSetTime(EntityUid uid, JukeboxComponent component, JukeboxSetTimeMessage args)
    {
        if (TryComp(args.Actor, out ActorComponent? actorComp))
        {
            var offset = actorComp.PlayerSession.Channel.Ping * 1.5f / 1000f;
            Audio.SetPlaybackPosition(component.AudioStream, args.SongTime + offset);
        }
    }

    private void OnPowerChanged(Entity<JukeboxComponent> entity, ref PowerChangedEvent args)
    {
        TryUpdateVisualState(entity);

        if (!this.IsPowered(entity.Owner, EntityManager))
        {
            Stop(entity);
        }
    }

    private void OnJukeboxStop(Entity<JukeboxComponent> entity, ref JukeboxStopMessage args)
    {
        Stop(entity);
    }

    // Pirate: Modified Stop() function for the Shuffling & Replay features.
    private void Stop(Entity<JukeboxComponent> entity)
    {
        //Audio.SetState(entity.Comp.AudioStream, AudioState.Stopped); // No longer needed since we're removing the AudioStream.
        entity.Comp.AudioStream = Audio.Stop(entity.Comp.AudioStream);
        entity.Comp.FirstPlay = true;
        Dirty(entity);
    }
    // End Pirate

    private void OnJukeboxSelected(EntityUid uid, JukeboxComponent component, JukeboxSelectedMessage args)
    {
        // Pirate: allow selecting songs while they're playing
        bool wasPlaying = Audio.IsPlaying(component.AudioStream);
        component.SelectedSongId = args.SongId;
        DirectSetVisualState(uid, JukeboxVisualState.Select);
        component.Selecting = true;
        component.SelectAccumulator = 0;
        component.AudioStream = Audio.Stop(component.AudioStream);
        component.FirstPlay = true; // Prevent shuffling
        if (wasPlaying)
        {
            var msg = new JukeboxPlayingMessage();
            OnJukeboxPlay(uid, component, ref msg);
        }
        // End Pirate

        Dirty(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<JukeboxComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Selecting)
            {
                comp.SelectAccumulator += frameTime;
                if (comp.SelectAccumulator >= 0.5f)
                {
                    comp.SelectAccumulator = 0f;
                    comp.Selecting = false;

                    TryUpdateVisualState(uid, comp);
                }
            }

            // Pirate: Replay feature. Please pitch in if you have better ideas. This is a pretty bad implementation.
            if (comp.PlaybackMode != JukeboxPlaybackMode.Single && comp.AudioStream != null &&
                GetAudioState(comp.AudioStream) == AudioState.Stopped)
            {
                var msg = new JukeboxPlayingMessage();
                OnJukeboxPlay(uid, comp, ref msg);
            }
            // End Pirate
        }
    }

    private void OnComponentShutdown(EntityUid uid, JukeboxComponent component, ComponentShutdown args)
    {
        component.AudioStream = Audio.Stop(component.AudioStream);
    }

    private void DirectSetVisualState(EntityUid uid, JukeboxVisualState state)
    {
        _appearanceSystem.SetData(uid, JukeboxVisuals.VisualState, state);
    }

    private void TryUpdateVisualState(EntityUid uid, JukeboxComponent? jukeboxComponent = null)
    {
        if (!Resolve(uid, ref jukeboxComponent))
            return;

        var finalState = JukeboxVisualState.On;

        if (!this.IsPowered(uid, EntityManager))
        {
            finalState = JukeboxVisualState.Off;
        }

        _appearanceSystem.SetData(uid, JukeboxVisuals.VisualState, finalState);
    }
}
