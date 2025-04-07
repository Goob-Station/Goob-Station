// goobstation - entire file; goobmod moment
using Content.Server.NPC.Components;

namespace Content.Server.NPC.Events;

public sealed class NPCRetaliatedEvent : EntityEventArgs
{
    public readonly Entity<NPCRetaliationComponent> Ent;
    public readonly EntityUid Against;
    public readonly bool Secondary;

    public NPCRetaliatedEvent(Entity<NPCRetaliationComponent> ent, EntityUid against, bool secondary)
    {
        Ent = ent;
        Against = against;
        Secondary = secondary;
    }
}
