using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModSoundModuleSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    [SubscribeLocalEvent]
    private void OnSoundUsed(Entity<ModSoundModuleComponent> ent, ref ModModuleUsedEvent args)
    {
        _audio.PlayPredicted(ent.Comp.Sound, args.User, args.User);
        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnSpawnUsed(Entity<ModSpawnModuleComponent> ent, ref ModModuleUsedEvent args)
    {
        if (_net.IsServer)
            Spawn(ent.Comp.Prototype, Transform(args.User).Coordinates);

        args.Handled = true;
    }
}
