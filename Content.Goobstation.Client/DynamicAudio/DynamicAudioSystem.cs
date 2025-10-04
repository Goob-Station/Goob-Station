using Content.Client.Atmos.Components;
using Content.Goobstation.Shared.DynamicAudio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.DynamicAudio;

public sealed class DynamicAudioSystem : SharedDynamicAudioSystem
{
    [Dependency] private readonly SharedDynamicAudioSystem _dynamicAudio = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AudioComponent, ComponentAdd>(OnAudioAdd);
        SubscribeLocalEvent<DynamicAudioComponent, ComponentStartup>(OnEffectedAudioStartup, after: [typeof(SharedAudioSystem)]);
    }

    private void OnAudioAdd(Entity<AudioComponent> ent, ref ComponentAdd args)
    {
        if (!_playerManager.LocalEntity.HasValue)
            return;

        if (!TryComp<EyeComponent>(_playerManager.LocalEntity.Value, out var eye))
            return;

        if (!eye.DrawFov)
            return;

        EnsureComp<DynamicAudioComponent>(ent);
    }

    private void OnEffectedAudioStartup(Entity<DynamicAudioComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<AudioComponent>(ent.Owner, out var audio))
            return;

        if (TerminatingOrDeleted(ent) || Paused(ent) || audio.Global)
            return;

        _dynamicAudio.ApplyAudioEffect((ent, audio));
    }
}
