// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking.Rules.VariationPass.Components;
using Content.Server.Wires;
using Content.Shared.Whitelist;
using Robust.Shared.Random;

namespace Content.Server.GameTicking.Rules.VariationPass;

/// <summary>
/// Handles cutting a random wire on random devices around the station.
/// This system identifies target devices and adds <see cref="CutWireOnMapInitComponent"/> to them.
/// The actual wire cutting is handled by <see cref="CutWireOnMapInitSystem"/>.
/// </summary>
public sealed class CutWireVariationPassSystem : VariationPassSystem<CutWireVariationPassComponent>
{
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    protected override void ApplyVariation(Entity<CutWireVariationPassComponent> ent, ref StationVariationPassEvent args)
    {
        var wiresCut = 0;
        var query = AllEntityQuery<WiresComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var transform))
        {
            // Ignore if not part of the station
            if (!IsMemberOfStation((uid, transform), ref args))
                continue;

            // Check against blacklist
            if (_whitelistSystem.IsBlacklistPass(ent.Comp.Blacklist, uid))
                continue;

            if (Random.Prob(ent.Comp.WireCutChance))
            {
                EnsureComp<CutWireOnMapInitComponent>(uid);
                wiresCut++;

                // Limit max wires cut
                if (wiresCut >= ent.Comp.MaxWiresCut)
                    break;
            }
        }
    }
}