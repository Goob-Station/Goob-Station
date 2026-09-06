using Content.Goobstation.Shared.Audio;
using Robust.Client.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Audio;

/// <summary>
/// Changes the Reverb of everything that has this.
/// </summary>
public sealed class AudioReverbSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AudioReverbComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<AudioReverbComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnShutdown(Entity<AudioReverbComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Owner == _player.LocalEntity)
            Restore(ent.Comp.Preset);
    }

    private void OnPlayerDetached(Entity<AudioReverbComponent> ent, ref LocalPlayerDetachedEvent args) =>
        Restore(ent.Comp.Preset);

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_player.LocalEntity is not { } player
            || !TryComp<AudioReverbComponent>(player, out var comp)
            || !_audio.Auxiliaries.TryGetValue(comp.Preset, out var slot))
            return;

        var query = AllEntityQuery<AudioComponent>();
        while (query.MoveNext(out var uid, out var audio))
        {
            if (audio.Loaded && audio.Auxiliary != slot)
                _audio.SetAuxiliary(uid, audio, slot);
        }
    }

    private void Restore(ProtoId<AudioPresetPrototype> preset)
    {
        if (!_audio.Auxiliaries.TryGetValue(preset, out var slot))
            return;

        var query = AllEntityQuery<AudioComponent>();
        while (query.MoveNext(out var uid, out var audio))
        {
            if (audio.Auxiliary == slot)
                _audio.SetAuxiliary(uid, audio, null);
        }
    }
}
