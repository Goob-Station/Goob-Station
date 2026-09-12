using Content.Goobstation.Common.VoxAudio;
using Content.Goobstation.Shared.VoxAudio;
using Content.Server.Communications;
using Content.Server.Station.Systems;
using Content.Shared.Station.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.VoxAudio;

public sealed partial class VoxAudioSystem : SharedVoxAudioSystem
{
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CommunicationConsoleAnnouncementEvent>(OnAnnouncement);
    }

    private void OnAnnouncement(ref CommunicationConsoleAnnouncementEvent ev)
    {
        if (!ev.Component.EnableVox)
            return; // sad

        // yay
        if (ev.Component.Global)
            Play(ev.Text, ev.Component.VoxVoices, 2.0f, 30f);
        else if (TryComp(_station.GetOwningStation(ev.Uid), out StationDataComponent? comp))
            Play(ev.Text, ev.Component.VoxVoices, 2.0f, 30f, null, _station.GetInStation(comp));
        else
            // vv if communication consoles work without depending on
            // the station itself in the future will need to change this.
            // shouldnt happen now tho..
            Log.Error("Announcing local communication console didn't belong to a station!");
    }

    public override void Play(string message, List<ProtoId<VoxVoicePrototype>> voiceProtoSet, float? delay = 0f,
        float? maxRuntime = null, EntityUid? uid = null, Filter? filter = null)
    {
        var msg = new VoxPlayMessage(message, voiceProtoSet, delay, GetNetEntity(uid));
        RaiseNetworkEvent(msg, filter ?? Filter.Broadcast());
    }
}
