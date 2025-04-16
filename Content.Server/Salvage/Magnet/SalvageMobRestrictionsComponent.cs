// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Salvage.Magnet;

// This is dumb
/// <summary>
/// Deletes the attached entity if the linked entity is deleted.
/// </summary>
[RegisterComponent]
public sealed partial class SalvageMobRestrictionsComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntityUid LinkedEntity;
}