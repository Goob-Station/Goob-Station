using Robust.Client.Audio;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Configuration;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.Traits;
using Content.Shared.CCVar;

namespace Content.Goobstation.Client.Traits;

public sealed class DeafnessSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IAudioManager _audio = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private float _originalVolume;

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
        {
            // Save the current volume before muting
            _originalVolume = _cfg.GetCVar(CCVars.AudioMasterVolume);
            _audio.SetMasterGain(0);
        }
    }

    private void OnDeafShutdown(EntityUid uid, DeafComponent component, ComponentShutdown args)
    {
        if (_player.LocalEntity == uid)
            _audio.SetMasterGain(_originalVolume);
    }

    private void OnPlayerDetached(EntityUid uid, DeafComponent component, LocalPlayerDetachedEvent args)
    {
        if (_player.LocalEntity == uid)
            _audio.SetMasterGain(_originalVolume);
    }

    private void OnPlayerAttached(EntityUid uid, DeafComponent component, LocalPlayerAttachedEvent args)
    {
        if (_player.LocalEntity == uid)
        {
            // Save volume when re-attaching too, in case it changed
            _originalVolume = _cfg.GetCVar(CCVars.AudioMasterVolume);
            _audio.SetMasterGain(0);
        }
    }
}
