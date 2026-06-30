using Content.Goobstation.Shared.Teleportation.Components;
using Content.Shared.DeviceLinking.Events;

namespace Content.Goobstation.Shared.Teleportation.Systems;


public sealed class TelesciComputerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TelesciComputerComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<TelesciComputerComponent, PortDisconnectedEvent>(OnPortDisconnected);

        SubscribeLocalEvent<TelesciComputerComponent,TelesciSendMessage>(OnSendMessage);
        SubscribeLocalEvent<TelesciComputerComponent,TelesciRetriveMessage>(OnRetriveMessage);
        SubscribeLocalEvent<TelesciComputerComponent,TelesciCooldowneEvent>(OnCooldownEvent);
    }


    private void OnNewLink(Entity<TelesciComputerComponent> ent, ref NewLinkEvent arg)
    {
        if (!TryComp<TelesciTeleporterComponent>(arg.Sink, out var teleporter))
            return;

        ent.Comp.TeleporterEntity = GetNetEntity(arg.Sink);
        teleporter.Console = ent;
        Dirty(arg.Sink, teleporter);
        Dirty(ent);
    }

    private void OnPortDisconnected(Entity<TelesciComputerComponent> ent, ref PortDisconnectedEvent arg)
    {
        var teleporterNetEntity = ent.Comp.TeleporterEntity;
        if (arg.Port != ent.Comp.LinkingPort || teleporterNetEntity == null)
            return;

        var teleporterEntityUid = GetEntity(teleporterNetEntity);
        if (TryComp<TelesciTeleporterComponent>(teleporterEntityUid, out var telerport))
        {
            telerport.Console = null;
            Dirty(teleporterEntityUid.Value, telerport);
        }

        ent.Comp.TeleporterEntity = null;
        Dirty(ent);
    }

    private void OnSendMessage(Entity<TelesciComputerComponent> ent,ref  TelesciSendMessage arg)
    {
        var teleporter = GetEntity(ent.Comp.TeleporterEntity);
        if( teleporter == null)
            return;

        ent.Comp.X = arg.Cordinates.X;
        ent.Comp.Y = arg.Cordinates.Y;
        Dirty(ent);

        var ev = new TelesciSendEvent(arg.Cordinates);
        RaiseLocalEvent(teleporter.Value, ev);
    }

    private void OnRetriveMessage(Entity<TelesciComputerComponent> ent, ref TelesciRetriveMessage arg)
    {
        var teleporter = GetEntity(ent.Comp.TeleporterEntity);
        if( teleporter == null)
            return;

        ent.Comp.X = arg.Cordinates.X;
        ent.Comp.Y = arg.Cordinates.Y;
        Dirty(ent);

        var ev = new TelesciRetriveEvent(arg.Cordinates);
        RaiseLocalEvent(teleporter.Value, ev);
    }

    private void OnCooldownEvent(Entity<TelesciComputerComponent> ent, ref TelesciCooldowneEvent arg)
    {
        ent.Comp.CooldownTime = arg.Cooldown;
        Dirty(ent);
    }

}
