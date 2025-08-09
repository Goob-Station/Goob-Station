// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Toggleable;

namespace Content.Goobstation.Shared.Skinnable;

public abstract partial class SharedSkinnableSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    protected void ChangeVisuals(Entity<SkinnableComponent> ent)
    {
        if (!TryComp<AppearanceComponent>(ent, out var appearance))
            return;

        _appearance.SetData(ent, ToggleVisuals.Toggled, true, appearance);
    }


}
