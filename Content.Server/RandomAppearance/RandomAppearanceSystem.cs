// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Random;

namespace Content.Server.RandomAppearance;

public sealed class RandomAppearanceSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomAppearanceComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(EntityUid uid, RandomAppearanceComponent component, ComponentInit args)
    {
        if (TryComp(uid, out AppearanceComponent? appearance) && component.EnumKey != null)
        {
            _appearance.SetData(uid, component.EnumKey, _random.Pick(component.SpriteStates), appearance);
        }
    }
}