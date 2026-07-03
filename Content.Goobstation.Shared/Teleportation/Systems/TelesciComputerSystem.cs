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

        SubscribeLocalEvent<TelesciComputerComponent, TelesciSendMessage>(OnSendMessage);
        SubscribeLocalEvent<TelesciComputerComponent, TelesciRetrieveMessage>(OnRetrieveMessage);
        SubscribeLocalEvent<TelesciComputerComponent, TelesciCooldowneEvent>(OnCooldownEvent);
        SubscribeLocalEvent<TelesciComputerComponent, TelesciPositionMessage>(OnPositionMessage);
    }

    private void OnNewLink(Entity<TelesciComputerComponent> ent, ref NewLinkEvent arg)
    {
        if (!TryComp<TelesciTeleporterComponent>(arg.Sink, out var telepad))
            return;

        ent.Comp.TeleporterEntity = GetNetEntity(arg.Sink);
        telepad.Computer = ent;
        Dirty(arg.Sink, telepad);
        Dirty(ent);
    }

    private void OnPortDisconnected(Entity<TelesciComputerComponent> ent, ref PortDisconnectedEvent arg)
    {
        var teleporterNetEntity = ent.Comp.TeleporterEntity;
        if (arg.Port != ent.Comp.LinkingPort || teleporterNetEntity == null)
            return;

        var teleporterEntityUid = GetEntity(teleporterNetEntity);
        if (TryComp<TelesciTeleporterComponent>(teleporterEntityUid, out var telepad))
        {
            telepad.Computer = null;
            Dirty(teleporterEntityUid.Value, telepad);
        }

        ent.Comp.TeleporterEntity = null;
        Dirty(ent);
    }

    private void OnSendMessage(Entity<TelesciComputerComponent> ent, ref  TelesciSendMessage arg)
    {
        var teleporter = GetEntity(ent.Comp.TeleporterEntity);
        if (teleporter == null)
            return;

        ent.Comp.Position = arg.Coordinates;
        Dirty(ent);

        var ev = new TelesciSendEvent(arg.Coordinates);
        RaiseLocalEvent(teleporter.Value, ev);
    }

    private void OnRetrieveMessage(Entity<TelesciComputerComponent> ent, ref TelesciRetrieveMessage arg)
    {
        var teleporter = GetEntity(ent.Comp.TeleporterEntity);
        if (teleporter == null)
            return;

        ent.Comp.Position = arg.Coordinates;
        Dirty(ent);

        var ev = new TelesciRetrieveEvent(arg.Coordinates);
        RaiseLocalEvent(teleporter.Value, ev);
    }

    private void OnPositionMessage(Entity<TelesciComputerComponent> ent, ref TelesciPositionMessage arg)
    {
        ent.Comp.Position = arg.Coordinates;
        Dirty(ent);
    }

    private void OnCooldownEvent(Entity<TelesciComputerComponent> ent, ref TelesciCooldowneEvent arg)
    {
        ent.Comp.CooldownTime = arg.Cooldown;
        Dirty(ent);
    }
}
