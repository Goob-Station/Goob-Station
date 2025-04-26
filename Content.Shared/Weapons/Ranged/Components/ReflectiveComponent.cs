// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Weapons.Reflect;
using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
/// Can this entity be reflected.
/// Only applies if it is shot like a projectile and not if it is thrown.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ReflectiveComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("reflective")]
    public ReflectType Reflective = ReflectType.NonEnergy;
}