using Content.Goobstation.Shared.Cinematic;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Cinematic;

/// <summary>
/// Writes out captions on a local players screen.
/// </summary>
public sealed partial class CinematicCaptionSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private CinematicCaptionOverlay _overlay = default!;

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

    public override void Shutdown()
    {
        base.Shutdown();

        _overlayMan.RemoveOverlay(_overlay);
        _overlay.Dispose();
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
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(EntityUid uid, CinematicCaptionComponent component, LocalPlayerAttachedEvent args)
    {
        SetShaders(component);
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, CinematicCaptionComponent component, LocalPlayerDetachedEvent args)
        => _overlayMan.RemoveOverlay(_overlay);

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
        if (component.TextSound == null || component.Stream != null)
            return;

        component.Stream = _audio.PlayGlobal(component.TextSound, Filter.Local(), false)?.Entity;

        if (component.Stream is { } stream)
            EnsureComp<CinematicSceneSoundComponent>(stream);
    }

    private void StopSound(CinematicCaptionComponent component)
    {
        if (component.Stream is { } stream)
            _audio.Stop(stream);

        component.Stream = null;
    }
}
