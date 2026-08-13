using Content.Client.Audio;
using Content.Goobstation.Shared.Antag.Intro;
using Content.Shared.GameTicking;
using Robust.Client.Audio;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Audio.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Reflection;
using Robust.Shared.Sandboxing;

namespace Content.Goobstation.Client.Antag.Intro;

/// <summary>
/// Plays the antagonists intro animation and tells the server when it's over.
/// </summary>
public sealed class AntagIntroSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ContentAudioSystem _fades = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IReflectionManager _reflection = default!;
    [Dependency] private readonly ISandboxHelper _sandbox = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;

    private readonly Dictionary<string, Type> _scenes = new();
    private AntagIntroScene? _playing;
    private ProtoId<AntagIntroPrototype>? _intro;
    private (EntityUid Entity, AudioComponent Component)? _track;
    private bool _fading;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<AntagIntroPlayEvent>(OnPlay);
        SubscribeNetworkEvent<RoundRestartCleanupEvent>(_ => Teardown());

        foreach (var type in _reflection.GetAllChildren<AntagIntroScene>())
        {
            if (!_scenes.TryAdd(type.Name, type))
                Log.Error($"Two antag intro scenes are both called {type.Name}.");
        }
    }

    public override void Shutdown()
    {
        base.Shutdown();

        Teardown();
    }

    private void OnPlay(AntagIntroPlayEvent args)
    {
        if (_playing != null
            || !_proto.TryIndex(args.Intro, out var intro)
            || Stage(intro.Scene) is not { } staged)
        {
            RaiseNetworkEvent(new AntagIntroFinishedEvent(args.Intro));
            return;
        }

        _playing = staged;
        _intro = args.Intro;
        _fading = false;

        _ui.WindowRoot.AddChild(_playing);
        LayoutContainer.SetAnchorPreset(_playing, LayoutContainer.LayoutPreset.Wide);
        _playing.OnSound = sound => _audio.PlayGlobal(sound, Filter.Local(), false);
        _playing.OnFadeOut = Quieten;

        if (intro.Track == null
            || _audio.PlayGlobal(intro.Track, Filter.Local(), false) is not { } stream)
            return;

        _track = (stream.Entity, stream.Component);
        _playing.TrackPosition = () => _track is { } track && Sound(track)
            ? track.Component.PlaybackPosition
            : null;
    }

    /// <summary>
    /// Starts the fading out.
    /// </summary>
    private void Quieten(float over)
    {
        if (_track is not { } track || !Sound(track))
            return;

        _fading = true;
        _fades.FadeOut(track.Entity, track.Component, over);
    }

    private bool Sound((EntityUid Entity, AudioComponent Component) sound)
        => !TerminatingOrDeleted(sound.Entity) && sound.Component.Running;

    private AntagIntroScene? Stage(string scene)
    {
        if (!_scenes.TryGetValue(scene, out var type))
        {
            Log.Error($"No antag intro staged for scene {scene}.");
            return null;
        }

        try
        {
            return (AntagIntroScene) _sandbox.CreateInstance(type);
        }
        catch (Exception e)
        {
            Log.Error($"Could not construct {scene}: {e}");
            return null;
        }
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_playing is { Finished: true })
            Teardown();
    }

    private void Teardown()
    {
        if (_playing == null)
            return;

        _playing.Orphan();
        _playing = null;

        if (_track is { } track)
        {
            if (!_fading && Sound(track))
                _audio.Stop(track.Entity, track.Component);

            _track = null;
        }

        if (_intro is not { } intro)
            return;

        _intro = null;
        RaiseNetworkEvent(new AntagIntroFinishedEvent(intro));
    }
}
