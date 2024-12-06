using Content.Server.Chat;
using Content.Server._Goobstation.Announcements.Systems; // Goobstation - Custom Announcers
using Robust.Shared.Player; // Goobstation - Custom Announcers

namespace Content.Server.Chat.Systems;

public sealed class AnnounceOnSpawnSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly AnnouncerSystem _announcer = default!; // Goobstation - Custom Announcers

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnnounceOnSpawnComponent, MapInitEvent>(OnInit);
    }

    private void OnInit(EntityUid uid, AnnounceOnSpawnComponent comp, MapInitEvent args)
    {
        var message = Loc.GetString(comp.Message);
        var sender = comp.Sender != null ? Loc.GetString(comp.Sender) : Loc.GetString("chat-manager-sender-announcement");
        _announcer.SendAnnouncement(_announcer.GetAnnouncementId("SpawnAnnounceCaptain"), Filter.Broadcast(),
            comp.Message, sender, comp.Color); // Goobstation - Custom Announcers
    }
}
