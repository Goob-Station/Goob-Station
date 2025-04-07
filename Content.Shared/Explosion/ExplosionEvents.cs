// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Damage;
using Content.Shared.Inventory;

namespace Content.Shared.Explosion;

/// <summary>
///     Raised directed at an entity to determine its explosion resistance, probably right before it is about to be
///     damaged by one.
/// </summary>
[ByRefEvent]
public record struct GetExplosionResistanceEvent(string ExplosionPrototype) : IInventoryRelayEvent
{
    /// <summary>
    ///     A coefficient applied to overall explosive damage.
    /// </summary>
    public float DamageCoefficient = 1;

    public readonly string ExplosionPrototype = ExplosionPrototype;

    SlotFlags IInventoryRelayEvent.TargetSlots =>  ~SlotFlags.POCKET;
}

/// <summary>
/// This event is raised directed at an entity that is about to receive damage from an explosion. It can be used to
/// recursively add contained/child entities that should also receive damage. E.g., entities in a player's inventory
/// or backpack. This event will be raised recursively so a matchbox in a backpack in a player's inventory
/// will also receive this event.
/// </summary>
[ByRefEvent]
public record struct BeforeExplodeEvent(DamageSpecifier Damage, string Id, List<EntityUid> Contents)
{
    /// <summary>
    /// The damage that will be received by this entity. Note that the entity's explosion resistance has already been
    /// used to modify this damage.
    /// </summary>
    public readonly DamageSpecifier Damage = Damage;

    /// <summary>
    /// ID of the explosion prototype.
    /// </summary>
    public readonly string Id = Id;

    /// <summary>
    /// Damage multiplier for modifying the damage that will get dealt to contained entities.
    /// </summary>
    public float DamageCoefficient = 1;

    /// <summary>
    /// Contained/child entities that should receive recursive explosion damage.
    /// </summary>
    public readonly List<EntityUid> Contents = Contents;
}