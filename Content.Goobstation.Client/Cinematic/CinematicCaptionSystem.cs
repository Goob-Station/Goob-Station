using Content.Goobstation.Shared.Cinematic;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Cinematic;

/// <summary>
/// Writes out captions on a local players screen.
/// This is explicitly not listening to the cvar because it can hold important information (Role intros and such)
/// </summary>
public sealed partial class CinematicCaptionSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private CinematicCaptionOverlay _overlay = default!;
    private readonly List<SoundFade> _fades = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CinematicCaptionComponent, ComponentInit>(OnCaptionInit);
        SubscribeLocalEvent<CinematicCaptionComponent, ComponentStartup>(OnCaptionStartup);
        SubscribeLocalEvent<CinematicCaptionComponent, ComponentShutdown>(OnCaptionShutdown);
        SubscribeLocalEvent<CinematicCaptionComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<CinematicCaptionComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new();
    }


    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var player = _playerManager.LocalEntity;

        var query = EntityQueryEnumerator<CinematicCaptionComponent>();
        while (query.MoveNext(out var uid, out var caption))
        {
            caption.Age += frameTime;

            var typing = uid == player && caption.Progress < 1f;
            if (typing)
            {
                caption.Progress = MathF.Min(1f, caption.Progress + frameTime / MathF.Max(0.01f, caption.WriteTime));
                typing = caption.Progress < 1f;
            }

            if (typing)
                StartSound(caption);
            else
                StopSound(caption);
        }

        UpdateFades(frameTime);
    }

    private void UpdateFades(float frameTime)
    {
        for (var i = _fades.Count - 1; i >= 0; i--)
        {
            var fade = _fades[i];
            fade.Elapsed += frameTime;

            var progress = fade.Elapsed / fade.Duration;
            if (progress >= 1f || !TryComp<AudioComponent>(fade.Stream, out var audio))
            {
                _audio.Stop(fade.Stream);
                _fades.RemoveAt(i);
                continue;
            }

            _audio.SetVolume(fade.Stream, fade.StartVolume + SharedAudioSystem.GainToVolume(1f - progress), audio);
            _fades[i] = fade;
        }
    }

    private void OnCaptionInit(EntityUid uid, CinematicCaptionComponent component, ComponentInit args)
    {
        if (uid != _playerManager.LocalEntity)
            return;

        SetShaders(component);
        _overlayMan.AddOverlay(_overlay);
    }

    private void SetShaders(CinematicCaptionComponent component)
    {
        _overlay.AuraShader?.Dispose();
        _overlay.AuraShader = _proto.Index<ShaderPrototype>(component.AuraShader).InstanceUnique();

        _overlay.BlurShader?.Dispose();
        _overlay.BlurShader = _proto.Index<ShaderPrototype>(component.BlurShader).InstanceUnique();
    }

    private void OnCaptionStartup(EntityUid uid, CinematicCaptionComponent component, ComponentStartup args)
        => Localize(uid, component);

    private void OnCaptionShutdown(EntityUid uid, CinematicCaptionComponent component, ComponentShutdown args)
    {
        StopSound(component);

        if (uid == _playerManager.LocalEntity)
            RemoveOverlay();
    }

    private void OnPlayerAttached(EntityUid uid, CinematicCaptionComponent component, LocalPlayerAttachedEvent args)
    {
        SetShaders(component);
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, CinematicCaptionComponent component, LocalPlayerDetachedEvent args)
        => RemoveOverlay();

    private void RemoveOverlay()
    {
        _overlayMan.RemoveOverlay(_overlay);
        _overlay.ReleaseTargets();
    }

    /// <summary>
    /// Resolves the captions text.
    /// </summary>
    private void Localize(EntityUid uid, CinematicCaptionComponent component)
    {
        if (string.IsNullOrEmpty(component.Text)
            || !TryComp<CinematicComponent>(uid, out var cinematic))
            return;

        var text = Loc.GetString(component.Text,
            ("name", cinematic.SubjectName ?? string.Empty),
            ("station", cinematic.StationName ?? string.Empty));

        component.Subject = component.ShowSubject ? cinematic.SubjectName ?? string.Empty : string.Empty;

        if (text == component.Target)
            return;

        component.Target = text;
        component.Progress = 0f;
    }

    private void StartSound(CinematicCaptionComponent component)
    {
        if (component.TextSound == null)
            return;

        if (component.Stream is { } current && !TerminatingOrDeleted(current))
            return;

        component.Stream = _audio.PlayGlobal(component.TextSound, Filter.Local(), false)?.Entity;

        if (component.Stream is { } stream)
            EnsureComp<CinematicSceneSoundComponent>(stream);
    }

    private void StopSound(CinematicCaptionComponent component)
    {
        if (component.Stream is { } stream && !TerminatingOrDeleted(stream))
            FadeOut(stream, component.TextSoundFadeTime);

        component.Stream = null;
    }

    private void FadeOut(EntityUid stream, float duration)
    {
        if (duration <= 0f || !TryComp<AudioComponent>(stream, out var audio))
        {
            _audio.Stop(stream);
            return;
        }

        _fades.Add(new SoundFade(stream, duration, audio.Params.Volume));
    }

    private record struct SoundFade(EntityUid Stream, float Duration, float StartVolume)
    {
        public float Elapsed;
    }
}
