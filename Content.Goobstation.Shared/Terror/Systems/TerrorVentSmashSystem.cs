using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Shared.Tools.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Smashes open welded vents.
/// </summary>
public sealed class TerrorVentSmashSystem : EntitySystem
{
    [Dependency] private readonly WeldableSystem _weldable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TerrorVentSmashComponent, TerrorVentSmashEvent>(OnSmash);
    }

    private void OnSmash(EntityUid uid, TerrorVentSmashComponent component, ref TerrorVentSmashEvent args)
    {
        var target = args.Target;

        if (!_weldable.IsWelded(target))
            return;

        _audio.PlayPredicted(component.SmashSound, uid, uid);

        if (_net.IsClient)
            return;

        _weldable.SetWeldedState(target, false);
    }
}
