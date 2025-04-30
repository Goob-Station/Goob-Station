// SPDX-FileCopyrightText: 2022 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 T-Stalker <43253663+DogZeroX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 T-Stalker <le0nel_1van@hotmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Damage.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Server.Weapons.Ranged.Components;

/// <summary>
/// Plays the specified sound upon receiving damage of that type.
/// </summary>
[RegisterComponent]
public sealed partial class RangedDamageSoundComponent : Component
{
    // TODO: Limb damage changing sound type.

    /// <summary>
    /// Specified sounds to apply when the entity takes damage with the specified group.
    /// Will fallback to defaults if none specified.
    /// </summary>
    [DataField("soundGroups",
        customTypeSerializer: typeof(PrototypeIdDictionarySerializer<SoundSpecifier, DamageGroupPrototype>))]
    public Dictionary<string, SoundSpecifier>? SoundGroups;

    /// <summary>
    /// Specified sounds to apply when the entity takes damage with the specified type.
    /// Will fallback to defaults if none specified.
    /// </summary>
    [DataField("soundTypes",
         customTypeSerializer: typeof(PrototypeIdDictionarySerializer<SoundSpecifier, DamageTypePrototype>))]
    public Dictionary<string, SoundSpecifier>? SoundTypes;
}