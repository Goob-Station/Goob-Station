// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Heretic.Abilities.HereticFlames;
public sealed partial class HereticFlamesSystem : EntitySystem
{
    [Dependency] private readonly HereticAbilitySystem _has = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HereticFlamesComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // remove it after ~60 seconds
            comp.LifetimeTimer += frameTime;
            if (comp.LifetimeTimer >= comp.LifetimeDuration)
                RemCompDeferred(uid, comp);

            // spawn fire box every .2 seconds
            comp.UpdateTimer += frameTime;
            if (comp.UpdateTimer >= comp.UpdateDuration)
            {
                comp.UpdateTimer = 0f;
                _has.SpawnFireBox(uid, 1, false);
            }
        }
    }
}
