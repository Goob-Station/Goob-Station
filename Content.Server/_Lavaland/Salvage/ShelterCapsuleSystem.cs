// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Lavaland.Procedural.Components;
using Content.Server.Fluids.EntitySystems;
using Content.Server.GridPreloader;
using Content.Shared._Lavaland.Shelter;
using Content.Shared.Chemistry.Components;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._Lavaland.Salvage;

public sealed class ShelterCapsuleSystem : SharedShelterCapsuleSystem
{
    [Dependency] private readonly GridPreloaderSystem _preloader = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShelterCapsuleComponent, ShelterCapsuleDeployDoAfterEvent>(OnDoAfter);
    }

    private void OnDoAfter(EntityUid uid, ShelterCapsuleComponent component, ShelterCapsuleDeployDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = TryDeployShelterCapsule((uid, component));

        if (args.Handled)
            QueueDel(uid);
    }

    public bool TryDeployShelterCapsule(Entity<ShelterCapsuleComponent> ent)
    {
        if (TerminatingOrDeleted(ent))
            return false;

        var xform = Transform(ent);
        var comp = ent.Comp;
        var proto = _protoMan.Index(comp.PreloadedGrid);
        var worldPos = _transform.GetMapCoordinates(ent, xform);

        if (!CheckCanDeploy(ent) || xform.MapUid == null)
            return false;

        // Load and place shelter
        var path = proto.Path;
        var mapEnt = xform.MapUid.Value;
        var posFixed = new MapCoordinates((worldPos.Position + comp.Offset).Rounded(), worldPos.MapId);

        // Smoke
        var foamEnt = Spawn("Smoke", worldPos);
        var spreadAmount = (int) Math.Round(comp.BoxSize.Length() * 2);
        _smoke.StartSmoke(foamEnt, new Solution(), comp.DeployTime + 2f, spreadAmount);

        if (!_preloader.TryGetPreloadedGrid(comp.PreloadedGrid, out var shelter))
        {
            _mapSystem.CreateMap(out var dummyMap);
            if (!_mapLoader.TryLoadGrid(dummyMap, path, out var shelterEnt))
            {
                Log.Error("Failed to load Shelter grid properly on it's deployment.");
                return false;
            }

            SetupShelter(shelterEnt.Value.Owner, new EntityCoordinates(mapEnt, posFixed.Position));
            _mapSystem.DeleteMap(dummyMap);
            return true;
        }

        SetupShelter(shelter.Value, new EntityCoordinates(mapEnt, posFixed.Position));
        return true;
    }

    private void SetupShelter(Entity<TransformComponent?> shelter, EntityCoordinates coords)
    {
        if (!Resolve(shelter, ref shelter.Comp))
            return;

        _transform.SetCoordinates(shelter,
            shelter.Comp,
            coords,
            Angle.Zero);
        EnsureComp<LavalandMemberComponent>(shelter);
    }
}