// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Goobstation.Shared.MisandryBox.Smites;

public sealed class FlipEyeSystem : ToggleableSmiteSystem<FlipEyeComponent>
{
    [Dependency] private readonly SharedContentEyeSystem _eyeSystem = default!;

    public override void Set(EntityUid owner)
    {
        EnsureComp<ContentEyeComponent>(owner, out var comp);
        _eyeSystem.SetZoom(owner, comp.TargetZoom * -1, ignoreLimits: true);
    }
}