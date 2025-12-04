using Content.Goobstation.Shared.StationRadio.Components;
using Content.Goobstation.Shared.StationRadio.Events;
using Content.Shared.Destructible;
using Robust.Shared.Audio;
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
        base.Initialize();
        SubscribeLocalEvent<VinylPlayerComponent, EntInsertedIntoContainerMessage>(OnVinylInserted);
        SubscribeLocalEvent<VinylPlayerComponent, EntRemovedFromContainerMessage>(OnVinylRemove);
        SubscribeLocalEvent<VinylPlayerComponent, DestructionEventArgs>(OnDestruction);
    }

    private void OnDestruction(EntityUid uid, VinylPlayerComponent comp, DestructionEventArgs args)
    {
        if(!comp.RelayToRadios)
            return;

        var query = EntityQueryEnumerator<StationRadioReceiverComponent>();
        while (query.MoveNext(out var receiver, out var _))
        {
            RaiseLocalEvent(receiver, new StationRadioMediaStoppedEvent());
        }
    }

    private void OnVinylInserted(EntityUid uid, VinylPlayerComponent comp, EntInsertedIntoContainerMessage args)
    {
        if (!TryComp(args.Entity, out VinylComponent? vinylcomp) || _net.IsClient || vinylcomp.Song == null)
            return;

        var audio = _audio.PlayPredicted(vinylcomp.Song, uid, uid, AudioParams.Default.WithVolume(3f).WithMaxDistance(4.5f));
        if (audio != null)
            comp.SoundEntity = audio.Value.Entity;

        if(!comp.RelayToRadios)
            return;

        var query = EntityQueryEnumerator<StationRadioReceiverComponent>();
        while (query.MoveNext(out var receiver, out var _))
        {
            RaiseLocalEvent(receiver, new StationRadioMediaPlayedEvent(vinylcomp.Song));
        }
    }

    private void OnVinylRemove(EntityUid uid, VinylPlayerComponent comp, EntRemovedFromContainerMessage args)
    {
        if(comp.SoundEntity != null)
            _audio.Stop(comp.SoundEntity);

        if(!comp.RelayToRadios)
            return;

        var query = EntityQueryEnumerator<StationRadioReceiverComponent>();
        while (query.MoveNext(out var receiver, out var _))
        {
            RaiseLocalEvent(receiver, new StationRadioMediaStoppedEvent());
        }
    }
}
