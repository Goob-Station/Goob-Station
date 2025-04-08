// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Random;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
///     Simply provides a certain capacity of entities that cannot be reloaded through normal means and have
///     no special behavior like cycling, magazine
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BasicEntityAmmoProviderComponent : AmmoProviderComponent
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("proto", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? Proto; // Goob edit

    // Goobstation
    [DataField]
    public ProtoId<WeightedRandomEntityPrototype>? Prototypes;

    /// <summary>
    ///     Max capacity.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("capacity")]
    [AutoNetworkedField]
    public int? Capacity = null;

    /// <summary>
    ///     Actual ammo left. Initialized to capacity unless they are non-null and differ.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("count")]
    [AutoNetworkedField]
    public int? Count = null;
}