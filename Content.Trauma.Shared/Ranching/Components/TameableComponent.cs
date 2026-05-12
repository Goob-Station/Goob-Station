// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.NPC.Prototypes;

namespace Content.Trauma.Shared.Ranching.Components;

/// <summary>
/// This is used for making animals change their faction when petted (successfully interacted with) enough
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TameableComponent : Component
{
    [DataField]
    public int MinPetsRequired = 10;

    [DataField]
    public int MaxPetsRequired = 20;

    [DataField]
    public int PetsRequired;

    [DataField]
    public int Pets;

    [DataField]
    public bool ClearFactions = true;

    [DataField]
    public ProtoId<NpcFactionPrototype> Faction = "RaptorTamed";
}
