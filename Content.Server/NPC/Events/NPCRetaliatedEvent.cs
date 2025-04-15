// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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
