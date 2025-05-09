// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.ChronoLegionnaire.Components;
using Content.Shared.Actions;

namespace Content.Goobstation.Shared.ChronoLegionnaire.EntitySystems;

public abstract class SharedStasisBlinkProviderSystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StasisBlinkProviderComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StasisBlinkProviderComponent, GetItemActionsEvent>(OnGetItemActions);
    }

    private void OnMapInit(Entity<StasisBlinkProviderComponent> provider, ref MapInitEvent args)
    {
        var comp = provider.Comp;

        _actionContainer.EnsureAction(provider, ref comp.BlinkActionEntity, comp.BlinkAction);

        Dirty(provider, comp);
    }

    private void OnGetItemActions(Entity<StasisBlinkProviderComponent> provider, ref GetItemActionsEvent args)
    {
        var comp = provider.Comp;

        args.AddAction(ref comp.BlinkActionEntity, comp.BlinkAction);
    }
}