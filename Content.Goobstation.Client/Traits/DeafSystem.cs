using Robust.Client.Audio;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Content.Goobstation.Shared.Traits.Components;

namespace Content.Goobstation.Client.Traits;

public sealed class DeafnessSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IAudioManager _audio = default!;

    private float _originalVolume = 0.5f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeafComponent, ComponentShutdown>(OnDeafShutdown);
        SubscribeLocalEvent<DeafComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<DeafComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<DeafComponent, ComponentStartup>(OnComponentStartUp);
    }

    private void OnComponentStartUp(EntityUid uid, DeafComponent component, ComponentStartup args)
    {
        if (_player.LocalEntity == uid)
            _audio.SetMasterGain(0);
    }
    private void OnDeafShutdown(EntityUid uid, DeafComponent component, ComponentShutdown args)
    {
        if (_player.LocalEntity == uid)
            _audio.SetMasterGain(_originalVolume);
    }

    private void OnPlayerDetached(EntityUid uid, DeafComponent component, LocalPlayerDetachedEvent args)
    {
        _audio.SetMasterGain(_originalVolume);
    }
    private void OnPlayerAttached(EntityUid uid, DeafComponent component, LocalPlayerAttachedEvent args)
    {
        _audio.SetMasterGain(0);
    }
}
