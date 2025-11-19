// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 amogus <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Objectives.Systems;
using Content.Shared.Objectives.Components;

namespace Content.Goobstation.Server.Pirates.Objectives;

public sealed partial class PirateObjectiveSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ObjectivePlunderComponent, ObjectiveGetProgressEvent>(GetPlunderProgress);
    }

    /// <summary>
    ///     Objective gets updated in <see cref=""/>
    /// </summary>
    private void GetPlunderProgress(Entity<ObjectivePlunderComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        var tgt = _number.GetTarget(ent);
        if (tgt != 0)
            args.Progress = MathF.Min(ent.Comp.Plundered / tgt, 1f);
        else args.Progress = 1f;
    }
}