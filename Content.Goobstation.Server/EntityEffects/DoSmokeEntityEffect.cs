// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics;
using System.Linq;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Spreader;
using Content.Shared.Chemistry.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects;

/// <summary>
///     Creates smoke similar to SmokeOnTrigger
/// </summary>
public sealed partial class DoSmokeEntityEffect : EntityEffect
{

    /// <summary>
    /// How long the smoke stays for, after it has spread.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Duration = 10;

    /// <summary>
    /// How much the smoke will spread.
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public int SpreadAmount;

    /// <summary>
    /// Smoke entity to spawn.
    /// Defaults to smoke but you can use foam if you want.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId SmokePrototype = "Smoke";

    /// <summary>
    /// Solution to add to each smoke cloud.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Solution Solution = new();

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;

    public override LogImpact LogImpact => LogImpact.Medium;

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not EntityEffectReagentArgs reagentArgs)
            return;

        var entityManager = args.EntityManager;
        var mapMan = IoCManager.Resolve<IMapManager>();
        var transformSys = entityManager.System<SharedTransformSystem>();
        var spreaderSys = entityManager.System<SpreaderSystem>();
        var smokeSys = entityManager.System<SmokeSystem>();

        if (!entityManager.TryGetComponent(args.TargetEntity, out TransformComponent? xform))
            return;

        var mapCoords = transformSys.GetMapCoordinates(args.TargetEntity, xform);


        if (!mapMan.TryFindGridAt(mapCoords, out _, out var grid)
            || !grid.TryGetTileRef(xform.Coordinates, out var tileRef)
            || tileRef.Tile.IsEmpty)
            return;

        if (spreaderSys.RequiresFloorToSpread(SmokePrototype.ToString()) && tileRef.Tile.IsEmpty)
            return;

        var coords = grid.MapToGrid(mapCoords);
        var ent = entityManager.SpawnAtPosition(SmokePrototype, coords.SnapToGrid());
        if (!entityManager.TryGetComponent<SmokeComponent>(ent, out var smoke))
        {
            entityManager.QueueDeleteEntity(ent);
            return;
        }

        smokeSys.StartSmoke(ent, Solution, Duration, SpreadAmount, smoke);
    }
}
