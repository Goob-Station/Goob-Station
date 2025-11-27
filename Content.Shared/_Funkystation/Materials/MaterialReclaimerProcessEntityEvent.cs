// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;

namespace Content.Shared._Funkystation.Materials;

// Raised when a material reclaimer processes an entity, allowing systems to intercept or handle.
// Handlers can set Handled = true to stop default processing.
public sealed partial class MaterialReclaimerProcessEntityEvent : EntityEventArgs
{
    public EntityUid Entity { get; }
    public bool Handled { get; set; }

    public MaterialReclaimerProcessEntityEvent(EntityUid entity)
    {
        Entity = entity;
    }
}
