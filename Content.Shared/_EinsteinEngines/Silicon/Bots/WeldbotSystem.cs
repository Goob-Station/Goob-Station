using Content.Shared.Emag.Systems;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._EinsteinEngines.Silicon.Bots;

/// <summary>
/// Handles emagging Weldbots
/// </summary>
public sealed class WeldbotSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeldbotComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnEmagged(EntityUid uid, WeldbotComponent comp, ref GotEmaggedEvent args)
    {
        _audio.PlayPredicted(comp.EmagSparkSound, uid, args.UserUid);

        comp.IsEmagged = true;
        args.Handled = true;
    }
}
