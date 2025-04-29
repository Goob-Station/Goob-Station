// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Shitmed.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server._Shitmed.Objectives.Systems;

public sealed class RoleplayObjectiveSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoleplayObjectiveComponent, ObjectiveGetProgressEvent>(OnRoleplayGetProgress);
    }

    private void OnRoleplayGetProgress(EntityUid uid, RoleplayObjectiveComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = 1f;
    }
}