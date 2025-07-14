// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Moomoobeef <62638182+Moomoobeef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Damage.Components;
using Content.Server.Destructible;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Rounding;
using Robust.Shared.Prototypes;

namespace Content.Server.Damage.Systems;

public sealed class ExaminableDamageSystem : EntitySystem
{
    [Dependency] private readonly DestructibleSystem _destructible = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ExaminableDamageComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<ExaminableDamageComponent> ent, ref ExaminedEvent args)
    {
        if (!_prototype.TryIndex(ent.Comp.Messages, out var proto) || proto.Values.Count == 0)
            return;

        var percent = GetDamagePercent(ent);
        var level = ContentHelpers.RoundToNearestLevels(percent, 1, proto.Values.Count - 1);
        var msg = Loc.GetString(proto.Values[level]);
        args.PushMarkup(msg, -99);
    }

    /// <summary>
    /// Returns a value between 0 and 1 representing how damaged the entity is,
    /// where 0 is undamaged and 1 is fully damaged.
    /// </summary>
    /// <returns>How damaged the entity is from 0 to 1</returns>
    private float GetDamagePercent(Entity<ExaminableDamageComponent> ent)
    {
        if (!TryComp<DamageableComponent>(ent, out var damageable))
            return 0;

        var damage = damageable.TotalDamage;
        var damageThreshold = _destructible.DestroyedAt(ent);

        if (damageThreshold == 0)
            return 0;

        return (damage / damageThreshold).Float();
    }
}