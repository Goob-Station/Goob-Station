// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Emp;
using Robust.Shared.Random;

namespace Content.Client.Emp;

public sealed class EmpSystem : SharedEmpSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<EmpDisabledComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var transform))
        {
            if (Timing.CurTime > comp.TargetTime)
            {
                comp.TargetTime = Timing.CurTime + _random.NextFloat(0.8f, 1.2f) * TimeSpan.FromSeconds(comp.EffectCooldown);
                Spawn(EmpDisabledEffectPrototype, transform.Coordinates);
            }
        }
    }
}