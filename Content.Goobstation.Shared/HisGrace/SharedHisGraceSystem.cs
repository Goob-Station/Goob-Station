// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Item;
using Content.Shared.Toggleable;

namespace Content.Goobstation.Shared.HisGrace;

public abstract partial class SharedHisGraceSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly SharedItemSystem _item = null!;

    protected virtual void VisualsChanged(Entity<HisGraceComponent> ent, string key)
    {

    }

    protected void DoAscensionVisuals(Entity<HisGraceComponent> ent, string key)
    {
        if (TryComp<AppearanceComponent>(ent, out var appearance))
            _appearance.SetData(ent, ToggleVisuals.Toggled, true, appearance);
        _item.SetHeldPrefix(ent, key);

        VisualsChanged(ent, key);
    }
}
