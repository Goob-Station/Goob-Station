// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Shared.CCVar;
using Content.Shared.Flash.Components;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Shared.Flash;

public sealed class SharedGoobFlashSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private  readonly IConfigurationManager _cfg = default!;

    private bool _checkDirection = true;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_cfg, GoobCVars.FlashDirectionCheck, value => _checkDirection = value, true);

        SubscribeLocalEvent<FlashComponent, CheckDirectionFlashEvent>(OnCheckDirection);
    }

    private void OnCheckDirection(Entity<FlashComponent> ent, ref CheckDirectionFlashEvent args)
    {
        if (args.Target == null || args.User == null || !_checkDirection)
            return;

        var user = args.User.Value;
        var target = args.Target.Value;

        var userXform = Transform(user);
        var targetXform = Transform(target);

        var userPos = _transformSystem.GetWorldPosition(userXform);
        var targetPos = _transformSystem.GetWorldPosition(targetXform);

        // Target’s forward facing vector
        var targetForward = _transformSystem.GetWorldRotation(targetXform).ToWorldVec();

        // Direction from target → user
        var toUser = (userPos - targetPos).Normalized();

        // Dot product check
        var targetFacingUser = Vector2.Dot(targetForward, toUser) > 0.7f; // ~45° cone

        if (!targetFacingUser)
            args.Cancelled = true;
    }
}
