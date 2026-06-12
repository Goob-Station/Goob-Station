// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

namespace Content.Shared._Funkystation.Materials;

/// <summary>
/// Raised on a material reclaimer when it processes an entity, allowing systems to intercept.
/// Handlers can set Handled = true to stop default processing (Goobstation - MalfAI robotics factory).
/// </summary>
public sealed partial class MaterialReclaimerProcessEntityEvent : EntityEventArgs
{
    public EntityUid Entity { get; }
    public bool Handled { get; set; }

    public MaterialReclaimerProcessEntityEvent(EntityUid entity)
    {
        Entity = entity;
    }
}
