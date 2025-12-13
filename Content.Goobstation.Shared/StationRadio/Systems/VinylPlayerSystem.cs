using Content.Goobstation.Shared.StationRadio.Components;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.DeviceNetwork;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.StationRadio.Systems;

public sealed class VinylPlayerSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedDeviceLinkSystem _link = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VinylPlayerComponent, EntityTerminatingEvent>(OnDelete);
        SubscribeLocalEvent<VinylPlayerComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<VinylPlayerComponent, EntInsertedIntoContainerMessage>(OnVinylInserted);
        SubscribeLocalEvent<VinylPlayerComponent, EntRemovedFromContainerMessage>(OnVinylRemove);
        SubscribeLocalEvent<VinylPlayerComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<VinylPlayerComponent, PortDisconnectedEvent>(OnDisconnect);
    }

    private void OnPowerChanged(Entity<VinylPlayerComponent> ent, ref PowerChangedEvent args)
    {
        SetAudioState(ent, !args.Powered ? AudioState.Paused : AudioState.Playing);
    }

    private void OnNewLink(Entity<VinylPlayerComponent> ent, ref NewLinkEvent args)
    {
        if (args.SourcePort != ent.Comp.ServerPort)
            return;

        ent.Comp.ServerEntity = args.Sink;
    }

    private void OnDisconnect(Entity<VinylPlayerComponent> ent, ref PortDisconnectedEvent args)
    {
        if (args.Port != ent.Comp.ServerPort)
            return;

        ent.Comp.ServerEntity = null;
    }

    private void OnDelete(Entity<VinylPlayerComponent> ent, ref EntityTerminatingEvent args)
    {
        StopAudio(ent);
    }

    private void OnVinylInserted(Entity<VinylPlayerComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        StartAudio(ent, args.Entity);
    }

    private void OnVinylRemove(Entity<VinylPlayerComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        StopAudio(ent);
    }

    public void StartAudio(Entity<VinylPlayerComponent> ent, Entity<VinylComponent?> vinyl)
    {
        var (uid, comp) = ent;
        if (_net.IsClient
            || comp.RequiresPower
            && !_power.IsPowered(uid)
            || !Resolve(vinyl.Owner, ref vinyl.Comp, false)
            || vinyl.Comp.Song == null)
            return;

        var audio = _audio.PlayPredicted(vinyl.Comp.Song, uid, uid, AudioParams.Default.WithVolume(3f).WithMaxDistance(4.5f));
        if (audio != null)
            comp.SoundEntity = audio.Value.Entity;

        if (ent.Comp.ServerEntity == null)
            return;

        var payload = new NetworkPayload()
        {
            [DeviceNetworkConstants.Command] = StationRadioSystem.PlayAudioCommand,
            [StationRadioSystem.AudioPathData] = vinyl.Comp.Song,
        };

        _link.InvokePort(ent.Owner, ent.Comp.ServerPort, payload);
    }

    public void StopAudio(Entity<VinylPlayerComponent> ent)
    {
        ent.Comp.SoundEntity = _audio.Stop(ent.Comp.SoundEntity);

        if (ent.Comp.ServerEntity == null)
            return;

        var payload = new NetworkPayload()
        {
            [DeviceNetworkConstants.Command] = StationRadioSystem.StopAudioCommand
        };

        _link.InvokePort(ent.Owner, ent.Comp.ServerPort, payload);
    }

    public void SetAudioState(Entity<VinylPlayerComponent> ent, AudioState state)
    {
        _audio.SetState(ent.Comp.SoundEntity, state);

        if (ent.Comp.ServerEntity == null)
            return;

        var payload = new NetworkPayload()
        {
            [DeviceNetworkConstants.Command] = StationRadioSystem.SetAudioStateCommand,
            [StationRadioSystem.AudioStateData] = state,
        };

        _link.InvokePort(ent.Owner, ent.Comp.ServerPort, payload);
    }
}
