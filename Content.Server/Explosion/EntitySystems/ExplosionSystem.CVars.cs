// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.CCVar;

namespace Content.Server.Explosion.EntitySystems;

public sealed partial class ExplosionSystem
{
    public int MaxIterations { get; private set; }
    public int MaxArea { get; private set; }
    public float MaxProcessingTime { get; private set; }
    public int TilesPerTick { get; private set; }
    public int ThrowLimit { get; private set; }
    public bool SleepNodeSys { get; private set; }
    public bool IncrementalTileBreaking { get; private set; }
    public int SingleTickAreaLimit { get; private set; }
    public bool CanCreateVacuum { get; private set; }

    private void SubscribeCvars()
    {
        Subs.CVar(_cfg, CCVars.ExplosionTilesPerTick, value => TilesPerTick = value, true);
        Subs.CVar(_cfg, CCVars.ExplosionThrowLimit, value => ThrowLimit = value, true);
        Subs.CVar(_cfg, CCVars.ExplosionSleepNodeSys, value => SleepNodeSys = value, true);
        Subs.CVar(_cfg, CCVars.ExplosionMaxArea, value => MaxArea = value, true);
        Subs.CVar(_cfg, CCVars.ExplosionMaxIterations, value => MaxIterations = value, true);
        Subs.CVar(_cfg, CCVars.ExplosionMaxProcessingTime, value => MaxProcessingTime = value, true);
        Subs.CVar(_cfg, CCVars.ExplosionIncrementalTileBreaking, value => IncrementalTileBreaking = value, true);
        Subs.CVar(_cfg, CCVars.ExplosionSingleTickAreaLimit, value => SingleTickAreaLimit = value, true);
        Subs.CVar(_cfg, CCVars.ExplosionCanCreateVacuum, value => CanCreateVacuum = value, true);
    }
}