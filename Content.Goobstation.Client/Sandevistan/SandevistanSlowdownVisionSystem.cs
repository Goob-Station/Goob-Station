using Content.Goobstation.Shared.Sandevistan;
using Content.Shared._DV.CCVars;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Sources;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Sandevistan;

/// <summary>
/// handles the sandevistan overlay / audio
/// </summary>
public sealed class SandevistanSlowdownVisionSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private SandevistanSlowdownVisionOverlay _overlay = default!;

    private bool _audioSlowed;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanSlowdownVisionComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SandevistanSlowdownVisionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SandevistanSlowdownVisionComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SandevistanSlowdownVisionComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        Subs.CVar(_cfg, DCCVars.NoVisionFilters, OnNoVisionFiltersChanged);

        _overlay = new();
    }


    private void SetOverlay(bool show)
    {
        if (show && !_cfg.GetCVar(DCCVars.NoVisionFilters))
        {
            _overlayMan.AddOverlay(_overlay);
            return;
        }

        _overlayMan.RemoveOverlay(_overlay);
        RestoreAudio();
    }

    private void OnInit(Entity<SandevistanSlowdownVisionComponent> ent, ref ComponentInit args)
    {
        if (ent.Owner == _player.LocalEntity)
            SetOverlay(true);
    }

    private void OnShutdown(Entity<SandevistanSlowdownVisionComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Owner == _player.LocalEntity)
            SetOverlay(false);
    }

    private void OnPlayerAttached(Entity<SandevistanSlowdownVisionComponent> ent, ref LocalPlayerAttachedEvent args) =>
        SetOverlay(true);

    private void OnPlayerDetached(Entity<SandevistanSlowdownVisionComponent> ent, ref LocalPlayerDetachedEvent args) =>
        SetOverlay(false);

    private void OnNoVisionFiltersChanged(bool enabled) =>
        SetOverlay(_player.LocalEntity is { } player && HasComp<SandevistanSlowdownVisionComponent>(player));

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_player.LocalEntity is { } player
            && TryComp<SandevistanSlowdownVisionComponent>(player, out var comp)
            && comp.SlowAudio)
        {
            var pitch = comp.AudioPitch;
            var query = AllEntityQuery<AudioComponent>();
            while (query.MoveNext(out _, out var audio))
                SetSourcePitch(audio, audio.Params.Pitch * pitch);

            _audioSlowed = true;
        }
        else if (_audioSlowed)
            RestoreAudio();
    }

    private void RestoreAudio()
    {
        if (!_audioSlowed)
            return;

        var query = AllEntityQuery<AudioComponent>();
        while (query.MoveNext(out _, out var audio))
            SetSourcePitch(audio, audio.Params.Pitch);

        _audioSlowed = false;
    }

    private static void SetSourcePitch(AudioComponent audio, float value)
    {
        // A source only exists once the component is loaded, and a stopping component has already disposed it.
        if (!audio.Loaded || audio.LifeStage >= ComponentLifeStage.Stopping)
            return;

        ((IAudioSource) audio).Pitch = value;
    }
}
