// SPDX-FileCopyrightText: 2026 Jonikibaka <153797633+Jonikibaka@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.StationAi;
using Robust.Shared.Prototypes;

namespace Content.Shared.Silicons.StationAi;

// Goob-MalfAi: some station AI internals are access-restricted or private to this system,
// so the Malfunction AI features go through these helpers.
public abstract partial class SharedStationAiSystem
{
    /// <summary>
    /// Reconfigures a vision source (e.g. a camera): whether walls block it and how far it sees.
    /// Used by the Malfunction AI camera network upgrade (see-through-walls + extended range).
    /// </summary>
    public void SetVisionUpgrade(Entity<StationAiVisionComponent> entity, bool occluded, float range)
    {
        entity.Comp.Occluded = occluded;
        entity.Comp.Range = range;
        Dirty(entity);
    }

    /// <summary>
    /// Forces the core iconography of a held AI to the given customization prototype.
    /// Used by the Malfunction AI to reveal its true face when Doomsday is armed.
    /// </summary>
    public void SetCoreIconography(EntityUid held, ProtoId<StationAiCustomizationPrototype> protoId)
    {
        var custom = EnsureComp<StationAiCustomizationComponent>(held);
        custom.ProtoIds[_stationAiCoreCustomGroupProtoId] = protoId;
        Dirty(held, custom);

        if (_containers.TryGetContainingContainer(held, out var container)
            && TryComp<StationAiHolderComponent>(container.Owner, out var holder))
        {
            UpdateAppearance((container.Owner, holder));
        }
    }
}
