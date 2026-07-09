// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Goobstation.Wizard.Components;
using Content.Server.Emp;
using Content.Shared.Emp;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class EmpImmuneSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EmpImmuneComponent, EmpAttemptEvent>(OnAttempt);
    }

    private void OnAttempt(Entity<EmpImmuneComponent> ent, ref EmpAttemptEvent args)
    {
        args.Cancelled = true;
    }
}
