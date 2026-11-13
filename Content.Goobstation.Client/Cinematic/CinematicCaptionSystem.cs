using Content.Goobstation.Shared.Cinematic;
using Robust.Client.Graphics;
using Robust.Client.Player;
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
    [Dependency] private IOverlayManager _overlayMan = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedCinematicSystem _cinematic = default!;

    private Caption? _live;

    private readonly Dictionary<string, ShaderInstance> _shaders = new();

    private readonly CinematicCaptionOverlay _overlay = new();
    private bool _overlayShown;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CinematicCaptionComponent, ComponentStartup>(OnCaptionStartup);
        SubscribeLocalEvent<CinematicCaptionComponent, ComponentShutdown>(OnCaptionShutdown);
        SubscribeLocalEvent<CinematicCaptionComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<CinematicCaptionComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<CinematicCaptionComponent, CinematicUpdatedEvent>(OnCinematicUpdated);

    }

    public override void Shutdown()
    {
        base.Shutdown();

        foreach (var caption in _overlay.Captions)
            StopSound(caption);

        _overlay.Captions.Clear();
        _live = null;
        ShowOverlay(false);

        foreach (var shader in _shaders.Values)
            shader.Dispose();

        _shaders.Clear();
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        for (var i = _overlay.Captions.Count - 1; i >= 0; i--)
        {
            var caption = _overlay.Captions[i];
            caption.Age += frameTime;

            if (caption != _live)
            {
                caption.Fade -= frameTime / MathF.Max(0.01f, caption.Style.FadeOutTime);
                if (caption.Fade <= 0f)
                    _overlay.Captions.RemoveAt(i);

                continue;
            }

            caption.Progress = MathF.Min(1f, caption.Progress + frameTime / MathF.Max(0.01f, caption.WriteTime));

            if (caption.Progress < 1f)
                StartSound(caption);
            else
                StopSound(caption);
        }

        ShowOverlay(_overlay.Captions.Count > 0);
    }

    private void OnCaptionStartup(Entity<CinematicCaptionComponent> ent, ref ComponentStartup args)
    {
        if (ent.Owner == _playerManager.LocalEntity)
            Show(ent);
    }

    private void OnCaptionShutdown(Entity<CinematicCaptionComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Owner == _playerManager.LocalEntity)
            Hide(fade: true);
    }

    private void OnPlayerAttached(Entity<CinematicCaptionComponent> ent, ref LocalPlayerAttachedEvent args)
        => Show(ent);

    private void OnPlayerDetached(Entity<CinematicCaptionComponent> ent, ref LocalPlayerDetachedEvent args)
        => Hide(fade: false);

    private void OnCinematicUpdated(Entity<CinematicCaptionComponent> ent, ref CinematicUpdatedEvent args)
    {
        if (_live != null && ent.Owner == _playerManager.LocalEntity)
            _live.Strength = args.Strength;
    }

    private void Show(Entity<CinematicCaptionComponent> ent)
    {
        Hide(fade: true);

        if (!_proto.TryIndex(ent.Comp.Style, out var style))
        {
            Log.Error($"Caption on {ToPrettyString(ent)} uses unknown style {ent.Comp.Style}.");
            return;
        }

        TryComp<CinematicComponent>(ent, out var cinematic);
        var name = cinematic?.SubjectName ?? string.Empty;
        var station = cinematic?.StationName ?? string.Empty;

        _live = new Caption
        {
            Style = style,
            Text = Loc.GetString(ent.Comp.Text, ("name", name), ("station", station)),
            Subject = ent.Comp.ShowSubject ? name : string.Empty,
            WriteTime = ent.Comp.WriteTime,
            AuraShader = GetShader(style.AuraShader),
            BlurShader = GetShader(style.BlurShader),
        };

        _overlay.Captions.Add(_live);
    }

    private void Hide(bool fade)
    {
        if (_live is not { } caption)
            return;

        StopSound(caption);
        _live = null;

        if (!fade)
            _overlay.Captions.Remove(caption);
    }

    private void ShowOverlay(bool shown)
    {
        if (shown == _overlayShown)
            return;

        _overlayShown = shown;

        if (shown)
        {
            _overlayMan.AddOverlay(_overlay);
            return;
        }

        _overlayMan.RemoveOverlay(_overlay);
        _overlay.ReleaseTargets();
    }

    private ShaderInstance? GetShader(string id)
    {
        if (_shaders.TryGetValue(id, out var shader))
            return shader;

        if (!_proto.TryIndex<ShaderPrototype>(id, out var proto))
        {
            Log.Error($"Unknown caption shader {id}.");
            return null;
        }

        return _shaders[id] = proto.InstanceUnique();
    }

    private void StartSound(Caption caption)
    {
        if (caption.Style.TextSound == null)
            return;

        if (caption.Stream is { } current && !TerminatingOrDeleted(current))
            return;

        caption.Stream = _audio.PlayGlobal(caption.Style.TextSound, Filter.Local(), false)?.Entity;

        if (caption.Stream is { } stream)
            EnsureComp<CinematicSceneSoundComponent>(stream);
    }

    private void StopSound(Caption caption)
    {
        if (caption.Stream is { } stream && !TerminatingOrDeleted(stream))
            _cinematic.FadeOut(stream, caption.Style.TextSoundFadeTime);

        caption.Stream = null;
    }

    public sealed class Caption
    {
        public required CinematicCaptionStylePrototype Style;
        public required string Text;
        public required string Subject;
        public required float WriteTime;

        public ShaderInstance? AuraShader;
        public ShaderInstance? BlurShader;

        public float Progress;

        public float Age;

        public float Strength;

        public float Fade = 1f;

        public EntityUid? Stream;
    }
}
