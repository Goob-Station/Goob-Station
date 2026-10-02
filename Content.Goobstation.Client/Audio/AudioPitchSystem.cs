using Content.Goobstation.Shared.Audio;
using Robust.Client.Player;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Sources;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Audio;

/// <summary>
/// Changes the pitch of everything that has this.
/// </summary>
public sealed class AudioPitchSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AudioPitchComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<AudioPitchComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnShutdown(Entity<AudioPitchComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Owner == _player.LocalEntity)
            SetPitch(1f);
    }

    private void OnPlayerDetached(Entity<AudioPitchComponent> ent, ref LocalPlayerDetachedEvent args) =>
        SetPitch(1f);

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_player.LocalEntity is { } player && TryComp<AudioPitchComponent>(player, out var comp))
            SetPitch(comp.Pitch);
    }

    private void SetPitch(float pitch)
    {
        var query = AllEntityQuery<AudioComponent>();
        while (query.MoveNext(out _, out var audio))
        {
            if (audio.Loaded && audio.LifeStage < ComponentLifeStage.Stopping)
                ((IAudioSource) audio).Pitch = audio.Params.Pitch * pitch;
        }
    }
}
