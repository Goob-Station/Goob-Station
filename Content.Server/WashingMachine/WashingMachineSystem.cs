// SPDX-FileCopyrightText: 2025 Doctor-Cpu <77215380+Doctor-Cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Will-Oliver-Br <164823659+Will-Oliver-Br@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Forensics;
using Content.Shared.WashingMachine;
using Content.Shared.Forensics.Components;

namespace Content.Server.WashingMachine;

public sealed partial class WashingMachineSystem : SharedWashingMachineSystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void UpdateForensics(Entity<WashingMachineComponent> ent, HashSet<EntityUid> items)
    {
        if (!TryComp<ForensicsComponent>(ent.Owner, out var forensics))
            return;

        foreach (var item in items)
        {
            if (!TryComp<FiberComponent>(item, out var fiber))
                continue;

            var fiberText = fiber.FiberColor == null
                ? Loc.GetString("forensic-fibers", ("material", fiber.FiberMaterial))
                : Loc.GetString("forensic-fibers-colored", ("color", fiber.FiberColor), ("material", fiber.FiberMaterial));

            forensics.Fibers.Add(fiberText);
        }
    }
}
