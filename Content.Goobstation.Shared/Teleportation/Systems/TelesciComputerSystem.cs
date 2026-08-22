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

        ent.Comp.TeleporterUid = arg.Sink;
        telepad.Computer = ent;
        Dirty(arg.Sink, telepad);
        Dirty(ent);
    }

    private void OnPortDisconnected(Entity<TelesciComputerComponent> ent, ref PortDisconnectedEvent arg)
    {

        if (arg.Port != ent.Comp.LinkingPort || ent.Comp.TeleporterUid == null)
            return;

        if (TryComp<TelesciTeleporterComponent>(ent.Comp.TeleporterUid, out var telepad))
        {
            telepad.Computer = null;
            Dirty(ent.Comp.TeleporterUid.Value, telepad);
        }

        ent.Comp.TeleporterUid = null;
        Dirty(ent);
    }

    private void OnSendMessage(Entity<TelesciComputerComponent> ent, ref  TelesciSendMessage arg)
    {
        if (ent.Comp.TeleporterUid == null)
            return;

        ent.Comp.Position = arg.Coordinates;
        Dirty(ent);
        var ev = new TelesciSendEvent(arg.Coordinates);
        RaiseLocalEvent(ent.Comp.TeleporterUid.Value, ev);
    }

    private void OnRetrieveMessage(Entity<TelesciComputerComponent> ent, ref TelesciRetrieveMessage arg)
    {
        if (ent.Comp.TeleporterUid == null)
            return;

        ent.Comp.Position = arg.Coordinates;
        Dirty(ent);

        var ev = new TelesciRetrieveEvent(arg.Coordinates);
        RaiseLocalEvent(ent.Comp.TeleporterUid.Value, ev);
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
