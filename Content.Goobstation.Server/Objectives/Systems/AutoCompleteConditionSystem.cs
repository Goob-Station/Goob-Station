// SPDX-FileCopyrightText: 2025 SaffronFennec <firefoxwolf2020@protonmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Mind;

namespace Content.Server.Objectives.Systems;

// Automatically assign an objective as complete.

public sealed class AutoCompleteConditionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AutoCompleteConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(EntityUid uid, AutoCompleteConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = 1f;
    }
}
