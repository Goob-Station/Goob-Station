// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Stunnable;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.Shared._White;

public sealed class TelefragSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public void DoTelefrag(EntityUid uid,
        EntityCoordinates coordinates,
        TimeSpan stunTime = default,
        float stunRadius = 1.5f,
        bool autoStandUp = false)
    {
        var ents = _lookup.GetEntitiesInRange(coordinates, stunRadius);

        foreach (var ent in ents)
        {
            if (ent == uid)
                continue;

            // Use Wizden's knockdown system instead of manual standing manipulation
            if (stunTime > TimeSpan.Zero)
            {
                _stun.TryKnockdown(ent, stunTime, true, autoStandUp);
            }
            else
            {
                // Default knockdown for immediate effect
                _stun.TryKnockdown(ent, TimeSpan.FromSeconds(3), true, autoStandUp);
            }
        }
    }
}