// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.MisandryBox.Smites;

public abstract class ToggleableSmiteSystem<T> : EntitySystem where T : Component
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<T, ComponentStartup>(OnInit);
        SubscribeLocalEvent<T, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(Entity<T> ent, ref ComponentStartup args)
    {
        Set(ent.Owner);
    }

    private void OnShutdown(Entity<T> ent, ref ComponentShutdown args)
    {
        Set(ent.Owner);
    }

    public abstract void Set(EntityUid owner);
}