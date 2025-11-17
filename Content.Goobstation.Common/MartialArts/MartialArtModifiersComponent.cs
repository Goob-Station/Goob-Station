// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MartialArts.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MartialArtModifiersComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public List<MartialArtModifierData> Data = new();

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    // Vector4 is (min_multiplier, max_multiplier, min_modifier, max_modifier)
    [DataField]
    public Dictionary<MartialArtModifierType, Vector4> MinMaxModifiersMultipliers = new()
    {
        { MartialArtModifierType.AttackRate, new Vector4(0.5f, 3f, -4f, 4f)},
         // Flat negative modifiers will be just clamped to zero so it's fine (probably)
        { MartialArtModifierType.Damage, new Vector4(0.5f, 3f, -20f, 20f)},
        // No modifiers for move speed are supported
        { MartialArtModifierType.MoveSpeed, new Vector4(0.2f, 3f, 0f, 0f)},
        // No modifiers for healing are supported
        { MartialArtModifierType.Healing, new Vector4(0f, 10f, 0f, 0f)},
    };
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MartialArtModifierData
{
    public MartialArtModifierType Type = MartialArtModifierType.AttackRate;

    public float Multiplier = 1f;

    public float Modifier;

    public TimeSpan EndTime = TimeSpan.Zero;
}

[Flags]
public enum MartialArtModifierType : byte
{
    Invalid = 0,
    AttackRate = 1 << 0,
    Damage = 1 << 1,
    MoveSpeed = 1 << 2,
    // Healing is not supported currently for martial arts, make custom code for it or add support yourself
    Healing = 1 << 3,
    // Add more if needed
    Unarmed = 1 << 4,
    Armed = 1 << 5,
}
