// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Heretic;
using Content.Shared.Maps;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Map.Components;
using Content.Server._Goobstation.Heretic.EntitySystems.PathSpecific;

namespace Content.Server.Magic;

public sealed partial class ImmovableVoidRodSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prot = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly VoidCurseSystem _voidcurse = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // we are deliberately including paused entities. rod hungers for all
        foreach (var (rod, trans) in EntityManager.EntityQuery<ImmovableVoidRodComponent, TransformComponent>(true))
        {
            rod.Accumulator += frameTime;

            if (rod.Accumulator > rod.Lifetime.TotalSeconds)
            {
                QueueDel(rod.Owner);
                return;
            }

            if (!_ent.TryGetComponent<MapGridComponent>(trans.GridUid, out var grid))
                continue;



            var tileref = grid.GetTileRef(trans.Coordinates);
            var tile = _prot.Index<ContentTileDefinition>("FloorAstroSnow");
            _tile.ReplaceTile(tileref, tile);
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ImmovableVoidRodComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(Entity<ImmovableVoidRodComponent> ent, ref StartCollideEvent args)
    {
        if ((TryComp<HereticComponent>(args.OtherEntity, out var th) && th.CurrentPath == "Void")
        || HasComp<GhoulComponent>(args.OtherEntity))
            return;

        var power = 1f;
        if (ent.Comp.User != null && ent.Comp.User.Value.Comp.CurrentPath == "Void")
            // ascended void heretic will give 6 SECONDS OF STUN :bluesurprised:
            power += ent.Comp.User.Value.Comp.PathStage / 2f;

        _stun.TryParalyze(args.OtherEntity, TimeSpan.FromSeconds(power), false);
        _voidcurse.DoCurse(args.OtherEntity);

        TryComp<TagComponent>(args.OtherEntity, out var tag);
        var tags = tag?.Tags ?? new();

        if (tags.Contains("Wall") && Prototype(args.OtherEntity) != null && Prototype(args.OtherEntity)!.ID != "WallSnowCobblebrick")
        {
            Spawn("WallSnowCobblebrick", Transform(args.OtherEntity).Coordinates);
            QueueDel(args.OtherEntity);
        }
    }
}