// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Blob;
using Content.Goobstation.Shared.Blob.Components;
using Content.Server.Emp;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Blob.Systems;

public sealed class BlobbernautSystem : SharedBlobbernautSystem
{
    [Dependency] private readonly EntityLookupSystem _entityLookupSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var blobFactoryQuery = EntityQueryEnumerator<BlobbernautComponent, MobStateComponent>();
        while (blobFactoryQuery.MoveNext(out var ent, out var comp, out var mobStateComponent))
        {
            if (_mobStateSystem.IsDead(ent,mobStateComponent))
                continue;

            comp.NextDamage += frameTime;

            if (comp.DamageFrequency > comp.NextDamage)
                continue;

            comp.NextDamage -= comp.DamageFrequency;

            if (TerminatingOrDeleted(comp.Factory))
            {
                TryChangeDamage("blobberaut-factory-destroy", ent, comp.Damage);
                continue;
            }

            var xform = Transform(ent);

            if (xform.GridUid == null)
                continue;

            var mapPos = _transform.ToMapCoordinates(xform.Coordinates);

            _entitySet.Clear();
            _entityLookupSystem.GetEntitiesInRange(mapPos.MapId, mapPos.Position, 1f, _entitySet);

            if(_entitySet.Count != 0)
                continue;

            TryChangeDamage("blobberaut-not-on-blob-tile", ent, comp.Damage);
        }
    }

    private DamageSpecifier? TryChangeDamage(string msg, EntityUid ent, DamageSpecifier dmg)
    {
        _popup.PopupEntity(Loc.GetString(msg), ent, ent, PopupType.LargeCaution);
        return _damageableSystem.TryChangeDamage(ent, dmg);
    }
}
