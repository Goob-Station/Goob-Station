// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MisandryBox;

namespace Content.Goobstation.Server.MisandryBox;

public sealed class IgniteMultiplierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IgniteMultiplierComponent, BeforeIgniteFirestacksEvent>(OnIgnite);
    }

    private void OnIgnite(Entity<IgniteMultiplierComponent> ent, ref BeforeIgniteFirestacksEvent ev)
    {
        ev.FireStacks *= ent.Comp.Factor;
    }
}
