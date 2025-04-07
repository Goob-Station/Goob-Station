// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;

namespace Content.Server.Heretic.Abilities;

[RegisterComponent]
public sealed partial class HereticFlamesComponent : Component
{
    public float UpdateTimer = 0f;
    public float LifetimeTimer = 0f;
    [DataField] public float UpdateDuration = .2f;
    [DataField] public float LifetimeDuration = 60f;
}

public sealed partial class HereticFlamesSystem : EntitySystem
{
    [Dependency] private readonly HereticAbilitySystem _has = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<HereticFlamesComponent>();
        while (eqe.MoveNext(out var uid, out var hfc))
        {
            // remove it after ~60 seconds
            hfc.LifetimeTimer += frameTime;
            if (hfc.LifetimeTimer >= hfc.LifetimeDuration)
                RemCompDeferred(uid, hfc);

            // spawn fire box every .2 seconds
            hfc.UpdateTimer += frameTime;
            if (hfc.UpdateTimer >= hfc.UpdateDuration)
            {
                hfc.UpdateTimer = 0f;
                _has.SpawnFireBox(uid, 1, false);
            }
        }
    }
}