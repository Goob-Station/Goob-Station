using Content.Goobstation.Shared.StationRadio.Components;
using Content.Goobstation.Shared.StationRadio.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.StationRadio.Systems;

public sealed class VinylPlayerSystem : EntitySystem
{

    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<VinylPlayerComponent, EntInsertedIntoContainerMessage>(OnVinylInserted);
        SubscribeLocalEvent<VinylPlayerComponent, EntRemovedFromContainerMessage>(OnVinylRemove);
    }

    private void OnVinylRemove(EntityUid uid, VinylPlayerComponent comp, EntRemovedFromContainerMessage args)
    {
        if(comp.SoundEntity != null)
            _audio.Stop(comp.SoundEntity);

        RaiseLocalEvent(new StationRadioMediaStoppedEvent());
    }

    private void OnVinylInserted(EntityUid uid, VinylPlayerComponent comp, EntInsertedIntoContainerMessage args)
    {
        if (!TryComp(args.Entity, out VinylComponent? vinylcomp) || _net.IsClient)
            return;

        var audio = _audio.PlayPredicted(vinylcomp.Song, uid, args.Entity);
        if (audio != null)
            comp.SoundEntity = audio.Value.Entity;

        RaiseLocalEvent(new StationRadioMediaPlayedEvent(vinylcomp.Song));
    }
}
