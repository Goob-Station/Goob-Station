// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Mobs.Hierophant.Components;

[RegisterComponent]
public sealed partial class HierophantChaserComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)] public EntityUid? Target = null;

    /// <summary>
    ///     Acts as a divisor for <see cref="BaseCooldown"/>
    /// </summary>
    [DataField] public float Speed = 4.5f;

    [DataField] public float MaxSteps = 20f;
    [ViewVariables(VVAccess.ReadWrite)] public float Steps = 0f;

    [DataField] public float BaseCooldown = 1f;
    [ViewVariables(VVAccess.ReadWrite)] public float CooldownTimer = 1f; // = BaseCooldown

    /// <summary>
    ///     What will it spawn alongside itself?
    /// </summary>
    [DataField(required: true)] public EntProtoId SpawnPrototype;
}