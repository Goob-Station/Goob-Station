using Content.Shared._CorvaxGoob.DynamicAudio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Client._CorvaxGoob.DynamicAudio;

public sealed class DynamicAudioSystem : EntitySystem
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
        if (!_playerManager.LocalEntity.HasValue
            || !TryComp<EyeComponent>(_playerManager.LocalEntity.Value, out var eye)
            || !eye.DrawFov)
            return;

        EnsureComp<DynamicAudioComponent>(ent);
    }

    private void OnEffectedAudioStartup(Entity<DynamicAudioComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<AudioComponent>(ent.Owner, out var audio) ||
            TerminatingOrDeleted(ent)
            || Paused(ent)
            || audio.Global)
            return;

        _dynamicAudio.ApplyAudioEffect((ent, audio));
    }
}
