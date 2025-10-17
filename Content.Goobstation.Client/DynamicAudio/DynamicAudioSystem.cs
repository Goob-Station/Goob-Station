using Content.Goobstation.Shared.DynamicAudio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.DynamicAudio;

public sealed class DynamicAudioSystem : SharedDynamicAudioSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AudioComponent, ComponentAdd>(OnAudioAdd);
        SubscribeLocalEvent<DynamicAudioComponent, ComponentStartup>(OnEffectedAudioStartup);
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

        ApplyAudioEffect((ent, audio));
    }
}
