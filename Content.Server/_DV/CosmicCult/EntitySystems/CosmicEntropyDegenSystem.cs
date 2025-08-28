// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 OnsenCapy <101037138+OnsenCapy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 loltart <lo1tartyt@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.CosmicCult.Components;
using Robust.Shared.Timing;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Robust.Shared.Random;
using Content.Shared._Shitmed.Targeting; // Shitmed Change
namespace Content.Server._DV.CosmicCult.EntitySystems;

/// <summary>
/// Makes the person with this component take damage over time.
/// Used for status effect.
/// </summary>
public sealed partial class CosmicEntropyDegenSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CosmicEntropyDebuffComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<CosmicEntropyNonCultistComponent, ComponentStartup>(OnInitNonCultist); // Goobstation change. For non-cultist equipment debuff
    }

    private void OnInit(EntityUid uid, CosmicEntropyDebuffComponent comp, ref ComponentStartup args)
    {
        comp.CheckTimer = _timing.CurTime + comp.CheckWait;
    }

    // Goobstation change. For non-cultist equipment debuff
    private void OnInitNonCultist(EntityUid uid, CosmicEntropyNonCultistComponent comp, ref ComponentStartup args)
    {
        comp.CheckTimer = _timing.CurTime + comp.CheckWait;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CosmicEntropyDebuffComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (_timing.CurTime < component.CheckTimer)
                continue;

            component.CheckTimer = _timing.CurTime + component.CheckWait;
            _damageable.TryChangeDamage(uid, component.Degen, true, false, targetPart: TargetBodyPart.All);
        }

        // Goobstation change. For non-cultist equipment Debuff
        var nonCultistQuery = EntityQueryEnumerator<CosmicEntropyNonCultistComponent>();
        while (nonCultistQuery.MoveNext(out var uid, out var component))
        {
            if (_timing.CurTime < component.CheckTimer)
                continue;

            component.CheckTimer = _timing.CurTime + component.CheckWait;
            _damageable.TryChangeDamage(uid, component.Degen, true, false, targetPart: TargetBodyPart.All);
        }

    }
}
